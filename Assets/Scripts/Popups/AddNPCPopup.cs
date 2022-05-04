using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddNPCPopup : Popup {
    public class PopupData {
        public string Name;
        public NPC.EType NPCType;
        public NPC.ETraining Training;
        public string Principle;
        public string Description;
    }

    [SerializeField] private TMP_InputField _nameInput = default;
    [SerializeField] private DropdownElement _npcType = default;
    [SerializeField] private DropdownElement _training = default;
    [SerializeField] private TMP_InputField _principleInput = default;
    [SerializeField] private TMP_InputField _descriptionInput = default;
    [SerializeField] private Button _confirmButton = default;
    [SerializeField] private Button _closeButton = default;
    [SerializeField] private TextMeshProUGUI _title = default;
    private Action<NPC> OnDone;
    private ICollection<string> _names;
    private NPC _editingNPC;
    private bool Editing => _editingNPC != null;

    private void Awake() {
        _confirmButton.onClick.AddListener(CreateCharacter);
        _closeButton.onClick.AddListener(() => _ = PopupManager.Instance.Back());

        string[] types = Enum.GetNames(typeof(NPC.EType));
        List<string> typeOptions = new List<string>(types.Length);

        for (int iType = 0; iType < types.Length; ++iType) {
            typeOptions.Add(types[iType]);
        }

        _npcType.Populate(new DropdownData {
            Options = typeOptions
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
            _npcType.Value = (int)editingNPC.Type;
            _training.Value = (int)editingNPC.Training;
        }

        _title.text = Editing ? "Character Edition" : "Character Creation";
    }

    private void Clear() {
        _nameInput.text = "";
        _descriptionInput.text = "";
        _principleInput.text = "";
        _npcType.Value = 0;
        _training.Value = 0;
    }

    private async void CreateCharacter() {
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
            Type = (NPC.EType)_npcType.Value,
            Training = (NPC.ETraining)_training.Value,
            Principle = _principleInput.text
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
            
        }

        OnDone.Invoke(npc);
        _ = PopupManager.Instance.Back();
    }

    public override object GetRestorationData() {
        PopupData popupData = new PopupData {
            Name = _nameInput.text,
            Description = _descriptionInput.text,
            NPCType = (NPC.EType)_npcType.Value,
            Training = (NPC.ETraining)_training.Value,
            Principle = _principleInput.text
        };

        return popupData;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            _nameInput.text = popupData.Name;
            _descriptionInput.text = popupData.Description;
            _npcType.Value = (int)popupData.NPCType;
            _training.Value = (int)popupData.Training;
            _principleInput.text = popupData.Principle;
        }
    }
}