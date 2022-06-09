using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListPopup : Popup {
    public class PopupData {
        public Func<List<InformationData>> Data;
        public string Title;
        public Action OnConfirm;
        public InformationList.ScrollData Scroll;
    }

    [SerializeField] private TextMeshProUGUI _title = default;
    [SerializeField] private InformationList _infoList = default;
    [SerializeField] private Button _confirmButton = default;
    [SerializeField] private Button _closeButton = default;
    private Func<List<InformationData>> _data;
    private Action _onConfirm;
    public InformationList.ScrollData GetScrollData() => _infoList.GetScrollData();

    private void Awake() {
        _confirmButton.onClick.AddListener(() => {
            _onConfirm?.Invoke();
            PopupManager.Instance.Back();
        });
        _closeButton.onClick.AddListener(PopupManager.Instance.Back);
    }

    public void Populate(
        Func<List<InformationData>> data, 
        string title, 
        Action onConfirm, 
        InformationList.ScrollData scrollData = null
    ) {
        _title.text = title;
        _data = data;
        _onConfirm = onConfirm;
        _confirmButton.gameObject.SetActive(_onConfirm != null);

        _infoList.Populate(data?.Invoke(), scrollData);
    }

    public override object GetRestorationData() {
        PopupData popupData = new PopupData {
            Data = _data,
            Title = _title.text,
            OnConfirm = _onConfirm,
            Scroll = _infoList.GetScrollData()
        };

        return popupData;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            Populate(popupData.Data, popupData.Title, popupData.OnConfirm, popupData.Scroll);
        }
    }
}