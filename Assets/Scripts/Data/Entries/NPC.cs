using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[JsonObject(MemberSerialization.OptIn)]
public class NPC : IDataEntry, IOnMoreInfo {
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
    public List<ETraining> Trainings = new List<ETraining>();

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

    [JsonProperty("active_conditions")]
    public List<string> Conditions = new List<string>();
    
    [JsonProperty("connections")]
    public Dictionary<string, string> Connections = new Dictionary<string, string>();

    private bool _showDescription;
    private bool _showNotes;
    private bool _showConditions;
    private bool _showTrainings;
    private bool _showTechniques;
    private bool _showStatuses;
    private bool _showConnections;
    private AppData Data => ApplicationManager.Instance.Data;
    private Campaign SelectedCampaign => Data.User.SelectedCampaign;
    public Action OnMoreInfo => ShowNPCData;

    public List<InformationData> RetrieveData(Action refresh, Action reload) {
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
            Action onDescriptionDropdown = () => {
                _showDescription = !_showDescription;
                refresh();
            };

            result.Add(new InformationData {
                OnDropdown = onDescriptionDropdown,
                Content = nameof(Description),
                Expanded = _showDescription
            });

            if (_showDescription) {
                result.Add(new InformationData {
                    ExpandableContent = Description,
                    IndentLevel = 1
                });
            }
        }

        Action onNotesDropdown = () => {
            _showNotes = !_showNotes;
            refresh();
        };

        result.Add(new InformationData {
            Content = nameof(Note),
            OnDropdown = string.IsNullOrEmpty(Note) ? null : onNotesDropdown,
            OnEdit = async () => {
                var inputPopup = await PopupManager.Instance.GetOrLoadPopup<InputPopup>();
                inputPopup.Populate(
                    "",
                    nameof(Note),
                    input => {
                        Note = input;
                        PopupManager.Instance.Back();
                        refresh();
                    },
                    inputText: Note,
                    multiLine: true
                );
            },
            Expanded = _showNotes
        });

        if (_showNotes) {
            result.Add(new InformationData {
                ExpandableContent = Note,
                IndentLevel = 1
            });
        }

        Action onTrainingDropdown = () => {
            _showTrainings = !_showTrainings;
            refresh();
        };

        result.Add(new InformationData {
            Content = $"Trainings ({Trainings.Count})",
            OnDropdown = (Trainings.Count > 0) ? onTrainingDropdown : null,
            OnAdd = (Trainings.Count < Enum.GetValues(typeof(ETraining)).Length) ?
                AddTraining :
                (Action)null,
            Expanded = _showTrainings
        });

        if (_showTrainings) {
            foreach (var training in Trainings) {
                result.Add(new InformationData {
                    Content = training.ToString(),
                    OnDelete = () => {
                        MessagePopup.ShowConfirmationPopup(
                            $"Remove {training} training?",
                            onYes: () => {
                                Trainings.Remove(training);
                                var learnableTechniques = GetLearnableTechniques();
                                for (int iTechnique = 0; iTechnique < Techniques.Count;) {
                                    if (learnableTechniques.Find(t => t.Name == Techniques[iTechnique]) == null) {
                                        Techniques.Remove(Techniques[iTechnique]);
                                    } else {
                                        ++iTechnique;
                                    }
                                }
                
                                refresh?.Invoke();
                            },
                            restore: false
                        );
                        refresh();
                    },
                    IndentLevel = 1
                });
            }
        }

        Action onConnectionDropdown = () => {
            _showConnections = !_showConnections;
            refresh();
        };

        result.AddRange(RetrieveCombatData(refresh, reload));

        result.Add(new InformationData {
            Content = $"Connections ({Connections.Count})",
            OnDropdown = (Connections.Count > 0) ? onConnectionDropdown : null,
            OnAdd = (Connections.Count < GetMaxConnections()) ?
                () => AddConnection(refresh) :
                (Action)null,
            Expanded = _showConnections
        });

