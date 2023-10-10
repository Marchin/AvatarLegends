using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AddPCPopup : AddEntryPopup<PC> {
    public class PopupData {
        public BasePopupData BasePopupData;
        public string Player;
        public ETraining Training;
        public int Playbook;
        public string Backstory;
    }

    [SerializeField] private TMP_InputField _playerInput = default;
    [SerializeField] private DropdownElement _training = default;
    [SerializeField] private DropdownElement _playbook = default;
    [SerializeField] private TMP_InputField _backstoryInput = default;
    private AppData Data => ApplicationManager.Instance.Data;

    protected override void Awake() {
        base.Awake();

        string[] trainings = Enum.GetNames(typeof(ETraining));
        List<string> trainingOptions = new List<string>(trainings.Length);

        for (int iTraining = 0; iTraining < trainings.Length; ++iTraining) {
            trainingOptions.Add(trainings[iTraining]);
        }

        _training.Populate(new DropdownData {
            Options = trainingOptions
        });
    
        _playbook.Populate(new DropdownData {
            Options = new List<string>(Data.Playbooks.Keys)
        });
    }

    protected override void OnClear() {
        _playerInput.text = "";
        _backstoryInput.text = "";
        _playbook.Value = 0;
        _training.Value = 0;
    }

    protected override void OnPopulated() {
        if (Editing) {
            _playerInput.text = _editingEntry.Player;
            _backstoryInput.text = _editingEntry.Backstory;

            List<string> playbooks = new List<string>(Data.Playbooks.Keys);
            _playbook.Value = playbooks.IndexOf(_editingEntry.Playbook);
        }
    }

    protected override IDataEntry OnEntryCreation() {
        PC pc = new PC() {
            Name = NewName,
            Player = _playerInput.text,
            Backstory = _backstoryInput.text,
            Playbook = _playbook.SelectedOption
        };

        if (Editing) {
            pc.Note = _editingEntry.Note;
            pc.Connections = _editingEntry.Connections;
            pc.Trainings = _editingEntry.Trainings;
        }

        return pc;
    }

    public override object GetRestorationData() {
        PopupData popupData = new PopupData {
            BasePopupData = base.GetRestorationData() as BasePopupData,
            Player = _playerInput.text,
            Training = (ETraining)_training.Value,
            Playbook = _playbook.Value,
            Backstory = _backstoryInput.text,
        };

        return popupData;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            base.Restore(popupData.BasePopupData);
            _playerInput.text = popupData.Player;
            _training.Value = (int)popupData.Training;
            _playbook.Value = popupData.Playbook;
            _backstoryInput.text = popupData.Backstory;
        }
    }
}
