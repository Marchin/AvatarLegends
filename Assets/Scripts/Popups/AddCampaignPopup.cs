using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddCampaignPopup : Popup {

    public class PopupData {
        public string Name;
        public string Description;
        public Action<IDataEntry> OnDone;
        public ICollection<string> Names;
        public Campaign EditingCampaign;
    }

    [SerializeField] private TMP_InputField _nameInput = default;
    [SerializeField] private TMP_InputField _descriptionInput = default;
    [SerializeField] private Button _confirmButton = default;
    [SerializeField] private Button _closeButton = default;
    private Action<IDataEntry> OnDone;
    private ICollection<string> _names;
    private Campaign _editingCampaign;
    private bool Editing => _editingCampaign != null;

    private void Awake() {
        _confirmButton.onClick.AddListener(CreateCampaign);
        _closeButton.onClick.AddListener(() => _ = PopupManager.Instance.Back());
    }

    public void Populate(Action<IDataEntry> onDone, ICollection<string> names, Campaign editingCampaign = null) {
        OnDone = onDone;
        this._names = names;
        _editingCampaign = editingCampaign;
        Clear();

        if (Editing) {
            _nameInput.text = editingCampaign.Name;
            _descriptionInput.text = editingCampaign.Note;;
        }
    }

    private void Clear() {
        _nameInput.text = "";
        _descriptionInput.text = "";
    }

    private async void CreateCampaign() {
        if (string.IsNullOrEmpty(_nameInput.text) || 
            (!Editing && _names.Contains(_nameInput.text))
        ) {
            var msgPopup = await PopupManager.Instance.GetOrLoadPopup<MessagePopup>();
            msgPopup.Populate(
                _names.Contains(_nameInput.text) ? "Name already exists." : "Please enter a name.",
                "Name");
            return;
        }

        Campaign campaign = new Campaign() {
            Name = _nameInput.text,
            Description = _descriptionInput.text
        };

        if (Editing) {
            campaign.NPCs = _editingCampaign.NPCs;
            campaign.Sessions = _editingCampaign.Sessions;
            campaign.Note = _editingCampaign.Note;
        }

        OnDone.Invoke(campaign);
        _ = PopupManager.Instance.Back();
    }

    public override object GetRestorationData() {
        PopupData popupData = new PopupData {
            Name = _nameInput.text,
            Description = _descriptionInput.text,
            OnDone = OnDone,
            Names = _names,
            EditingCampaign = _editingCampaign
        };

        return popupData;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            Populate(popupData.OnDone, popupData.Names, popupData.EditingCampaign);
            _nameInput.text = popupData.Name;
            _descriptionInput.text = popupData.Description;
        }
    }
}
