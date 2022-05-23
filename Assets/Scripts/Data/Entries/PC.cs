using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[JsonObject(MemberSerialization.OptIn)]
public class PC : IDataEntry, IOnMoreInfo {
    public const int ConditionsAmount = 4;

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("player")]
    public string Player { get; set; }

    [JsonProperty("backstory")]
    public string Backstory;

    [JsonProperty("training")]
    public List<ETraining> Trainings = new List<ETraining>();

    [JsonProperty("playbook")]
    public string Playbook;

    [JsonProperty("note")]
    public string Note;
    
    [JsonProperty("connections")]
    public Dictionary<string, string> Connections = new Dictionary<string, string>();

    private bool _showConnections;
    private bool _showConditions;
    private bool _showTrainings;
    private Action _refresh;
    private AppData Data => ApplicationManager.Instance.Data;
    private Campaign SelectedCampaign => Data.User.SelectedCampaign;
    public string NoteDisplay => !string.IsNullOrEmpty(Note) ? Note : "(Empty)";
    public Action OnMoreInfo => ShowPCData;

    public List<InformationData> RetrieveData(Action refresh, Action reload) {
        _refresh = refresh;

        List<InformationData> result = new List<InformationData>();

        if (!string.IsNullOrEmpty(Player)) {
            result.Add(new InformationData {
                Prefix = nameof(Player),
                Content = Player,
            });
        }

        if (!string.IsNullOrEmpty(Backstory)) {
            result.Add(new InformationData {
                Prefix = nameof(Backstory),
                OnHoverIn = () => TooltipManager.Instance.ShowMessage(Backstory),
                OnHoverOut = TooltipManager.Instance.Hide,
            });
        }

        if (Data.Playbooks.ContainsKey(Playbook)) {
            result.Add(new InformationData {
                Prefix = nameof(Playbook),
                Content = Playbook,
                OnMoreInfo = async () => {
                    var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
                    Refresh();

                    void Refresh() {
                        listPopup.Populate(Data.Playbooks[Playbook].RetrieveData(Refresh, Refresh), Playbook, null);
                    }
                }
            });
        }

        result.Add(new InformationData {
            Content = nameof(Note),
            OnHoverIn = () => TooltipManager.Instance.ShowMessage(NoteDisplay),
            OnHoverOut = TooltipManager.Instance.Hide,
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

        Action onTrainingDropdown = () => {
            _showTrainings = !_showTrainings;
            _refresh();
        };

        result.Add(new InformationData {
            Content = nameof(Trainings),
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
                            onYes: () => Trainings.Remove(training)
                        );
                        _refresh();
                    },
                    IndentLevel = 1
                });
            }
        }

        if (Data.Playbooks.ContainsKey(Playbook)) {
            Action onConditionDropdown = () => {
                _showConditions = !_showConditions;
                _refresh();
            };

            result.Add(new InformationData {
                Content = "Conditions",
                OnDropdown = (Data.Playbooks[Playbook].Conditions.Count > 0) ? onConditionDropdown : null,
            });

            if (_showConditions) {
                foreach (var conditionName in Data.Playbooks[Playbook].Conditions) {
                    if (!Data.Conditions.ContainsKey(conditionName)) {
                        continue;
                    }

                    result.Add(new InformationData {
                        Content = conditionName,
                        OnHoverIn = () => TooltipManager.Instance.ShowMessage(Data.Conditions[conditionName].InfoDisplay),
                        OnHoverOut = TooltipManager.Instance.Hide,
                        IndentLevel = 1
                    });
                }
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
                                _refresh();
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

    public async void ShowPCData() {
        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
        RefreshInfo();

        void RefreshInfo() {
            listPopup.Populate(RetrieveData(RefreshInfo, RefreshInfo), Name, null);
        }
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
            
            listPopup.Populate(data, "Training", () => {
                 Trainings.AddRange(trainingsToAdd);
            });
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
                            listPopup.Populate(Data.NPCs[connection].RetrieveData(RefreshInfo, RefreshInfo), connection, null);
                        } else if (SelectedCampaign.PCs.ContainsKey(connection)) {
                            listPopup.Populate(
                                SelectedCampaign.PCs[connection].RetrieveData(RefreshInfo, RefreshInfo),
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
        
        var trainingFilter = new FilterChannelData(
            nameof(Trainings),
            entry => {
                PC pc = entry as PC;
                List<int> trainings = new List<int>(pc.Trainings.Count);

                foreach (var training in pc.Trainings) {
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
