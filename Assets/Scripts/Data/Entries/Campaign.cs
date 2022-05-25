using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class Campaign : IDataEntry {
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description;

    [JsonProperty("note")]
    public string Note;

    [JsonProperty("npcs")]
    public Dictionary<string, NPC> NPCs = new Dictionary<string, NPC>();

    [JsonProperty("pcs")]
    public Dictionary<string, PC> PCs = new Dictionary<string, PC>();

    [JsonProperty("sesions")]
    public Dictionary<string, Session> Sessions = new Dictionary<string, Session>();

    private string _currentSessionName;

    public Session CurrentSession
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

    public Session LastSession {
        get {
            List<Session> sessions = new List<Session>(Sessions.Values);

            if (sessions.Count > 0) {
                sessions.Sort((x, y) => y.Number.CompareTo(x.Number));

                return sessions[0];
            } else {
                return null;
            }
        }
    }

    private Action _onRefresh;
    private bool _showNPCs;
    private bool _showPCs;
    private bool _showSessions;
    private AppData Data => ApplicationManager.Instance.Data;
    public string DescriptionDisplay => !string.IsNullOrEmpty(Description) ? Description : "(Empty)";
    public string NoteDisplay => !string.IsNullOrEmpty(Note) ? Note : "(Empty)";

    public List<InformationData> RetrieveData(Action refresh, Action reload) {
        _onRefresh = refresh;

        List<InformationData> result = new List<InformationData>();

        result.Add(new InformationData {
            Prefix = nameof(Name),
            Content = Name,
            OnEdit = async () => {
                var inputPopup = await PopupManager.Instance.GetOrLoadPopup<InputPopup>(restore: false);
                inputPopup.Populate(
                    "",
                    nameof(Name),
                    input => {
                        Data.User.Campaigns.Remove(Name);
                        Name = input;
                        Data.User.Campaigns.Add(input, this);
                        Data.User.SelectedCampaignName = input;
                        reload();
                        PopupManager.Instance.Back();
                    },
                    inputText: Name,
                    multiLine: true
                );
            }
        });

        result.Add(new InformationData {
            Content = nameof(Description),
            OnHoverIn = () => TooltipManager.Instance.ShowMessage(DescriptionDisplay),
            OnHoverOut = TooltipManager.Instance.Hide,
            OnEdit = async () => {
                var inputPopup = await PopupManager.Instance.GetOrLoadPopup<InputPopup>();
                inputPopup.Populate(
                    "",
                    nameof(Description),
                    input => {
                        Description = input;
                        PopupManager.Instance.Back();
                        _onRefresh();
                    },
                    inputText: Description,
                    multiLine: true
                );
            }
        });

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
                        _onRefresh();
                    },
                    inputText: Note,
                    multiLine: true
                );
            }
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
                    OnDelete = () => {
                        MessagePopup.ShowConfirmationPopup(
                            $"Remove {npc.Name} from the engagement?",
                            onYes: () => NPCs.Remove(npcKVP.Key)
                        );
                        _onRefresh();
                    },
                    OnMoreInfo = async () => {
                        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
                        Refresh();

                        void Refresh() {
                            listPopup.Populate(() => npc.RetrieveData(Refresh, Refresh), npc.Name, null);
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
                    OnDelete = () => {
                        MessagePopup.ShowConfirmationPopup(
                            $"Remove {session.Key} from the engagement?",
                            onYes: () => Sessions.Remove(session.Key)
                        );
                        _onRefresh();
                    },
                    OnMoreInfo = async () => {
                        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
                        Refresh();

                        void Refresh() {
                            listPopup.Populate(() => session.Value.RetrieveData(Refresh, Refresh), session.Key, null);
                        }
                    }
                });
            }
        }

        return result;
    }
    
    public Filter GetFilterData() {
        return null;
    }
}
