using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListPopup : Popup {
    public class PopupData {
        public Func<List<InformationData>> DataRetriever;
        public string Title;
        public Action OnConfirm;
        public Func<IReadOnlyList<ButtonData>> ButtonsRetriever;
        public InformationList.ScrollData Scroll;
    }

    [SerializeField] private TextMeshProUGUI _title = default;
    [SerializeField] private InformationList _infoList = default;
    [SerializeField] private ButtonList _buttonList = default;
    [SerializeField] private Button _confirmButton = default;
    [SerializeField] private Button _closeButton = default;
    private Func<List<InformationData>> _dataRetriever;
    private Func<IReadOnlyList<ButtonData>> _buttonRetriever;
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
        Func<List<InformationData>> dataRetriever, 
        string title, 
        Action onConfirm = null, 
        Func<IReadOnlyList<ButtonData>> buttonsRetriever = null, 
        InformationList.ScrollData scrollData = null
    ) {
        _title.text = title;
        _dataRetriever = dataRetriever;
        _onConfirm = onConfirm;
        _buttonRetriever = buttonsRetriever;
        _confirmButton.gameObject.SetActive(_onConfirm != null);
        _buttonList.gameObject.SetActive(buttonsRetriever != null);
        _buttonList.Populate(buttonsRetriever?.Invoke());

        _infoList.Populate(dataRetriever?.Invoke(), scrollData);
    }

    public override object GetRestorationData() {
        PopupData popupData = new PopupData {
            DataRetriever = _dataRetriever,
            Title = _title.text,
            OnConfirm = _onConfirm,
            ButtonsRetriever = _buttonRetriever,
            Scroll = _infoList.GetScrollData()
        };

        return popupData;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            Populate(
                popupData.DataRetriever,
                popupData.Title,
                popupData.OnConfirm,
                popupData.ButtonsRetriever,
                popupData.Scroll
            );
        }
    }
}