        if (_showConnections) {
            List<string> connectionNames = new List<string>(Connections.Keys);
            connectionNames.Sort();
            foreach (var connectionName in connectionNames) {
                result.Add(new InformationData {
                    Content = connectionName,
                    OnHoverIn = () => TooltipManager.Instance.ShowMessage(Connections[connectionName]),
                    OnHoverOut = TooltipManager.Instance.Hide,
                    OnEdit = async () => {
                        var inputPopup = await PopupManager.Instance.GetOrLoadPopup<InputPopup>();
                        inputPopup.Populate(
                            "",
                            connectionName,
                            input => {
                                EditConnection(connectionName, input);
                                PopupManager.Instance.Back();
                                refresh();
                            },
                            inputText: Connections[connectionName],
                            multiLine: true
                        );
                    },
                    OnDelete = () => {
                        MessagePopup.ShowConfirmationPopup(
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
                        refresh();
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

    public List<InformationData> RetrieveCombatData(Action refresh, Action reload, int indentLevel = 0) {
        List<InformationData> result = new List<InformationData>();

        result.Add(new InformationData {
            Prefix = "Balance",
            Content = string.IsNullOrEmpty(Principle) ? "(none)" : Principle,
            InitValue = Balance,
            MaxValue = GetMaxBalance(),
            OnValueChange = ChangeBalance,
            IndentLevel = indentLevel
        });

        result.Add(new InformationData {
            Content = $"Fatigue",
            InitValue = Fatigue,
            MaxValue = GetMaxFatigue(),
            OnValueChange = ChangeFatigue,
            IndentLevel = indentLevel
        });

        Action onConditionDropdown = () => {
            _showConditions = !_showConditions;
            refresh();
        };

        result.Add(new InformationData {
            Content = $"Conditions ({Conditions.Count}/{GetMaxConditions()})",
            OnDropdown = onConditionDropdown,
            Expanded = _showConditions,
            IndentLevel = indentLevel
        });

        if (_showConditions) {
            foreach (var condition in Data.Conditions) {
                result.Add(new InformationData {
                    Content = condition.Key,
                    IsToggleOn = Conditions.Contains(condition.Key),
                    OnToggle = on => {
                        if (!on || (Conditions.Count < GetMaxConditions())) {
                            if (on) {
                                Conditions.Add(condition.Key);
                            } else {
                                Conditions.Remove(condition.Key);
                            }
                        } else {
                            MessagePopup.ShowMessage("Amount of conditions exceeded! NPC is down!", "NPC Down", restore: false);
                        }
                        refresh?.Invoke();
                    },
                    IndentLevel = indentLevel + 1
                });
            }
        }

        Action onTechniqueDropdown = () => {
            _showTechniques = !_showTechniques;
            refresh();
        };

        result.Add(new InformationData {
            Content = $"Techniques ({Techniques.Count}/{GetMaxTechniques()})",
            OnDropdown = (Techniques.Count > 0) ? onTechniqueDropdown : null,
            OnAdd = (Techniques.Count < GetMaxTechniques()) ?
                () => AddTechnique(refresh) :
                (Action)null,
            Expanded = _showTechniques,
            IndentLevel = indentLevel
        });

        if (_showTechniques) {
            foreach (var techniqueName in Techniques) {
                Technique technique = Data.Techniques[techniqueName];
                result.Add(new InformationData {
                    Content = technique.ColoredName,
                    OnHoverIn = () => TooltipManager.Instance.ShowMessage(technique.InfoDisplay),
                    OnHoverOut = TooltipManager.Instance.Hide,
                    OnDelete = () => {
                        MessagePopup.ShowConfirmationPopup(
                            $"Remove {techniqueName} technique?",
                            onYes: () => {
                                Techniques.Remove(techniqueName);
                                refresh?.Invoke();
                            },
                            restore: false
                        );
                        refresh();
                    },
                    IndentLevel = indentLevel + 1
                });
            }
        }

        Action onStatusesDropdown = () => {
            _showStatuses = !_showStatuses;
            refresh();
        };

        result.Add(new InformationData {
            Content = $"Statuses ({Statuses.Count})",
            OnDropdown = onStatusesDropdown,
            Expanded = _showStatuses,
            IndentLevel = indentLevel
        });

        foreach (var status in Data.Statuses) {
            if (!_showStatuses && !Statuses.Contains(status.Key)) {
                continue;
            }

            string effect = status.Value.Positive ? "Positive" : "Negative";
            result.Add(new InformationData {
                Content = $"{status.Key} ({effect})",
                IsToggleOn = Statuses.Contains(status.Key),
                OnToggle = on => {
                    if (on) {
                        Statuses.Add(status.Key);
                    } else {
                        Statuses.Remove(status.Key);
                    }

                    refresh?.Invoke();
                },
                OnHoverIn = () => TooltipManager.Instance.ShowMessage(status.Value.Description),
                OnHoverOut = TooltipManager.Instance.Hide,
                OnDelete = () => {
                    MessagePopup.ShowConfirmationPopup(
                        $"Remove {status.Key} status?",
                        onYes: () => Statuses.Remove(status.Key)
                    );
                    refresh();
                },
                IndentLevel = indentLevel + 1
            });
        }

        return result;
    }

    public async void ShowNPCData() {
        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
        RefreshInfo();

        void RefreshInfo() {
            listPopup.Populate(() => RetrieveData(RefreshInfo, RefreshInfo), Name, null);
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

    private async void AddTraining() {
        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
        var trainings = Enum.GetValues(typeof(ETraining)) as ETraining[];
        Refresh();

        void Refresh() {
            List<InformationData> data = new List<InformationData>();
            List<ETraining> trainingsToAdd = new List<ETraining>();

            foreach (var training in trainings) {
                if (!Trainings.Contains(training)) {
                    data.Add(new InformationData {
                        Content = training.ToString(),
                        IsToggleOn = trainingsToAdd.Contains(training),
                        OnToggle = isOn => {
                            if (isOn) {
                                trainingsToAdd.Add(training);
                            } else {
                                trainingsToAdd.Remove(training);
                            }
                        }
                    });
                }
            }
            
            listPopup.Populate(() => data, "Training", () => Trainings.AddRange(trainingsToAdd));
        }
    }

    public void ChangeFatigue(int value) {
        int maxFatigue = GetMaxFatigue();
        UnityEngine.Debug.Assert((value >= 0) && (value <= maxFatigue), "Invalid Balance");

        Fatigue = UnityEngine.Mathf.Clamp(value, 0, maxFatigue);
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

    public void AddTechnique(Action refresh) {
        var availableTechniques = IDataEntry.GetAvailableEntries<Technique>(Techniques, GetLearnableTechniques());
        if (availableTechniques.Count > 0) {
            availableTechniques.Sort((x, y) => {
                if (x.Mastery != y.Mastery) {
                    return x.Mastery.CompareTo(y.Mastery);
                } else if (x.Approach != y.Approach) {
                    return x.Approach.CompareTo(y.Approach);
                } else {
                    return x.Name.CompareTo(y.Name);
                }
            });

            Action<List<string>> onDone = techniquesToAdd => {
                Techniques.AddRange(techniquesToAdd);
                Techniques.Sort();
                refresh?.Invoke();
            };

            IDataEntry.AddEntry<Technique>(
                Techniques, 
                availableTechniques, 
                onDone,
                "Add Techniques",
                GetMaxTechniques(),
                entry => (entry as Technique).ColoredName);
        } else {
            MessagePopup.ShowMessage("No more techniques available, add more under the \"Techniques\" tab.", "Techniques");
        }
    }

    public int GetMaxTechniques() {
        int result = 0;

        switch (Type) {
            case EType.Minor: {
                result = 1;
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

    public List<Technique> GetLearnableTechniques() {
        List<Technique> results = new List<Technique>(64);

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
                result = Group;
            } break;
            case Technique.EMastery.Air: {
                result = Trainings.Contains(ETraining.Air);
            } break;
            case Technique.EMastery.Earth: {
                result = Trainings.Contains(ETraining.Earth);
            } break;
            case Technique.EMastery.Water: {
                result = Trainings.Contains(ETraining.Water);
            } break;
            case Technique.EMastery.Fire: {
                result = Trainings.Contains(ETraining.Fire);
            } break;
            case Technique.EMastery.Weapons: {
                result = Trainings.Contains(ETraining.Weapons);
            } break;
            case Technique.EMastery.Technology: {
                result = Trainings.Contains(ETraining.Technology);
            } break;
        }

        return result;
    }
    
    private async void AddStatus(Action refresh) {
        List<string> statusesToAdd = new List<string>();
        List<InformationData> infoList = new List<InformationData>(Data.Statuses.Count);
        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
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
                    OnHoverIn = () => TooltipManager.Instance.ShowMessage(status.Description),
                    OnHoverOut = TooltipManager.Instance.Hide,
                });
            }

            listPopup.Populate(() => infoList,
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

                    refresh?.Invoke();
                }
            );
        }
    }

    private async void AddConnection(Action refresh) {
        Dictionary<string, string> connectionsToAdd = new Dictionary<string, string>(GetMaxConnections());
        List<InformationData> infoList = new List<InformationData>(GetMaxConnections());
        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
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
                            listPopup.Populate(() => Data.NPCs[connection].RetrieveData(RefreshInfo, RefreshInfo), connection, null);
                        } else if (SelectedCampaign.PCs.ContainsKey(connection)) {
                            listPopup.Populate(
                                () => SelectedCampaign.PCs[connection].RetrieveData(RefreshInfo, RefreshInfo),
                                connection,
                                null);
                        } else {
                            UnityEngine.Debug.LogWarning("Character not found.");
                        }
                    }
                }
            }

            listPopup.Populate(() => infoList,
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

                    refresh?.Invoke();
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

    public void Restore() {
        Fatigue = 0;
        Balance = 0;
        Conditions.Clear();
        Statuses.Clear();
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
            nameof(Trainings),
            entry => {
                NPC npc = entry as NPC;
                List<int> trainings = new List<int>(npc.Trainings.Count);

                foreach (var training in npc.Trainings) {
                    trainings.Add((int)training);
                }
                
                return trainings;
            }
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