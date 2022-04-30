using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddCharacterPopup : Popup {
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
    [SerializeField] private Button _createButton = default;
    [SerializeField] private Button _closeButton = default;
    private Action<NPC> OnDone;

    private void Awake() {
        _createButton.onClick.AddListener(CreateCharacter);
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

    public void Populate(Action<NPC> onDone) {
        OnDone = onDone;
        Clear();
    }

    private void Clear() {
        _nameInput.text = "";
        _principleInput.text = "";
        _npcType.Value = 0;
        _training.Value = 0;
    }

    private void CreateCharacter() {
        if (string.IsNullOrEmpty(_nameInput.text) ||
            string.IsNullOrEmpty(_descriptionInput.text) ||
            string.IsNullOrEmpty(_principleInput.text)
        ) {

            return;
        }

        NPC npc = new NPC() {
            Name = _nameInput.text,
            Description = _descriptionInput.text,
            Type = (NPC.EType)_npcType.Value,
            Training = (NPC.ETraining)_training.Value,
            Principle = _principleInput.text
        };

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