public class AddEngagementPopup : AddEntryPopup<Engagement> {
    public class PopupData {
        public BasePopupData BasePopupData;
    }

    private AppData Data => ApplicationManager.Instance.Data;
    private Session CurrentSession => Data.User.CurrentSession;


    protected override void OnPopulated() {
        if (!Editing) {
            NewName = $"Engagement {(CurrentSession.Engagements.Count + 1)}";
        }
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