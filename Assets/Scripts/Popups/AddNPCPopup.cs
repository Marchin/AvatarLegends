using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddNPCPopup : Popup {
    public class PopupData {
        public string Name;
        public NPC.EAlignment Alignment;
        public NPC.EType Type;
        public NPC.ETraining Training;
        public string Principle;
        public string Description;
        public bool IsGroup;
        public Action<IDataEntry> OnDone;
        public ICollection<string> Names;
        public NPC EditingNPC;
    }

    [SerializeField] private TMP_InputField _nameInput = default;
    [SerializeField] private DropdownElement _type = default;
    [SerializeField] private DropdownElement _alignment = default;
    [SerializeField] private Toggle _isGroup = default;
    [SerializeField] private DropdownElement _training = default;
    [SerializeField] private TMP_InputField _principleInput = default;
    [SerializeField] private TMP_InputField _descriptionInput = default;
    [SerializeField] private Button _confirmButton = default;
    [SerializeField] private Button _closeButton = default;
    [SerializeField] private TextMeshProUGUI _title = default;
    private Action<IDataEntry> OnDone;
    private ICollection<string> _names;
    private NPC _editingNPC;
    private bool Editing => _editingNPC != null;

    private void Awake() {
        _confirmButton.onClick.AddListener(CreateNPC);
        _closeButton.onClick.AddListener(() => _ = PopupManager.Instance.Back());

        string[] alignments = Enum.GetNames(typeof(NPC.EAlignment));
        List<string> alignmentOptions = new List<string>(alignments.Length);

        for (int iAlignment = 0; iAlignment < alignments.Length; ++iAlignment) {
            alignmentOptions.Add(alignments[iAlignment]);
        }

        _alignment.Populate(new DropdownData {
            Options = alignmentOptions
        });

        string[] types = Enum.GetNames(typeof(NPC.EType));
        List<string> typeOptions = new List<string>(types.Length);

        for (int iType = 0; iType < types.Length; ++iType) {
            typeOptions.Add(types[iType]);
        }

        _type.Populate(new DropdownData {
            Options = typeOptions,
            Callback = value => {
                if ((NPC.EType)value == NPC.EType.Minor) {
                    _isGroup.isOn = false;
                    _isGroup.interactable = false;
                } else {
                    _isGroup.interactable = true;
                }
            }
        });

        string[] trainings = Enum.GetNames(typeof(NPC.ETraining));
        List<string> trainingOptions = new List<string>(trainings.Length);

        for (int iTraining = 0; iTraining < trainings.Length; ++iTraining) {
            trainingOptions.Add(trainings[iTraining]);
        }

        _training.Populate(new DropdownData {
            Options = trainingOptions
        });
    }

    public void Populate(Action<IDataEntry> onDone, ICollection<string> names, NPC editingNPC = null) {
        OnDone = onDone;
        this._names = names;
        _editingNPC = editingNPC;
        Clear();

        if (Editing) {
            _nameInput.text = editingNPC.Name;
            _descriptionInput.text = editingNPC.Description;
            _principleInput.text = editingNPC.Principle;
            _alignment.Value = (int)editingNPC.Alignment;
            _type.Value = (int)editingNPC.Type;
            _training.Value = (int)editingNPC.Training;
            _isGroup.isOn = editingNPC.IsGroup;
        }

        _title.text = Editing ? "NPC Edition" : "NPC Creation";
    }

    private void Clear() {
        _nameInput.text = "";
        _descriptionInput.text = "";
        _principleInput.text = "";
        _alignment.Value = 0;
        _type.Value = 0;
        _training.Value = 0;
        _isGroup.isOn = false;
    }

    private async void CreateNPC() {
        if (string.IsNullOrEmpty(_nameInput.text) || 
            (!Editing && _names.Contains(_nameInput.text))
        ) {
            var msgPopup = await PopupManager.Instance.GetOrLoadPopup<MessagePopup>();
            msgPopup.Populate(
                _names.Contains(_nameInput.text) ? "Name already exists." : "Please enter a name.",
                "Name");
            return;
        }

        NPC npc = new NPC() {
            Name = _nameInput.text,
            Description = _descriptionInput.text,
            Alignment = (NPC.EAlignment)_alignment.Value,
            Type = (NPC.EType)_type.Value,
            Training = (NPC.ETraining)_training.Value,
            Principle = _principleInput.text,
            IsGroup = _isGroup.isOn
        };

        if (Editing) {
            npc.Balance = Math.Min(_editingNPC.Balance, npc.GetMaxBalance());
            npc.Fatigue = Math.Min(_editingNPC.Balance, npc.GetMaxFatigue());
            npc.Conditions = new Dictionary<string, Condition>(_editingNPC.Conditions);

            int amountToRemove = Mathf.Max(npc.Conditions.Count - npc.GetMaxConditions(), 0);
            List<string> keys = new List<string>(npc.Conditions.Keys);
            for (int iKey = 0; iKey < amountToRemove; ++iKey) {
                npc.Conditions.Remove(keys[keys.Count - iKey - 1]);
            }
            
            var availableTechniques = npc.GetAvailableTechniques();

            foreach (var technique in _editingNPC.Techniques) {
                if (availableTechniques.Contains(technique.Value)) {
                    npc.Techniques.Add(technique.Key, technique.Value);
                }
            }
        }

        OnDone.Invoke(npc);
        _ = PopupManager.Instance.Back();
    }

    public override object GetRestorationData() {
        PopupData popupData = new PopupData {
            Name = _nameInput.text,
            Description = _descriptionInput.text,
            Alignment = (NPC.EAlignment)_alignment.Value,
            Type = (NPC.EType)_type.Value,
            Training = (NPC.ETraining)_training.Value,
            Principle = _principleInput.text,
            IsGroup = _isGroup.isOn,
            EditingNPC = _editingNPC
        };

        return popupData;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            Populate(popupData.OnDone, popupData.Names, popupData.EditingNPC);
            _nameInput.text = popupData.Name;
            _descriptionInput.text = popupData.Description;
            _alignment.Value = (int)popupData.Alignment;
            _type.Value = (int)popupData.Type;
            _training.Value = (int)popupData.Training;
            _principleInput.text = popupData.Principle;
            _isGroup.isOn = popupData.IsGroup;
        }
    }
}