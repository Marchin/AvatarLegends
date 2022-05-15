using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddTechniquePopup : AddEntryPopup<Technique> {
    public class PopupData {
        public BasePopupData BasePopupData;
        public Technique.EMastery Mastery;
        public Technique.EApproach Approach;
        public string Description;
        public bool IsRare;
    }

    [SerializeField] private DropdownElement _masteryDropdown = default;
    [SerializeField] private DropdownElement _approachDropdown = default;
    [SerializeField] private TMP_InputField _descriptionInput = default;
    [SerializeField] private Toggle _isRare = default;

    protected override void Awake() {
        base.Awake();

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

    protected override void OnPopulated() {
        if (Editing) {
            _descriptionInput.text = _editingEntry.Description;
            _approachDropdown.Value = (int)_editingEntry.Approach;
            _masteryDropdown.Value = (int)_editingEntry.Mastery;
            _isRare.isOn = _editingEntry.Rare;
        }
    }

    protected override void OnClear() {
        _descriptionInput.text = "";
        _approachDropdown.Value = 0;
        _masteryDropdown.Value = 0;
        _isRare.isOn = false;
    }

    protected override IDataEntry OnEntryCreation() {
        Technique technique = new Technique() {
            Name = NewName,
            Description = _descriptionInput.text,
            Mastery = (Technique.EMastery)_masteryDropdown.Value,
            Approach = (Technique.EApproach)_approachDropdown.Value,
            Rare = _isRare.isOn
        };

        return technique;

    }

    public override object GetRestorationData() {
        PopupData popupData = new PopupData {
            BasePopupData = base.GetRestorationData() as BasePopupData,
            Description = _descriptionInput.text,
            Mastery = (Technique.EMastery)_masteryDropdown.Value,
            Approach = (Technique.EApproach)_approachDropdown.Value,
            IsRare = _isRare.isOn
        };

        return popupData;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            base.Restore(popupData.BasePopupData);
            _descriptionInput.text = popupData.Description;
            _masteryDropdown.Value = (int)popupData.Mastery;
            _approachDropdown.Value = (int)popupData.Approach;
            _isRare.isOn = popupData.IsRare;
        }
    }
}
