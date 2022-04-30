using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AddCharacterPopup : MonoBehaviour {
    [SerializeField] private TMP_InputField _nameInput = default;
    [SerializeField] private DropdownElement _npcType = default;
    [SerializeField] private DropdownElement _training = default;
    [SerializeField] private TMP_InputField _principleInput = default;
    [SerializeField] private TMP_InputField _descriptionInput = default;
    [SerializeField] private Button _createButton = default;
    private Action<NPC> OnDone;

    private void Awake() {
        _createButton.onClick.AddListener(CreateCharacter);

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
        NPC npc = new NPC() {
            Name = _nameInput.text,
            Description = _descriptionInput.text,
            Type = (NPC.EType)_npcType.Value,
            Training = (NPC.ETraining)_training.Value,
            Techniques = new List<Technique>(),
            Statuses = new List<Status>(),
            Principle = _principleInput.text
        };

        OnDone.Invoke(npc);
        gameObject.SetActive(false);
    }
}