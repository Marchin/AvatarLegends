using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddNPCPopup : AddEntryPopup<NPC> {
    public class PopupData {
        public BasePopupData BasePopupData;
        public NPC.EAlignment Alignment;
        public NPC.EType Type;
        public NPC.ETraining Training;
        public string Principle;
        public string Description;
        public bool IsGroup;
    }

    [SerializeField] private DropdownElement _type = default;
    [SerializeField] private DropdownElement _alignment = default;
    [SerializeField] private Toggle _isGroup = default;
    [SerializeField] private DropdownElement _training = default;
    [SerializeField] private TMP_InputField _principleInput = default;
    [SerializeField] private TMP_InputField _descriptionInput = default;

    protected override void Awake() {
        base.Awake();

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

    protected override void OnPopulated() {
        if (Editing) {
            _descriptionInput.text = _editingEntry.Description;
            _principleInput.text = _editingEntry.Principle;
            _alignment.Value = (int)_editingEntry.Alignment;
            _type.Value = (int)_editingEntry.Type;
            _training.Value = (int)_editingEntry.Training;
            _isGroup.isOn = _editingEntry.IsGroup;
        }
    }

    protected override void OnClear() {
        _descriptionInput.text = "";
        _principleInput.text = "";
        _alignment.Value = 0;
        _type.Value = 0;
        _training.Value = 0;
        _isGroup.isOn = false;
    }

    protected override IDataEntry OnEntryCreation() {
        NPC npc = new NPC() {
            Name = NewName,
            Description = _descriptionInput.text,
            Alignment = (NPC.EAlignment)_alignment.Value,
            Type = (NPC.EType)_type.Value,
            Training = (NPC.ETraining)_training.Value,
            Principle = _principleInput.text,
            IsGroup = _isGroup.isOn
        };

        if (Editing) {
            npc.Balance = Math.Min(_editingEntry.Balance, npc.GetMaxBalance());
            npc.Fatigue = Math.Min(_editingEntry.Balance, npc.GetMaxFatigue());
            npc.Conditions = new Dictionary<string, ConditionState>(_editingEntry.Conditions);

            int amountToRemove = Mathf.Max(npc.Conditions.Count - npc.GetMaxConditions(), 0);
            List<string> keys = new List<string>(npc.Conditions.Keys);
            for (int iKey = 0; iKey < amountToRemove; ++iKey) {
                npc.Conditions.Remove(keys[keys.Count - iKey - 1]);
            }
            
            var availableTechniques = npc.GetAvailableTechniques();

            foreach (var technique in _editingEntry.Techniques) {
                if (availableTechniques.Contains(technique.Value)) {
                    npc.Techniques.Add(technique.Key, technique.Value);
                }
            }
        }

        return npc;
    }

    public override object GetRestorationData() {
        PopupData popupData = new PopupData {
            BasePopupData = base.GetRestorationData() as BasePopupData,
            Description = _descriptionInput.text,
            Alignment = (NPC.EAlignment)_alignment.Value,
            Type = (NPC.EType)_type.Value,
            Training = (NPC.ETraining)_training.Value,
            Principle = _principleInput.text,
            IsGroup = _isGroup.isOn
        };

        return popupData;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            base.Restore(popupData.BasePopupData);
            _descriptionInput.text = popupData.Description;
            _alignment.Value = (int)popupData.Alignment;
            _type.Value = (int)popupData.Type;
            _training.Value = (int)popupData.Training;
            _principleInput.text = popupData.Principle;
            _isGroup.isOn = popupData.IsGroup;
        }
    }
}