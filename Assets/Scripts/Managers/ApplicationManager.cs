using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

public class ApplicationManager : MonoBehaviourSingleton<ApplicationManager> {
    [SerializeField] private GameObject _loadingScreen = default;
    [SerializeField] private GameObject _loadingWheel = default;
    [SerializeField] private GameObject _inputLock = default;
    public OperationBySubscription ShowLoadingScreen { get; private set; }
    public OperationBySubscription ShowLoadingWheel { get; private set; }
    public OperationBySubscription DisableBackButton { get; private set; }
    public OperationBySubscription LockScreen { get; private set; }
    private OperationBySubscription.Subscription _loadingWheelSubscription;
    private AppData _appData;
    public bool Initialized { get; private set; }
    
    private async void Start() {
        ShowLoadingScreen = new OperationBySubscription(
            onStart: () => {
                _loadingScreen.SetActive(true);
                _loadingWheelSubscription = ShowLoadingWheel.Subscribe();
            },
            onAllFinished: () => {
                _loadingScreen.SetActive(false);
                _loadingWheelSubscription.Finish();
            }
        );

        ShowLoadingWheel = new OperationBySubscription(
            onStart: () => _loadingWheel.SetActive(true),
            onAllFinished: () => _loadingWheel.SetActive(false)
        );

        DisableBackButton = new OperationBySubscription(null, null);

        LockScreen = new OperationBySubscription(
            onStart: () => _inputLock.SetActive(true),
            onAllFinished: () => _inputLock.SetActive(false)
        );

        await Addressables.InitializeAsync();

        var data = await Addressables.LoadAssetAsync<TextAsset>("Data");
        _appData = JsonConvert.DeserializeObject<AppData>(data.text) ?? new AppData();

        // UserDataManager.Instance.Sync().Forget();
        var mainPopup = await PopupManager.Instance.GetOrLoadPopup<MainPopup>();
        mainPopup.Populate(_appData);

        Initialized = true;
    }

    private void Update() {
        if (!DisableBackButton.IsRunning && Input.GetKeyDown(KeyCode.Escape)) {
            _ = PopupManager.Instance.Back();
        }
    }

    private void OnDestroy() {
        _appData?.RecordData();
    }
}