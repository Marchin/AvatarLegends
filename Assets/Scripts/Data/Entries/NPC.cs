using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class NPC : IDataEntry {
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

    public enum EAlignment {
        Neutral,
        Ally,
        Enemy
    }

    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("description")]
    public string Description;

    [JsonProperty("type")]
    public EType Type;

    [JsonProperty("alignment")]
    public EAlignment Alignment;

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
    public Dictionary<string, ConditionState> Conditions = new Dictionary<string, ConditionState>();
    private bool _showConditions;
    private bool _showTechinques;
    private bool _showStatuses;
    private Action _onRefresh;
    public Action OnMoreInfo => !string.IsNullOrEmpty(Description) ? ShowDescription : null;
    private AppData Data => ApplicationManager.Instance.Data;

    public List<InformationData> RetrieveData(Action refresh) {
        _onRefresh = refresh;

        List<InformationData> result = new List<InformationData>();

        result.Add(new InformationData {
            Prefix = "Type",
            Content = Type.ToString(),
        });

        result.Add(new InformationData {
            Content = "Is Group",
            IsToggleOn = IsGroup,
        });

        result.Add(new InformationData {
            Prefix = "Alignment",
            Content = Alignment.ToString(),
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
            Content = string.IsNullOrEmpty(Principle) ? "(none)" : Principle,
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
            List<string> conditionNames = new List<string>(Conditions.Keys);
            conditionNames.Sort();
            foreach (var conditionName in conditionNames) {
                ConditionState condition = Conditions[conditionName];
                result.Add(new InformationData {
                    Content = conditionName,
                    IsToggleOn = condition.IsOn,
                    OnToggle = isOn => Conditions[conditionName].IsOn = isOn,
                    OnDelete = async () => {
                        await MessagePopup.ShowConfirmationPopup(
                            $"Remove {conditionName} condition?",
                            onYes: () => Conditions.Remove(conditionName)
                        );
                        _onRefresh();
                    },
                    IndentLevel = 1
                });
            }
        }

        Action onTechniqueDropdown = () => {
            _showTechinques = !_showTechinques;
            _onRefresh();
        };

        result.Add(new InformationData {
            Content = $"Techniques ({Techniques.Count}/{GetMaxTechniques()})",
            OnDropdown = (Techniques.Count > 0) ? onTechniqueDropdown : null,
            OnAdd = (Techniques.Count < GetMaxTechniques()) ?
                AddTechnique :
                (Action)null,
            Expanded = _showTechinques
        });

        if (_showTechinques) {
            List<string> techniqueNames = new List<string>(Techniques.Keys);
            techniqueNames.Sort();
            foreach (var techniqueName in techniqueNames) {
                Technique technique = Techniques[techniqueName];
                result.Add(new InformationData {
                    Content = techniqueName,
                    OnMoreInfo = technique.ShowInfo,
                    OnDelete = async () => {
                        await MessagePopup.ShowConfirmationPopup(
                            $"Remove {techniqueName} technique?",
                            onYes: () => Techniques.Remove(techniqueName)
                        );
                        _onRefresh();
                    },
                    IndentLevel = 1
                });
            }
        }

        Action onStatusesDropdown = () => {
            _showStatuses = !_showStatuses;
            _onRefresh();
        };

        result.Add(new InformationData {
            Content = $"Statuses ({Statuses.Count})",
            OnDropdown = (Statuses.Count > 0) ? onStatusesDropdown : null,
            OnAdd = (Statuses.Count < Data.Statuses.Count) ?
                AddStatus :
                (Action)null,
            Expanded = _showStatuses
        });

        if (_showStatuses) {
            List<string> statusesName = new List<string>(Statuses.Keys);
            statusesName.Sort((x, y) => {
                if (Statuses[x].IsPositive != Statuses[y].IsPositive) {
                    return Statuses[x].IsPositive ? -1 : 1;
                } else {
                    return x.CompareTo(y);
                }
            });

            foreach (var statusName in statusesName) {
                Status status = Statuses[statusName];
                string effect = status.IsPositive ? "Positive" : "Negative";
                result.Add(new InformationData {
                    Content = $"{statusName} ({effect})",
                    OnMoreInfo = status.ShowDescription,
                    OnDelete = async () => {
                        await MessagePopup.ShowConfirmationPopup(
                            $"Remove {statusName} status?",
                            onYes: () => Statuses.Remove(statusName)
                        );
                        _onRefresh();
                    },
                    IndentLevel = 1
                });
            }
        }

        return result;
    }

    private void AddCondition() {
        var availableConditions = IDataEntry.GetAvailableEntries<Condition>(Conditions.Keys, Data.Conditions.Values);
        if (availableConditions.Count > 0) {
            Action<List<string>> onDone = names => {
                foreach (var name in names) {
                    if (!Conditions.ContainsKey(name)) {
                        Conditions.Add(name, new ConditionState { Name = name });
                    }
                }
                _onRefresh();
            };
            IDataEntry.AddEntry<Condition>(Conditions.Keys, Data.Conditions.Values, onDone);
        } else {
            MessagePopup.ShowMessage("No more conditions available, add more under the \"Conditions\" tab.", "Conditions");
        }
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

    public void ShowDescription() {
        MessagePopup.ShowMessage(Description, nameof(Description));
    }

    public async void AddTechnique() {
        Dictionary<string, Technique> techniquesToAdd = new Dictionary<string, Technique>(Techniques);
        List<InformationData> infoList = new List<InformationData>(Data.Techniques.Count);
        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>(restore: false);
        var availableTechniques = GetAvailableTechniques();

        foreach (var technique in Techniques) {
            availableTechniques.Remove(technique.Value);
        }

        availableTechniques.Sort((x, y) => x.Name.CompareTo(y.Name));

        Refresh();

        void Refresh() {
            infoList.Clear();
            foreach (var technique in availableTechniques) {
                infoList.Add(new InformationData {
                    Content = technique.Name,
                    IsToggleOn = techniquesToAdd.ContainsKey(technique.Name),
                    OnToggle = async isOn => {
                        if (!isOn || (techniquesToAdd.Count < GetMaxTechniques())) {
                            if (isOn) {
                                techniquesToAdd.Add(technique.Name, technique);
                            } else {
                                techniquesToAdd.Remove(technique.Name);
                            }

                            Refresh();
                        } else {
                            var msgPopup = await PopupManager.Instance.GetOrLoadPopup<MessagePopup>();
                            msgPopup.Populate(
                                "You've already reached the maximum amount of techniques for this NPC.",
                                "Techniques Maxed"
                            );
                        }
                    },
                    OnMoreInfo = technique.ShowInfo
                });
            }

            listPopup.Populate(infoList,
                $"Add Techniques ({techniquesToAdd.Count}/{GetMaxTechniques()})",
                () => {
                    Techniques = techniquesToAdd;
                    _onRefresh();
                }
            );
        }
    }

    public int GetMaxTechniques() {
        int result = 0;

        switch (Type) {
            case EType.Minor: {
                result = 0;
            } break;
            case EType.Major: {
                result = 2;
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

    public List<Technique> GetAvailableTechniques() {
        List<Technique> results = new List<Technique>(100);

        foreach (var technique in Data.Techniques) {
            if (CanLearnTechnique(technique.Value)) {
                results.Add(technique.Value);
            }
        }

        return results;
    }

    private bool CanLearnTechnique(Technique technique) {
        bool result = false;

        switch (technique.Mastery) {
            case Technique.EMastery.Universal: {
                result = true;
            } break;
            case Technique.EMastery.Group: {
                result = IsGroup;
            } break;
            case Technique.EMastery.Air: {
                result = (Training == ETraining.Air);
            } break;
            case Technique.EMastery.Earth: {
                result = (Training == ETraining.Earth);
            } break;
            case Technique.EMastery.Water: {
                result = (Training == ETraining.Water);
            } break;
            case Technique.EMastery.Fire: {
                result = (Training == ETraining.Fire);
            } break;
            case Technique.EMastery.Weapons: {
                result = (Training == ETraining.Weapons);
            } break;
            case Technique.EMastery.Tech: {
                result = (Training == ETraining.Tech);
            } break;
        }

        return result;
    }
    
    public async void AddStatus() {
        Dictionary<string, Status> statusesToAdd = new Dictionary<string, Status>(Statuses);
        List<InformationData> infoList = new List<InformationData>(Data.Statuses.Count);
        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>(restore: false);
        var availableStatuses = new Dictionary<string, Status>(Data.Statuses);

        foreach (var status in Statuses) {
            availableStatuses.Remove(status.Key);
        }

        List<string> statusesName = new List<string>(availableStatuses.Keys);
        statusesName.Sort((x, y) => {
            if (availableStatuses[x].IsPositive != availableStatuses[y].IsPositive) {
                return availableStatuses[x].IsPositive ? -1 : 1;
            } else {
                return x.CompareTo(y);
            }
        });

        Refresh();

        void Refresh() {
            infoList.Clear();

            foreach (var statusName in statusesName) {
                Status status = availableStatuses[statusName];
                string effect = status.IsPositive ? "Positive" : "Negative";
                infoList.Add(new InformationData {
                    Content = statusName + $" {(effect)}",
                    IsToggleOn = statusesToAdd.ContainsKey(statusName),
                    OnToggle = isOn => {
                        if (isOn) {
                            statusesToAdd.Add(statusName, status);
                        } else {
                            statusesToAdd.Remove(statusName);
                        }

                        Refresh();
                    },
                    OnMoreInfo = status.ShowDescription
                });
            }

            listPopup.Populate(infoList,
                $"Add Statuses",
                () => {
                    Statuses = statusesToAdd;
                    _onRefresh();
                }
            );
        }
    }
}