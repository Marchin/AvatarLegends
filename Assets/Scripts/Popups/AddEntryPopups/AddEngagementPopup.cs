public class AddEngagementPopup : AddEntryPopup<Engagement> {
    public class PopupData {
        public BasePopupData BasePopupData;
    }


    protected override void OnPopulated() {
    }

    protected override void OnClear() {
    }

    protected override IDataEntry OnEntryCreation() {
        Engagement engagement = new Engagement() {
            Name = NewName
        };

        if (Editing) {
            engagement.NPCs = _editingEntry.NPCs;
            engagement.PCs = _editingEntry.PCs;
            engagement.Note = _editingEntry.Note;
        }

        return engagement;
    }

    public override object GetRestorationData() {
        PopupData popupData = new PopupData {
            BasePopupData = base.GetRestorationData() as BasePopupData
        };

        return popupData;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            base.Restore(popupData.BasePopupData);
        }
    }
}