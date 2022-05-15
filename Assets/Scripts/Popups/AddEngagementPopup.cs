using TMPro;
using UnityEngine;

public class AddEngagementPopup : AddEntryPopup<Engagement> {
    public class PopupData {
        public BasePopupData BasePopupData;
        public string Note;
    }

    [SerializeField] private TMP_InputField _noteInput = default;

    protected override void OnPopulated() {
        if (Editing) {
            _noteInput.text = _editingEntry.Note;;
        }
    }

    protected override void OnClear() {
        _noteInput.text = "";
    }

    protected override IDataEntry OnEntryCreation() {
        Engagement engagement = new Engagement() {
            Name = NewName,
            Note = _noteInput.text
        };

        if (Editing) {
            engagement.NPCs = _editingEntry.NPCs;
        }

        return engagement;
    }

    public override object GetRestorationData() {
        PopupData popupData = new PopupData {
            BasePopupData = base.GetRestorationData() as BasePopupData,
            Note = _noteInput.text
        };

        return popupData;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            base.Restore(popupData.BasePopupData);
            _noteInput.text = popupData.Note;
        }
    }
}