using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddTechniquePopup : Popup {
    public class PopupData {
        public string Name;
        public Technique.EMastery Mastery;
        public Technique.EApproach Approach;
        public string Description;
        public bool IsRare;
    }

    [SerializeField] private TextMeshProUGUI _title = default;
    [SerializeField] private TMP_InputField _nameInput = default;
    [SerializeField] private DropdownElement _masteryDropdown = default;
    [SerializeField] private DropdownElement _approachDropdown = default;
    [SerializeField] private TMP_InputField _descriptionInput = default;
    [SerializeField] private Toggle _isRare = default;
    [SerializeField] private Button _confirmButton = default;
    [SerializeField] private Button _closeButton = default;
    private Action<Technique> OnDone;
    private ICollection<string> _names;
    private Technique _editingTechnique;
    private bool Editing => _editingTechnique != null;

    private void Awake() {
        _confirmButton.onClick.AddListener(CreateTechnique);
        _closeButton.onClick.AddListener(() => _ = PopupManager.Instance.Back());

        string[] masteries = Enum.GetNames(typeof(Technique.EMastery));
        List<string> masteryOptions = new List<string>(masteries.Length);

        for (int iMastery = 0; iMastery < masteries.Length; ++iMastery) {
            masteryOptions.Add(masteries[iMastery]);
        }

        _masteryDropdown.Populate(new DropdownData {
            Options = masteryOptions
        });

        string[] approaches = Enum.GetNames(typeof(Technique.EApproach));
        List<string> approachOptions = new List<string>(approaches.Length);

        for (int iApproach = 0; iApproach < approaches.Length; ++iApproach) {
            approachOptions.Add(approaches[iApproach]);
        }

        _approachDropdown.Populate(new DropdownData {
            Options = approachOptions
        });
    }

    public void Populate(Action<IDataEntry> onDone, ICollection<string> names, Technique editingTechnique = null) {
        OnDone = onDone;
        this._names = names;
        _editingTechnique = editingTechnique;
        Clear();

        if (Editing) {
            _nameInput.text = editingTechnique.Name;
            _descriptionInput.text = editingTechnique.Description;
            _approachDropdown.Value = (int)editingTechnique.Approach;
            _masteryDropdown.Value = (int)editingTechnique.Mastery;
            _isRare.isOn = editingTechnique.Rare;
        }

        _title.text = Editing ? "Techique Edition" : "Techique Creation";
    }

    private void Clear() {
        _nameInput.text = "";
        _descriptionInput.text = "";
        _approachDropdown.Value = 0;
        _masteryDropdown.Value = 0;
        _isRare.isOn = false;
    }

    private async void CreateTechnique() {
        if (string.IsNullOrEmpty(_nameInput.text) || 
            (!Editing && _names.Contains(_nameInput.text))
        ) {
            var msgPopup = await PopupManager.Instance.GetOrLoadPopup<MessagePopup>();
            msgPopup.Populate(
                _names.Contains(_nameInput.text) ? "Name already exists." : "Please enter a name.",
                "Name");
            return;
        }

        Technique technique = new Technique() {
            Name = _nameInput.text,
            Description = _descriptionInput.text,
            Mastery = (Technique.EMastery)_masteryDropdown.Value,
            Approach = (Technique.EApproach)_approachDropdown.Value,
            Rare = _isRare.isOn
        };

        OnDone.Invoke(technique);
        _ = PopupManager.Instance.Back();

    }

    public override object GetRestorationData() {
        PopupData popupData = new PopupData {
            Name = _nameInput.text,
            Description = _descriptionInput.text,
            Mastery = (Technique.EMastery)_masteryDropdown.Value,
            Approach = (Technique.EApproach)_approachDropdown.Value,
            IsRare = _isRare.isOn
        };

        return popupData;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            _nameInput.text = popupData.Name;
            _descriptionInput.text = popupData.Description;
            _masteryDropdown.Value = (int)popupData.Mastery;
            _approachDropdown.Value = (int)popupData.Approach;
            _isRare.isOn = popupData.IsRare;
        }
    }
}
