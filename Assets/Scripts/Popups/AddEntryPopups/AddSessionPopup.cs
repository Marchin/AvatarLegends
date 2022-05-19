using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class AddSessionPopup : AddEntryPopup<Session> {

    public class PopupData {
        public BasePopupData BasePopupData;
        public string Description;
    }

    [SerializeField] private TMP_InputField _descriptionInput = default;

    protected override void OnPopulated() {
        if (Editing) {
            _descriptionInput.text = _editingEntry.Description;
        }
    }

    protected override void OnClear() {
        _descriptionInput.text = "";
    }

    protected override IDataEntry OnEntryCreation() {
        Session session = new Session() {
            Name = NewName,
            Description = _descriptionInput.text,
            Number = ApplicationManager.Instance.Data.User.SelectedCampaign.Sessions.Count + 1
        };

        if (Editing) {
            session.NPCs = _editingEntry.NPCs;
            session.PCs = _editingEntry.PCs;
            session.Engagements = _editingEntry.Engagements;
            session.Note = _editingEntry.Note;
        } else {
            session.PCs = new List<string>(ApplicationManager.Instance.Data.User.SelectedCampaign.PCs.Keys);
        }

        return session;
    }

    public override object GetRestorationData() {
        PopupData popupData = new PopupData {
            BasePopupData = base.GetRestorationData() as BasePopupData,
            Description = _descriptionInput.text,
        };

        return popupData;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            base.Restore(popupData.BasePopupData);
            _descriptionInput.text = popupData.Description;
        }
    }
}
