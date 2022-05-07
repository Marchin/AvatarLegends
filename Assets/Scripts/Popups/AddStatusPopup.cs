using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddStatusPopup : Popup {
    public class PopupData {
        public string Name;
        public string Description;
        public bool IsPositive;
    }

    [SerializeField] private TextMeshProUGUI _title = default;
    [SerializeField] private TMP_InputField _nameInput = default;
    [SerializeField] private TMP_InputField _descriptionInput = default;
    [SerializeField] private Toggle _isPositive = default;
    [SerializeField] private Button _confirmButton = default;
    [SerializeField] private Button _closeButton = default;
    private Action<Status> OnDone;
    private ICollection<string> _names;
    private Status _editingStatus;
    private bool Editing => _editingStatus != null;

    private void Awake() {
        _confirmButton.onClick.AddListener(CreateStatus);
        _closeButton.onClick.AddListener(() => _ = PopupManager.Instance.Back());
    }

    public void Populate(Action<IDataEntry> onDone, ICollection<string> names, Status editingStatus = null) {
        OnDone = onDone;
        this._names = names;
        _editingStatus = editingStatus;
        Clear();

        if (Editing) {
            _nameInput.text = editingStatus.Name;
            _descriptionInput.text = editingStatus.Description;
            _isPositive.isOn = editingStatus.IsPositive;
        }

        _title.text = Editing ? "Techique Edition" : "Techique Creation";
    }

    private void Clear() {
        _nameInput.text = "";
        _descriptionInput.text = "";
        _isPositive.isOn = false;
    }

    private async void CreateStatus() {
        if (string.IsNullOrEmpty(_nameInput.text) || 
            (!Editing && _names.Contains(_nameInput.text))
        ) {
            var msgPopup = await PopupManager.Instance.GetOrLoadPopup<MessagePopup>();
            msgPopup.Populate(
                _names.Contains(_nameInput.text) ? "Name already exists." : "Please enter a name.",
                "Name");
            return;
        }

        Status status = new Status() {
            Name = _nameInput.text,
            Description = _descriptionInput.text,
            IsPositive = _isPositive.isOn
        };

        OnDone.Invoke(status);
        _ = PopupManager.Instance.Back();

    }

    public override object GetRestorationData() {
        PopupData popupData = new PopupData {
            Name = _nameInput.text,
            Description = _descriptionInput.text,
            IsPositive = _isPositive.isOn
        };

        return popupData;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            _nameInput.text = popupData.Name;
            _descriptionInput.text = popupData.Description;
            _isPositive.isOn = popupData.IsPositive;
        }
    }
}
