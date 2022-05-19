using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class NPC : IDataEntry {
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

    [JsonProperty("group")]
    public bool Group;

    [JsonProperty("training")]
    public ETraining Training;

    [JsonProperty("principle")]
    public string Principle;

    [JsonProperty("balance")]
    public int Balance;
    
    [JsonProperty("fatigue")]
    public int Fatigue;

    [JsonProperty("note")]
    public string Note;
    
    [JsonProperty("techniques")]
    public List<string> Techniques = new List<string>();
    
    [JsonProperty("statuses")]
    public List<string> Statuses = new List<string>();

    [JsonProperty("conditions")]
    public Dictionary<string, ConditionState> Conditions = new Dictionary<string, ConditionState>();
    
    [JsonProperty("connections")]
    public Dictionary<string, string> Connections = new Dictionary<string, string>();

    private bool _showConditions;
    private bool _showTechinques;
    private bool _showStatuses;
    private bool _showConnections;
    private Action _refresh;
    public Action OnMoreInfo => !string.IsNullOrEmpty(Description) ? ShowDescription : null;
    private AppData Data => ApplicationManager.Instance.Data;
    private Campaign SelectedCampaign => Data.User.SelectedCampaign;

    public List<InformationData> RetrieveData(Action refresh) {
        _refresh = refresh;

        List<InformationData> result = new List<InformationData>();

        result.Add(new InformationData {
            Prefix = "Type",
            Content = Type.ToString(),
        });

        result.Add(new InformationData {
            Content = "Group",
            IsToggleOn = Group,
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
            Content = nameof(Note),
            OnMoreInfo = !string.IsNullOrEmpty(Note) ?
                () => MessagePopup.ShowMessage(Note, nameof(Note), false) :
                null,
            OnEdit = async () => {
                var inputPopup = await PopupManager.Instance.GetOrLoadPopup<InputPopup>();
                inputPopup.Populate(
                    "",
                    nameof(Note),
                    input => {
                        Note = input;
                        PopupManager.Instance.Back();
                        _refresh();
                    },
                    inputText: Note,
                    multiLine: true
                );
            }
        });

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
            _refresh();
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
                    IsToggleOn = condition.On,
                    OnToggle = on => Conditions[conditionName].On = on,
                    OnDelete = async () => {
                        await MessagePopup.ShowConfirmationPopup(
                            $"Remove {conditionName} condition?",
                            onYes: () => Conditions.Remove(conditionName)
                        );
                        _refresh();
                    },
                    IndentLevel = 1
                });
            }
        }

        Action onTechniqueDropdown = () => {
            _showTechinques = !_showTechinques;
            _refresh();
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
            foreach (var techniqueName in Techniques) {
                Technique technique = Data.Techniques[techniqueName];
                result.Add(new InformationData {
                    Content = techniqueName,
                    OnMoreInfo = technique.ShowInfo,
                    OnDelete = async () => {
                        await MessagePopup.ShowConfirmationPopup(
                            $"Remove {techniqueName} technique?",
                            onYes: () => Techniques.Remove(techniqueName)
                        );
                        _refresh();
                    },
                    IndentLevel = 1
                });
            }
        }

        Action onStatusesDropdown = () => {
            _showStatuses = !_showStatuses;
            _refresh();
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
            foreach (var statusName in Statuses) {
                Status status = Data.Statuses[statusName];
                string effect = status.Positive ? "Positive" : "Negative";
                result.Add(new InformationData {
                    Content = $"{statusName} ({effect})",
                    OnMoreInfo = status.ShowDescription,
                    OnDelete = async () => {
                        await MessagePopup.ShowConfirmationPopup(
                            $"Remove {statusName} status?",
                            onYes: () => Statuses.Remove(statusName)
                        );
                        _refresh();
                    },
                    IndentLevel = 1
                });
            }
        }

        Action onConnectionDropdown = () => {
            _showConnections = !_showConnections;
            _refresh();
        };

        result.Add(new InformationData {
            Content = $"Connections ({Connections.Count})",
            OnDropdown = (Connections.Count > 0) ? onConnectionDropdown : null,
            OnAdd = (Connections.Count < GetMaxConnections()) ?
                AddConnection :
                (Action)null,
            Expanded = _showConnections
        });

        if (_showConnections) {
            List<string> connectionNames = new List<string>(Connections.Keys);
            connectionNames.Sort();
            foreach (var connectionName in connectionNames) {
                result.Add(new InformationData {
                    Content = connectionName,
                    OnMoreInfo = () => MessagePopup.ShowMessage(Connections[connectionName], connectionName, false),
                    OnEdit = async () => {
                        var inputPopup = await PopupManager.Instance.GetOrLoadPopup<InputPopup>();
                        inputPopup.Populate(
                            "",
                            connectionName,
                            input => {
                                EditConnection(connectionName, input);
                                PopupManager.Instance.Back();
                                _refresh();
                            },
                            inputText: Connections[connectionName],
                            multiLine: true
                        );
                    },
                    OnDelete = async () => {
                        await MessagePopup.ShowConfirmationPopup(
                            $"Remove {connectionName} connection?",
                            onYes: () => {
                                Connections.Remove(connectionName);

                                if (Data.NPCs.ContainsKey(connectionName)) {
                                    Data.NPCs[connectionName].Connections.Remove(Name);
                                } else if (SelectedCampaign.PCs.ContainsKey(connectionName)) {
                                    SelectedCampaign.PCs[connectionName].Connections.Remove(Name);
                                }
                            }
                        );
                        _refresh();
                    },
                    IndentLevel = 1
                });

                void EditConnection(string connectionName, string description) {
                    Connections[connectionName] = description;

                    if (Data.NPCs.ContainsKey(connectionName)) {
                        Data.NPCs[connectionName].Connections[Name] = description;
                    } else if (SelectedCampaign.PCs.ContainsKey(connectionName)) {
                        SelectedCampaign.PCs[connectionName].Connections[Name] = description;
                    }
                }
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
                _refresh();
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
        _refresh();
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
        List<string> techniquesToAdd = new List<string>(Data.Techniques.Count);
        List<InformationData> infoList = new List<InformationData>(Data.Techniques.Count);
        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>(restore: false);
        var availableTechniques = GetLearnableTechniques();

        foreach (var technique in Techniques) {
            availableTechniques.Remove(technique);
        }

        availableTechniques.Sort();

        Refresh();

        void Refresh() {
            infoList.Clear();
            foreach (var techniqueName in availableTechniques) {
                Technique technique = Data.Techniques[techniqueName];
                infoList.Add(new InformationData {
                    Content = technique.Name,
                    IsToggleOn = techniquesToAdd.Contains(technique.Name),
                    OnToggle = async on => {
                        if (!on || (techniquesToAdd.Count < GetMaxTechniques())) {
                            if (on) {
                                techniquesToAdd.Add(technique.Name);
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
                $"Add Techniques ({techniquesToAdd.Count}/{availableTechniques.Count})",
                () => {
                    Techniques.AddRange(techniquesToAdd);
                    Techniques.Sort();
                    _refresh();
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

    public List<string> GetLearnableTechniques() {
        List<string> results = new List<string>(100);

        foreach (var technique in Data.Techniques) {
            if (CanLearnTechnique(technique.Value)) {
                results.Add(technique.Key);
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
                result = Group;
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
        List<string> statusesToAdd = new List<string>();
        List<InformationData> infoList = new List<InformationData>(Data.Statuses.Count);
        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>(restore: false);
        var availableStatuses = new List<string>(Data.Statuses.Keys);

        foreach (var status in Statuses) {
            availableStatuses.Remove(status);
        }

        availableStatuses.Sort((x, y) => {
            if (Data.Statuses[x].Positive != Data.Statuses[y].Positive) {
                return Data.Statuses[x].Positive ? -1 : 1;
            } else {
                return x.CompareTo(y);
            }
        });

        Refresh();

        void Refresh() {
            infoList.Clear();

            foreach (var statusName in availableStatuses) {
                Status status = Data.Statuses[statusName];
                string effect = status.Positive ? "Positive" : "Negative";
                infoList.Add(new InformationData {
                    Content = statusName + $" ({effect})",
                    IsToggleOn = statusesToAdd.Contains(statusName),
                    OnToggle = on => {
                        if (on) {
                            statusesToAdd.Add(statusName);
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
                    Statuses.AddRange(statusesToAdd);
                    Statuses.Sort((x, y) => {
                        if (Data.Statuses[x].Positive != Data.Statuses[y].Positive) {
                            return Data.Statuses[x].Positive ? -1 : 1;
                        } else {
                            return x.CompareTo(y);
                        }
                    });

                    _refresh();
                }
            );
        }
    }

    public async void AddConnection() {
        Dictionary<string, string> connectionsToAdd = new Dictionary<string, string>(GetMaxConnections());
        List<InformationData> infoList = new List<InformationData>(GetMaxConnections());
        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>(restore: false);
        var availableConnections = GetAvailableConnections();

        foreach (var connection in Connections) {
            availableConnections.Remove(connection.Value);
        }

        availableConnections.Sort();

        Refresh();

        void Refresh() {
            infoList.Clear();
            foreach (var connection in availableConnections) {
                infoList.Add(new InformationData {
                    Content = GetDisplayName(connection),
                    IsToggleOn = connectionsToAdd.ContainsKey(connection),
                    OnToggle = on => {
                        if (on) {
                            connectionsToAdd.Add(connection, "(No connection yet).");
                        } else {
                            connectionsToAdd.Remove(connection);
                        }

                        Refresh();
                    },
                    OnMoreInfo = () => OnMoreInfo(connection)
                });

                string GetDisplayName(string connection) {
                    string result = connection;

                    if (Data.NPCs.ContainsKey(connection)) {
                        connection += $" (NPC) ({Data.NPCs[connection].Alignment})";
                    } else if (SelectedCampaign.PCs.ContainsKey(connection)) {
                        connection += $" (PC) ({SelectedCampaign.PCs[connection].Player})";
                    } else {
                        UnityEngine.Debug.LogWarning("Character not found.");
                    }

                    return result;
                }

                async void OnMoreInfo(string connection) {
                    var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
                    RefreshInfo();

                    void RefreshInfo() {
                        if (Data.NPCs.ContainsKey(connection)) {
                            listPopup.Populate(Data.NPCs[connection].RetrieveData(RefreshInfo), connection, null);
                        } else if (SelectedCampaign.PCs.ContainsKey(connection)) {
                            listPopup.Populate(
                                SelectedCampaign.PCs[connection].RetrieveData(RefreshInfo),
                                connection,
                                null);
                        } else {
                            UnityEngine.Debug.LogWarning("Character not found.");
                        }
                    }
                }
            }

            listPopup.Populate(infoList,
                $"Add Connections ({connectionsToAdd.Count}/{availableConnections.Count})",
                () => {
                    foreach (var connection in connectionsToAdd) {
                        Connections.Add(connection.Key, connection.Value);

                        if (Data.NPCs.ContainsKey(connection.Key)) {
                            Data.NPCs[connection.Key].Connections[Name] = connection.Value;
                        } else if (SelectedCampaign.PCs.ContainsKey(connection.Key)) {
                            SelectedCampaign.PCs[connection.Key].Connections[Name] = connection.Value;
                        }
                    }

                    _refresh();
                }
            );
        }
    }

    public List<string> GetAvailableConnections() {
       List<string> results = new List<string>(GetMaxConnections());

        foreach (var npc in Data.NPCs) {
            if (!Connections.ContainsKey(npc.Key)) {
                results.Add(npc.Key);
            }
        }

        foreach (var pc in SelectedCampaign.PCs) {
            if (!Connections.ContainsKey(pc.Key)) {
                results.Add(pc.Key);
            }
        }
        
        results.Remove(Name);

        return results;
    }

    private int GetMaxConnections() {
        // Susbtract 1 since you can't have a connection with yourself
        int result = Data.NPCs.Count + SelectedCampaign.PCs.Count - 1;

        return result;
    }

    public Filter GetFilterData() {
        Filter filter = new Filter();

        var typeFilter = new FilterChannelData(
            nameof(Type),
            entry => new List<int> { (int)(entry as NPC).Type }
        );

        string[] types = Enum.GetNames(typeof(EType));
        typeFilter.Elements = new List<FilterChannelEntryData>(types.Length);
        for (int iType = 0; iType < types.Length; ++iType) {
            typeFilter.Elements.Add(
                new FilterChannelEntryData { Name = ((EType)iType).ToString() });
        }

        filter.Filters.Add(typeFilter);


        var alignmentFilter = new FilterChannelData(
            nameof(Alignment),
            entry => new List<int> { (int)(entry as NPC).Alignment }
        );

        string[] alignments = Enum.GetNames(typeof(EAlignment));
        alignmentFilter.Elements = new List<FilterChannelEntryData>(alignments.Length);
        for (int iAlignment = 0; iAlignment < alignments.Length; ++iAlignment) {
            alignmentFilter.Elements.Add(
                new FilterChannelEntryData { Name = ((EAlignment)iAlignment).ToString() });
        }

        filter.Filters.Add(alignmentFilter);


        var trainingFilter = new FilterChannelData(
            nameof(Training),
            entry => new List<int> { (int)(entry as NPC).Training }
        );

        string[] trainings = Enum.GetNames(typeof(ETraining));
        trainingFilter.Elements = new List<FilterChannelEntryData>(trainings.Length);
        for (int iTraining = 0; iTraining < trainings.Length; ++iTraining) {
            trainingFilter.Elements.Add(
                new FilterChannelEntryData { Name = ((ETraining)iTraining).ToString() });
        }

        filter.Filters.Add(trainingFilter);


        filter.Toggles.Add(new ToggleActionData(
            "Is Group",
            action: (list, on) => {
                if (on) {
                    list.RemoveAll(x => !(x as NPC).Group);
                }
                return list;
            }
        ));

        filter.Toggles.Add(new ToggleActionData(
            "Is Not Group",
            action: (list, on) => {
                if (on) {
                    list.RemoveAll(x => (x as NPC).Group);
                }
                return list;
            }
        ));

        filter.Toggles.Add(new ToggleActionData(
            "Reverse",
            action: (list, on) => {
                if (on) {
                    list.Reverse();
                }
                return list;
            }
        ));

        return filter;
    }
}