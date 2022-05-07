using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListPopup : Popup {
    public class PopupData {
        public List<InformationData> Data;
        public string Title;
        public Action OnConfirm;
    }

    [SerializeField] private TextMeshProUGUI _title = default;
    [SerializeField] private InformationList _infoList = default;
    [SerializeField] private Button _confirmButton = default;
    [SerializeField] private Button _closeButton = default;
    private List<InformationData> _data;
    private Action _onConfirm;

    private void Awake() {
        _confirmButton.onClick.AddListener(() => {
            _onConfirm();
            _ = PopupManager.Instance.Back();
        });
        _closeButton.onClick.AddListener(() => _ = PopupManager.Instance.Back());
    }

    public void Populate(List<InformationData> data, string title, Action onConfirm) {
        _title.text = title;
        _data = data;
        _onConfirm = onConfirm;

        _infoList.Populate(data);
    }

    public override object GetRestorationData() {
        PopupData popupData = new PopupData {
            Data = _data,
            Title = _title.text,
            OnConfirm = _onConfirm
        };

        return popupData;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            Populate(popupData.Data, popupData.Title, popupData.OnConfirm);
        }
    }
}