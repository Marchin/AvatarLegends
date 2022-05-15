using TMPro;
using UnityEngine;

public class AddCampaignPopup : AddEntryPopup<Campaign> {
    public class PopupData {
        public BasePopupData BasePopupData;
        public string Description;
    }

    [SerializeField] private TMP_InputField _descriptionInput = default;

    protected override void OnPopulated() {
        if (Editing) {
            _descriptionInput.text = _editingEntry.Note;;
        }
    }

    protected override void OnClear() {
        _descriptionInput.text = "";
    }

    protected override IDataEntry OnEntryCreation() {
        Campaign campaign = new Campaign() {
            Name = NewName,
            Description = _descriptionInput.text
        };

        if (Editing) {
            campaign.NPCs = _editingEntry.NPCs;
            campaign.Sessions = _editingEntry.Sessions;
            campaign.Note = _editingEntry.Note;
        }

        return campaign;
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
