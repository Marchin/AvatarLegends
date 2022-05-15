using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class Campaign : IDataEntry {
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description;

    [JsonProperty("note")]
    public string Note;

    [JsonProperty("npcs")]
    public Dictionary<string, NPC> NPCs = new Dictionary<string, NPC>();

    [JsonProperty("sesions")]
    public Dictionary<string, Session> Sessions = new Dictionary<string, Session>();

    [JsonIgnore] private string _currentSessionName;

    [JsonIgnore] public Session CurrentSession
    {
        get {
            if (!string.IsNullOrEmpty(_currentSessionName) && Sessions.ContainsKey(_currentSessionName)) {
                return Sessions[_currentSessionName];
            } else {
                return null;
            }
        }
        set {
            if ((value != null) && Sessions.ContainsKey(value.Name)) {
                _currentSessionName = value.Name;
            }
        }
    }

    [JsonIgnore] public Session LastSession {
        get {
            List<Session> sessions = new List<Session>(Sessions.Values);

            if (sessions.Count > 0) {
                sessions.Sort((x, y) => x.Number.CompareTo(y.Number));

                return sessions[0];
            } else {
                return null;
            }
        }
    }
    // PCs
    private Action _onRefresh;
    private bool _showNPCs;
    private bool _showSessions;
    public Action OnMoreInfo => null;
    private AppData Data => ApplicationManager.Instance.Data;

    public List<InformationData> RetrieveData(Action refresh) {
        _onRefresh = refresh;

        List<InformationData> result = new List<InformationData>();

        if (string.IsNullOrEmpty(Description)) {
            result.Add(new InformationData {
                Prefix = "Description",
                OnMoreInfo = () => MessagePopup.ShowMessage(Description, "Description", false),
            });
        }

        result.Add(new InformationData {
            Prefix = "Note",
            OnMoreInfo = () => MessagePopup.ShowMessage(Note, "Note", false),
        });

        Action onNPCDropdown = () => {
            _showNPCs = !_showNPCs;
            _onRefresh();
        };

        result.Add(new InformationData {
            Content = $"NPCs ({NPCs.Count})",
            OnDropdown = (NPCs.Count > 0) ? onNPCDropdown : null,
            Expanded = _showNPCs
        });

        if (_showNPCs) {
            foreach (var npcKVP in NPCs) {
                NPC npc = npcKVP.Value;
                result.Add(new InformationData {
                    Content = $"{npc.Name} ({npc.Alignment})",
                    OnDelete = async () => {
                        await MessagePopup.ShowConfirmationPopup(
                            $"Remove {npc.Name} from the engagement?",
                            onYes: () => NPCs.Remove(npcKVP.Key)
                        );
                        _onRefresh();
                    },
                    OnMoreInfo = async () => {
                        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
                        Refresh();

                        void Refresh() {
                            listPopup.Populate(npc.RetrieveData(Refresh), npc.Name, null);
                        }
                    }
                });
            }
        }

        Action onSessionDropdown = () => {
            _showSessions = !_showSessions;
            _onRefresh();
        };

        result.Add(new InformationData {
            Content = $"Sessions ({Sessions.Count})",
            OnDropdown = (Sessions.Count > 0) ? onSessionDropdown : null,
            Expanded = _showSessions
        });

        if (_showSessions) {
            foreach (var session in Sessions) {
                result.Add(new InformationData {
                    Content = session.Key,
                    OnDelete = async () => {
                        await MessagePopup.ShowConfirmationPopup(
                            $"Remove {session.Key} from the engagement?",
                            onYes: () => Sessions.Remove(session.Key)
                        );
                        _onRefresh();
                    },
                    OnMoreInfo = async () => {
                        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
                        Refresh();

                        void Refresh() {
                            listPopup.Populate(session.Value.RetrieveData(Refresh), session.Key, null);
                        }
                    }
                });
            }
        }

        return result;
    }
}