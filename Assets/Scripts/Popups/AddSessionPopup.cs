using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddSessionPopup : Popup {

    public class PopupData {
        public string Name;
        public string Description;
        public Action<IDataEntry> OnDone;
        public ICollection<string> Names;
        public Session EditingSession;
    }

    [SerializeField] private TMP_InputField _nameInput = default;
    [SerializeField] private TMP_InputField _descriptionInput = default;
    [SerializeField] private Button _confirmButton = default;
    [SerializeField] private Button _closeButton = default;
    private Action<IDataEntry> OnDone;
    private ICollection<string> _names;
    private Session _editingSession;
    private bool Editing => _editingSession != null;

    private void Awake() {
        _confirmButton.onClick.AddListener(CreateSession);
        _closeButton.onClick.AddListener(() => _ = PopupManager.Instance.Back());
    }

    public void Populate(Action<IDataEntry> onDone, ICollection<string> names, Session editingSession = null) {
        OnDone = onDone;
        this._names = names;
        _editingSession = editingSession;
        Clear();

        if (Editing) {
            _nameInput.text = editingSession.Name;
            _descriptionInput.text = editingSession.Note;;
        }
    }

    private void Clear() {
        _nameInput.text = "";
        _descriptionInput.text = "";
    }

    private async void CreateSession() {
        if (string.IsNullOrEmpty(_nameInput.text) || 
            (!Editing && _names.Contains(_nameInput.text))
        ) {
            var msgPopup = await PopupManager.Instance.GetOrLoadPopup<MessagePopup>();
            msgPopup.Populate(
                _names.Contains(_nameInput.text) ? "Name already exists." : "Please enter a name.",
                "Name");
            return;
        }

        Session session = new Session() {
            Name = _nameInput.text,
            Description = _descriptionInput.text,
            Number = ApplicationManager.Instance.Data.User.SelectedCampaign.Sessions.Count + 1
        };

        if (Editing) {
            session.NPCs = _editingSession.NPCs;
            session.Engagements = _editingSession.Engagements;
            session.Note = _editingSession.Note;
        }

        OnDone.Invoke(session);
        _ = PopupManager.Instance.Back();
    }

    public override object GetRestorationData() {
        PopupData popupData = new PopupData {
            Name = _nameInput.text,
            Description = _descriptionInput.text,
            OnDone = OnDone,
            Names = _names,
            EditingSession = _editingSession
        };

        return popupData;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            Populate(popupData.OnDone, popupData.Names, popupData.EditingSession);
            _nameInput.text = popupData.Name;
            _descriptionInput.text = popupData.Description;
        }
    }
}
