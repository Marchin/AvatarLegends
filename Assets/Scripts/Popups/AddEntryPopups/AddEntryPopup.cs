using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class AddEntryPopup<T> : Popup where T : IDataEntry {
    public class BasePopupData {
        public string Name;
        public Action<IDataEntry> OnDone;
        public ICollection<string> Names;
        public IDataEntry EditingEntry;
    }

    [SerializeField] private TextMeshProUGUI _title = default;
    [SerializeField] private TMP_InputField _nameInput = default;
    [SerializeField] private Button _confirmButton = default;
    [SerializeField] private Button _closeButton = default;
    private Action<IDataEntry> OnDone;
    private ICollection<string> _names;
    protected T _editingEntry;
    protected bool Editing => _editingEntry != null;
    protected string NewName {
        get => _nameInput.text;
        set => _nameInput.text = value;
    }

    protected virtual void Awake() {
        _confirmButton.onClick.AddListener(CreateEntry);
        _closeButton.onClick.AddListener(PopupManager.Instance.Back);
    }

    public void Populate(Action<IDataEntry> onDone, ICollection<string> names, T editingEntry) {
        OnDone = onDone;
        this._names = names;
        _editingEntry = editingEntry;
        Clear();

        _title.text = Editing ? $"{typeof(T)} Edition" : $"{typeof(T)} Creation";

        if (Editing) {
            _nameInput.text = _editingEntry.Name;
        }

        _nameInput.Select();

        OnPopulated();
    }

    protected abstract void OnPopulated();

    private void Clear() {
        _nameInput.text = "";
        OnClear();
    }

    protected abstract void OnClear();

    private async void CreateEntry() {
        if (string.IsNullOrEmpty(_nameInput.text) || 
            (!Editing && _names.Contains(_nameInput.text))
        ) {
            var msgPopup = await PopupManager.Instance.GetOrLoadPopup<MessagePopup>();
            msgPopup.Populate(
                _names.Contains(_nameInput.text) ? "Name already exists." : "Please enter a name.",
                "Name");
            return;
        }

        OnDone.Invoke(OnEntryCreation());
        PopupManager.Instance.Back();
    }

    protected abstract IDataEntry OnEntryCreation();

    public override object GetRestorationData() {
        BasePopupData popupData = new BasePopupData {
            Name = _nameInput.text,
            OnDone = OnDone,
            Names = _names,
            EditingEntry = _editingEntry
        };

        return popupData;
    }

    public override void Restore(object data) {
        if (data is BasePopupData popupData) {
            Populate(popupData.OnDone, popupData.Names, (T)popupData.EditingEntry);
            _nameInput.text = popupData.Name;
        }
    }
}
