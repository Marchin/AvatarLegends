using TMPro;
using UnityEngine;

public class AddConditionPopup : AddEntryPopup<Condition> {
    public class PopupData {
        public BasePopupData BasePopupData;
        public string Effect;
        public string ClearingCondition;
    }

    [SerializeField] private TMP_InputField _effectInput = default;
    [SerializeField] private TMP_InputField _clearingConditionInput = default;

    protected override void OnPopulated() {
        if (Editing) {
            _effectInput.text = _editingEntry.Effect;
            _clearingConditionInput.text = _editingEntry.ClearingCondition;
        }
    }

    protected override void OnClear() {
        _effectInput.text = "";
        _clearingConditionInput.text = "";
    }

    protected override IDataEntry OnEntryCreation() {
        Condition session = new Condition() {
            Name = NewName,
            Effect = _effectInput.text,
            ClearingCondition = _clearingConditionInput.text
        };

        return session;
    }

    public override object GetRestorationData() {
        PopupData popupData = new PopupData {
            BasePopupData = base.GetRestorationData() as BasePopupData,
            Effect = _effectInput.text,
            ClearingCondition = _clearingConditionInput.text
        };

        return popupData;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            base.Restore(popupData.BasePopupData);
            _effectInput.text = popupData.Effect;
            _clearingConditionInput.text = popupData.ClearingCondition;
        }
    }
}
