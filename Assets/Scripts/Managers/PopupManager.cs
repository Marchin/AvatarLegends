using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

public class PopupRestorationData {
    public Type PopupType;
    public bool IsFullScreen;
    public object Data;
}

public class PopupManager : MonoBehaviourSingleton<PopupManager> {
    private List<Popup> _stack = new List<Popup>();
    private List<AsyncOperationHandle<GameObject>> _handles = new List<AsyncOperationHandle<GameObject>>();
    private GameObject _parentCanvas;
    public event Action OnStackChange;
    public event Action OverrideBack;
    private List<PopupRestorationData> _restorationData = new List<PopupRestorationData>();
    public bool ClosingPopup { get; private set; }
    public bool ReloadingPopups { get; private set; }
    private AsyncOperationHandle<IList<GameObject>> _popupsHandle;
    public Popup ActivePopup {
        get {
            foreach (var popup in _stack) {
                if (popup.gameObject.activeSelf) {
                    return popup;
                }
            }

            return null;
        }
    }
    
    private void Awake() {
        SceneManager.activeSceneChanged += (prev, next) => {
            foreach (var handle in _handles) {
                Addressables.ReleaseInstance(handle);
            }
            _handles.Clear();
            _stack.Clear();
        };

        _parentCanvas = UnityUtils.GetOrGenerateRootGO("Popup Canvas");
        _parentCanvas.GetOrAddComponent<Canvas>();
    }

    private void RemovePopup(int index = 0) {
        _stack.RemoveAt(index);
        Addressables.ReleaseInstance(_handles[index]);
        _handles.RemoveAt(index);
    }

    public async UniTask<T> GetOrLoadPopup<T>(bool restore = true, bool track = true) where T : Popup {
        var lockHandle = ApplicationManager.Instance.LockScreen.Subscribe();
        T popup = null;
        if (track && ActivePopup != null) {
            var activePopup = ActivePopup;
            PopupRestorationData restorationData = new PopupRestorationData {
                PopupType = activePopup.GetType(),
                IsFullScreen = activePopup.FullScreen,
                Data = restore ? activePopup.GetRestorationData() : null,
            };
            _restorationData.Insert(0, restorationData);
        }

        while (_stack.Count > 0 && !_stack[0].gameObject.activeSelf) {
            if (_stack[0].GetType() == typeof(T)) {
                popup = _stack[0] as T;
                popup.gameObject.SetActive(true);
            } else {
                _stack[0].OnClose();
                RemovePopup();
            }
        }

        if (popup == null) {
            int popupIndex = _stack.FindIndex(0, _stack.Count, popup => popup.GetType() == typeof(T));
            while (popupIndex > 0) {
                _stack[0].OnClose();
                RemovePopup();
                --popupIndex;
            }

            if (popupIndex >= 0) {
                Debug.Assert(_stack[0] is T, "The found popup type doesn't correspond with the request");
                popup = _stack[0] as T;
                popup.gameObject.SetActive(true);
            } else {
                string popupName = typeof(T).Name;
                var loadingHandle = ApplicationManager.Instance.ShowLoadingScreen.Subscribe();
                var handle = Addressables.InstantiateAsync(popupName, _parentCanvas.transform);
                _handles.Insert(0, handle);
                popup = (await handle).GetComponent<T>();
                Debug.Assert(popup!= null);
                RectTransform popupRect = (popup.transform as RectTransform);
                popupRect.AdjustToSafeZone();
                _stack.Insert(0, popup);
                loadingHandle.Finish();
            }
        }

        OnStackChange?.Invoke();

        lockHandle.Finish();

        return popup;
    }

    public T GetLoadedPopupOfType<T>() where T : Popup {
        T popup = _stack.Find(p => p.GetType() == typeof(T)) as T;

        return popup;
    }

    public async UniTask Back() {
        if (ClosingPopup) {
            Debug.LogWarning("A popup is already being closed");
            return;
        }
        
        if (OverrideBack != null && OverrideBack.GetInvocationList().Length > 0) {
            OverrideBack?.Invoke();
            return;
        }

        if (ActivePopup != null) {
            ClosingPopup = true;
            int startingIndex = -1;
            if (_restorationData.Count > 0) {
                startingIndex = 0;

                while ((startingIndex + 1) < _restorationData.Count && !_restorationData[startingIndex].IsFullScreen) {
                    ++startingIndex;
                }
                // while (startingIndex > 0 && _restorationData[startingIndex].Vertical == IsScreenOnPortrait) {
                //     --startingIndex;
                // }
            }
            
            if (startingIndex >= 0) {
                var handle = ApplicationManager.Instance.ShowLoadingScreen.Subscribe();
                while (startingIndex >= 0) {
                    PopupRestorationData restorationData = _restorationData[startingIndex];

                    if (restorationData.Data != null) {
                        await RestorePopup(restorationData);
                    } else {
                        CloseActivePopup();
                    }
                    
                    if (startingIndex == 0) {
                        _restorationData.RemoveAt(startingIndex);
                    } else {
                        _restorationData[startingIndex].Data = null;
                    }
                    --startingIndex;
                }
                handle.Finish();
            } else {
                CloseActivePopup();
            }

            ClosingPopup = false;
            OnStackChange?.Invoke();
        } else {
            UnityUtils.Quit();
        }
    }

    public void ClearStackUntilPopup<T>() where T : Popup {
        if (ActivePopup == null || ActivePopup.GetType() == typeof(T)) {
            return;
        }

        while ((_restorationData.Count > 0) && _restorationData[0].PopupType != typeof(T)) {
            _restorationData.RemoveAt(0);
        }

        if (_restorationData.Count > 0) {
            Debug.Assert(_restorationData[0].PopupType == typeof(T), "Something went wrong while clearing stack");
            while ((ActivePopup != null) && (ActivePopup.GetType() != typeof(T))) {
                CloseActivePopup();
            }
        }

        OnStackChange?.Invoke();
    }
    
    private void CloseActivePopup() {
        foreach (var popup in _stack) {
            if (popup.gameObject.activeSelf) {
                popup.OnClose();
                popup.gameObject.SetActive(false);
                break;
            }
        }
    }

    private async UniTask RestorePopup(PopupRestorationData restorationData) {
        MethodInfo method = this.GetType().GetMethod(nameof(GetOrLoadPopup));
        MethodInfo generic = method.MakeGenericMethod(restorationData.PopupType);
        var task = generic.Invoke(this, new object[] { false, false });
        var awaiter = task.GetType().GetMethod("GetAwaiter").Invoke(task, null);
        await UniTask.WaitUntil(() =>
            (awaiter.GetType().GetProperty("IsCompleted").GetValue(awaiter) as bool?) ?? true);
        var result = awaiter.GetType().GetMethod("GetResult");
        Popup popup = result.Invoke(awaiter, null) as Popup;
        popup.Restore(restorationData.Data);
    }

}
