using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddStatusPopup : AddEntryPopup<Status> {
    public class PopupData {
        public BasePopupData BasePopupData;
        public string Description;
        public bool Positive;
    }

    [SerializeField] private TMP_InputField _descriptionInput = default;
    [SerializeField] private Toggle _positive = default;

    protected override void OnPopulated() {
        if (Editing) {
            _descriptionInput.text = _editingEntry.Description;
            _positive.isOn = _editingEntry.Positive;
        }
    }

    protected override void OnClear() {
        _descriptionInput.text = "";
        _positive.isOn = false;
    }

    protected override IDataEntry OnEntryCreation() {
        Status status = new Status() {
            Name = NewName,
            Description = _descriptionInput.text,
            Positive = _positive.isOn
        };

        return status;
    }
    
    public override object GetRestorationData() {
        PopupData popupData = new PopupData {
            BasePopupData = base.GetRestorationData() as BasePopupData,
            Description = _descriptionInput.text,
            Positive = _positive.isOn
        };

        return popupData;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            base.Restore(popupData.BasePopupData);
            _descriptionInput.text = popupData.Description;
            _positive.isOn = popupData.Positive;
        }
    }
}
