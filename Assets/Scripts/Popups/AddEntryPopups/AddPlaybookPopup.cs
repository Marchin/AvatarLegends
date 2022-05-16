using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AddPlaybookPopup : AddEntryPopup<Playbook> {
    public class PopupData {
        public BasePopupData BasePopupData;
        public string Description;
        public string Principles;
        public List<string> Conditions;
    }

    [SerializeField] private TMP_InputField _descriptionInput = default;
    [SerializeField] private TMP_InputField _principlesInput = default;
    [SerializeField] private InformationList _conditionsList = default;
    private List<string> _conditions = new List<string>();
    private bool _showConditions;
    private AppData Data => ApplicationManager.Instance.Data;

    protected override void OnPopulated() {
        if (Editing) {
            _descriptionInput.text = _editingEntry.Description;
            _principlesInput.text = _editingEntry.Principles;
            _conditions = _editingEntry.Conditions;
        }

        _conditions = _conditions ?? new List<string>();

        RefreshConditions();
    }

    private void AddCondition() {
        var availableConditions = IDataEntry.GetAvailableEntries<Condition>(_conditions, Data.Conditions.Values);
        if (availableConditions.Count > 0) {
            Action<List<string>> onDone = names => {
                foreach (var name in names) {
                    if (!_conditions.Contains(name)) {
                        _conditions.Add(name);
                    }
                }
                RefreshConditions();
            };
            IDataEntry.AddEntry<Condition>(_conditions, Data.Conditions.Values, onDone);
        } else {
            MessagePopup.ShowMessage("No more conditions available, add more under the \"Conditions\" tab.", "Conditions");
        }
    }

    private void RefreshConditions() {
        List<InformationData> conditionsData = new List<InformationData>();
        
        Action onConditionDropdown = () => {
            _showConditions = !_showConditions;
            RefreshConditions();
        };

        conditionsData.Add(new InformationData {
            Content = $"Conditions ({_conditions.Count}/{PC.ConditionsAmount})",
            OnDropdown = (_conditions.Count > 0) ? onConditionDropdown : null,
            OnAdd = (_conditions.Count < PC.ConditionsAmount) ?
                AddCondition :
                (Action)null,
            Expanded = _showConditions
        });

        if (_showConditions) {
            List<string> conditionNames = new List<string>(_conditions);
            conditionNames.Sort();
            foreach (var conditionName in conditionNames) {
                conditionsData.Add(new InformationData {
                    Content = conditionName,
                    OnDelete = () => {
                        _conditions.Remove(conditionName);
                        RefreshConditions();
                    },
                    IndentLevel = 1
                });
            }
        }

        _conditionsList.Populate(conditionsData);
    }

    protected override void OnClear() {
        _descriptionInput.text = "";
        _principlesInput.text = "";
        _conditions.Clear();
        RefreshConditions();
    }

    protected override IDataEntry OnEntryCreation() {
        Playbook playbook = new Playbook {
            Name = NewName,
            Description = _descriptionInput.text,
            Principles = _principlesInput.text,
            Conditions = _conditions
        };

        return playbook;
    }

    public override object GetRestorationData() {
        PopupData popupData = new PopupData {
            BasePopupData = base.GetRestorationData() as BasePopupData,
            Description = _descriptionInput.text,
            Principles = _principlesInput.text,
            Conditions = _conditions
        };

        return popupData;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            base.Restore(popupData.BasePopupData);
            _descriptionInput.text = popupData.Description;
            _principlesInput.text = popupData.Principles;
            _conditions = popupData.Conditions;
            RefreshConditions();
        }
    }
}
