using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class Engagement : IDataEntry {
    public int Number => CurrentSession.Engagements.IndexOf(this) + 1;

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("npcs")]
    public List<string> NPCs = new List<string>();

    [JsonProperty("pcs")]
    public List<string> PCs = new List<string>();

    [JsonProperty("note")]
    public string Note;

    private Action _refresh;
    private Action _reload;
    private bool _showNotes;
    private bool _showNPCs = true;
    private bool _showPCs = true;
    private Dictionary<string, bool> _showNPCData = new Dictionary<string, bool>();
    private static AppData Data => ApplicationManager.Instance.Data;
    private static Campaign SelectedCampaign => Data.User.SelectedCampaign;
    private static Session CurrentSession => Data.User.CurrentSession;

    public List<InformationData> RetrieveData(Action refresh, Action reload) {
        _refresh = refresh;
        _reload = reload;

        var result = new List<InformationData>();

        result.Add(new InformationData {
            Prefix = nameof(Number),
            InitValue = Number,
            MinValue = 1,
            MaxValue = CurrentSession.Engagements.Count,
            LoopValue = true,
            OnValueChange = value => {
                CurrentSession.Engagements.Remove(this);
                CurrentSession.Engagements.Insert(value - 1, this);
                _reload?.Invoke();
            }
        });

        Action onNotesDropdown = () => {
            _showNotes = !_showNotes;
            _refresh();
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
                        _refresh();
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

        Action onNPCDropdown = () => {
            _showNPCs = !_showNPCs;
            _refresh();
        };

        result.Add(new InformationData {
            Content = $"NPCs ({NPCs.Count}/{CurrentSession.NPCs.Count - CurrentSession.GetEngagedNPCs().Count + NPCs.Count})",
            OnDropdown = (NPCs.Count > 0) ? onNPCDropdown : null,
            OnAdd = (IDataEntry.GetAvailableEntries<NPC>(CurrentSession.GetEngagedNPCs(), CurrentSession.GetNPCsData()).Count > 0) ?
                () => IDataEntry.AddEntry<NPC>(CurrentSession.GetEngagedNPCs(), CurrentSession.GetNPCsData(), UpdateNPCs)  :
                (Action)null,
            Expanded = _showNPCs
        });

        if (_showNPCs) {
            foreach (var npcName in NPCs) {
                if (!Data.NPCs.ContainsKey(npcName)) {
                    continue;
                }
                if (!_showNPCData.ContainsKey(npcName)) {
                    _showNPCData.Add(npcName, false);
                }


                Action showNPC = () => {
                    _showNPCData[npcName] = !_showNPCData[npcName];
                    _refresh();
                };

                result.Add(new InformationData {
                    OnDropdown = showNPC,
                    Content = $"{npcName} ({Data.NPCs[npcName].Alignment})",
                    OnDelete = () => {
                        MessagePopup.ShowConfirmationPopup(
                            $"Remove {npcName} from the engagement?",
                            onYes: () => NPCs.Remove(npcName)
                        );
                        _refresh();
                    },
                    OnMoreInfo = async () => {
                        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
                        Refresh();

                        void Refresh() {
                            listPopup.Populate(() => Data.NPCs[npcName].RetrieveData(Refresh, Refresh), npcName, null);
                        }
                    },
                    IndentLevel = 1
                });

                if (_showNPCData[npcName]) {
                    result.AddRange(Data.NPCs[npcName].RetrieveCombatData(refresh, reload, indentLevel: 2));
                }
            }
        }

        Action onPCDropdown = () => {
            _showPCs = !_showPCs;
            _refresh();
        };

        result.Add(new InformationData {
            Content = $"PCs ({PCs.Count}/{CurrentSession.PCs.Count - CurrentSession.GetEngagedPCs().Count + PCs.Count})",
            OnDropdown = (PCs.Count > 0) ? onPCDropdown : null,
            OnAdd = (IDataEntry.GetAvailableEntries<PC>(CurrentSession.GetEngagedPCs(), CurrentSession.GetPCsData()).Count > 0) ?
                () => IDataEntry.AddEntry<PC>(CurrentSession.GetEngagedPCs(), CurrentSession.GetPCsData(), UpdatePCs)  :
                (Action)null,
            Expanded = _showPCs
        });

        if (_showPCs) {
            foreach (var pcName in PCs) {
                if (!CurrentSession.PCs.Contains(pcName)) {
                    continue;
                }

                PC pc = SelectedCampaign.PCs[pcName];
                result.Add(new InformationData {
                    Content = pcName,
                    OnDelete = () => {
                        MessagePopup.ShowConfirmationPopup(
                            $"Remove {pcName} from the engagement?",
                            onYes: () => PCs.Remove(pcName)
                        );
                        _refresh();
                    },
                    OnMoreInfo = async () => {
                        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
                        Refresh();

                        void Refresh() {
                            listPopup.Populate(() => pc.RetrieveData(Refresh, Refresh), pcName, null);
                        }
                    },
                    IndentLevel = 1
                });
            }
        }

        return result;

        void UpdateNPCs(List<string> newNPCs) {
            NPCs.AddRange(newNPCs);
            _refresh();
        }

        void UpdatePCs(List<string> newPCs) {
            PCs.AddRange(newPCs);
            _refresh();
        }
    }

    public static List<ButtonData> GetControllerButtons(Action refresh) {
        List<ButtonData> buttonData = new List<ButtonData>();

        buttonData.Add(new ButtonData {
            Text = $"Current ({CurrentSession.CurrentEngagementIndex})",
            Callback = () => {
                refresh?.Invoke();
            }
        });

        buttonData.Add(new ButtonData {
            Text = $"Next ({CurrentSession.NextEngagementIndex})",
            Callback = () => {
                ++CurrentSession.CurrentEngagementIndex;
                refresh?.Invoke();
            }
        });

        return buttonData;
    }
    
    public Filter GetFilterData() {
        Filter filter = new Filter();
        
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