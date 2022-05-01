using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class NPC {
    public enum ETraining {
        Earth,
        Water,
        Fire,
        Air,
        Weapons,
        Tech
    }

    public enum EType {
        Minor,
        Major,
        Master,
        Legendary,
    }

    [JsonProperty("name")]
    public string Name;
    
    [JsonProperty("description")]
    public string Description;

    [JsonProperty("type")]
    public EType Type;

    [JsonProperty("is_group")]
    public bool IsGroup;

    [JsonProperty("training")]
    public ETraining Training;

    [JsonProperty("principle")]
    public string Principle;

    [JsonProperty("balance")]
    public int Balance;
    
    [JsonProperty("fatigue")]
    public int Fatigue;
    
    [JsonProperty("techniques")]
    public Dictionary<string, Technique> Techniques = new Dictionary<string, Technique>();
    
    [JsonProperty("statuses")]
    public Dictionary<string, Status> Statuses = new Dictionary<string, Status>();

    [JsonProperty("conditions")]
    public Dictionary<string, Condition> Conditions = new Dictionary<string, Condition>();
    private bool _showConditions;
    private Action _onRefresh;

    public List<InformationData> RetrieveData(Action refresh) {
        _onRefresh = refresh;

        List<InformationData> result = new List<InformationData>();

        result.Add(new InformationData {
            Prefix = "Type",
            Content = Type.ToString(),
        });

        if (!string.IsNullOrEmpty(Description)) {
            result.Add(new InformationData {
                Content = "Description",
                OnMoreInfo = ShowDescription,
            });
        }

        result.Add(new InformationData {
            Prefix = "Training",
            Content = Training.ToString(),
        });

        result.Add(new InformationData {
            Prefix = "Principle",
            Content = Principle,
            InitValue = Balance,
            MaxValue = GetMaxBalance(),
            OnValueChange = ChangeBalance
        });

        result.Add(new InformationData {
            Content = $"Fatigue",
            InitValue = Fatigue,
            MaxValue = GetMaxFatigue(),
            OnValueChange = ChangeFatigue
        });

        Action onConditionDropdown = () => {
            _showConditions = !_showConditions;
            _onRefresh();
        };

        result.Add(new InformationData {
            Content = $"Condition ({Conditions.Count}/{GetMaxConditions()})",
            OnDropdown = (Conditions.Count > 0) ? onConditionDropdown : null,
            OnAdd = (Conditions.Count < GetMaxConditions()) ?
                AddCondition :
                (Action)null,
            Expanded = _showConditions
        });

        if (_showConditions) {
            foreach (var condition in Conditions) {
                result.Add(new InformationData {
                    Content = condition.Key,
                    IsToggleOn = condition.Value.IsOn,
                    OnToggle = isOn => Conditions[condition.Key].IsOn = isOn,
                    IndentLevel = 1
                });
            }
        }


        return result;
    }

    private async void AddCondition() {
        var inputPopup = await PopupManager.Instance.GetOrLoadPopup<InputPopup>(restore: false);
        inputPopup.Populate("Add a condition.", "Condition", onConfirm: async input => {
            if (string.IsNullOrEmpty(input) || Conditions.ContainsKey(input)) {
                var msgPopup = await PopupManager.Instance.GetOrLoadPopup<MessagePopup>();
                msgPopup.Populate(
                    Conditions.ContainsKey(input) ? "Name already exists." : "Please enter a name.",
                    "Name");
            } else {
                Conditions.Add(input, new Condition { Name = input });
                _showConditions = true;
                _onRefresh();
                await PopupManager.Instance.Back();
            }
        });
    }

    public void ChangeBalance(int value) {
        int maxBalance = GetMaxBalance();
        UnityEngine.Debug.Assert((value >= 0) && (value <= maxBalance), "Invalid Balance");

        Balance = UnityEngine.Mathf.Clamp(value, 0, maxBalance);
    }

    public int GetMaxBalance() {
        int result = 0;

        switch (Type) {
            case EType.Minor: {
                result = 1;
            } break;
            case EType.Major: {
                result = 2;
            } break;
            case EType.Master: {
                result = 3;
            } break;
            case EType.Legendary: {
                result = 4;
            } break;
        }

        return result;
    }

    public void ChangeFatigue(int value) {
        int maxFatigue = GetMaxFatigue();
        UnityEngine.Debug.Assert((value >= 0) && (value <= maxFatigue), "Invalid Balance");

        Fatigue = UnityEngine.Mathf.Clamp(value, 0, maxFatigue);
        _onRefresh();
    }

    public int GetMaxFatigue() {
        int result = 0;

        switch (Type) {
            case EType.Minor: {
                result = 3;
            } break;
            case EType.Major: {
                result = 5;
            } break;
            case EType.Master: {
                result = 10;
            } break;
            case EType.Legendary: {
                result = 15;
            } break;
        }

        return result;
    }

    public int GetMaxConditions() {
        int result = 0;

        switch (Type) {
            case EType.Minor: {
                result = 1;
            } break;
            case EType.Major: {
                result = 3;
            } break;
            case EType.Master: {
                result = 5;
            } break;
            case EType.Legendary: {
                result = 8;
            } break;
        }

        return result;
    }

    public async void ShowDescription() {
        var msgPopup = await PopupManager.Instance.GetOrLoadPopup<MessagePopup>(restore: false);
        msgPopup.Populate(Description, "Description");
    }
}