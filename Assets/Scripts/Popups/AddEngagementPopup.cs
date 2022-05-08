using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddEngagementPopup : Popup {
    public class PopupData {
        public string Name;
        public string Note;
    }

    [SerializeField] private TMP_InputField _nameInput = default;
    [SerializeField] private TMP_InputField _noteInput = default;
    [SerializeField] private Button _confirmButton = default;
    [SerializeField] private Button _closeButton = default;
    private Action<Engagement> OnDone;
    private ICollection<string> _names;
    private Engagement _editingEngagement;
    private bool Editing => _editingEngagement != null;

    private void Awake() {
        _confirmButton.onClick.AddListener(CreateEngagement);
        _closeButton.onClick.AddListener(() => _ = PopupManager.Instance.Back());
    }

    public void Populate(Action<IDataEntry> onDone, ICollection<string> names, Engagement editingEngagement = null) {
        OnDone = onDone;
        this._names = names;
        _editingEngagement = editingEngagement;
        Clear();

        if (Editing) {
            _nameInput.text = editingEngagement.Name;
            _noteInput.text = editingEngagement.Note;;
        }
    }

    private void Clear() {
        _nameInput.text = "";
        _noteInput.text = "";
    }

    private async void CreateEngagement() {
        if (string.IsNullOrEmpty(_nameInput.text) || 
            (!Editing && _names.Contains(_nameInput.text))
        ) {
            var msgPopup = await PopupManager.Instance.GetOrLoadPopup<MessagePopup>();
            msgPopup.Populate(
                _names.Contains(_nameInput.text) ? "Name already exists." : "Please enter a name.",
                "Name");
            return;
        }

        Engagement engagement = new Engagement() {
            Name = _nameInput.text,
            Note = _noteInput.text
        };

        if (Editing) {
            engagement.NPCs = _editingEngagement.NPCs;
            engagement.Enemies = _editingEngagement.Enemies;
        }

        OnDone.Invoke(engagement);
        _ = PopupManager.Instance.Back();
    }

    public override object GetRestorationData() {
        PopupData popupData = new PopupData {
            Name = _nameInput.text,
            Note = _noteInput.text
        };

        return popupData;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            _nameInput.text = popupData.Name;
            _noteInput.text = popupData.Note;
        }
    }
}