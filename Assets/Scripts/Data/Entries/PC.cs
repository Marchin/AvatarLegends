using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class PC : IDataEntry {
    public const int ConditionsAmount = 4;

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("player")]
    public string Player { get; set; }

    [JsonProperty("backstory")]
    public string Backstory;

    [JsonProperty("training")]
    public ETraining Training;

    [JsonProperty("playbook")]
    public string Playbook;

    [JsonProperty("connections")]
    public Dictionary<string, string> Connections = new Dictionary<string, string>();

    private bool _showConnections;
    public Action OnMoreInfo => null;
    private Action _refresh;
    private AppData Data => ApplicationManager.Instance.Data;
    private Campaign SelectedCampaign => Data.User.SelectedCampaign;

    public List<InformationData> RetrieveData(Action refresh) {
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
                Content = Backstory,
            });
        }

        result.Add(new InformationData {
            Prefix = nameof(Training),
            Content = Training.ToString(),
        });

        if (Data.Playbooks.ContainsKey(Playbook)) {
            result.Add(new InformationData {
                Prefix = nameof(Playbook),
                Content = Playbook,
                OnMoreInfo = async () => {
                    var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
                    Refresh();

                    void Refresh() {
                        listPopup.Populate(Data.Playbooks[Playbook].RetrieveData(Refresh), Playbook, null);
                    }
                }
            });
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
                    OnToggle = isOn => {
                        if (isOn) {
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
        return null;
    }
}