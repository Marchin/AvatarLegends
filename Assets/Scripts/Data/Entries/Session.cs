using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class Session : IDataEntry {
    [JsonProperty("number")]
    public int Number;

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description;

    [JsonProperty("note")]
    public string Note;

    [JsonProperty("npcs")]
    public List<string> NPCs = new List<string>();

    [JsonProperty("pcs")]
    public List<string> PCs = new List<string>();

    [JsonProperty("engagement")]
    public List<Engagement> Engagements = new List<Engagement>();
    public Dictionary<string, Engagement> EngagementsByName {
        get {
            Dictionary<string, Engagement> engagements = new Dictionary<string, Engagement>(Engagements.Count);

            foreach (var engagement in Engagements) {
                engagements.Add(engagement.Name, engagement);
            }

            return engagements;
        }
        set {
            Engagements.Clear();
            
            foreach (var engagement in value) {
                Engagements.Add(engagement.Value);
            }
        }
    }

    private int _currentEngagementIndex;
    public int PreviousEngagementIndex => UnityUtils.Repeat(_currentEngagementIndex - 1, Engagements.Count) + 1;
    public int NextEngagementIndex => UnityUtils.Repeat(_currentEngagementIndex + 1, Engagements.Count) + 1;
    public int CurrentEngagementIndex {
        get => _currentEngagementIndex + 1;
        set => _currentEngagementIndex = UnityUtils.Repeat(value - 1, Engagements.Count);
    }
    public Engagement CurrentEngagement => (Engagements.Count > 0) ? Engagements[_currentEngagementIndex] : null;

    private Action _refresh;
    private bool _showDescription;
    private bool _showNotes;
    private bool _showNPCs;
    private bool _showPCs;
    private AppData Data => ApplicationManager.Instance.Data;
    private Campaign SelectedCampaign => Data.User.SelectedCampaign;

    public List<InformationData> RetrieveData(Action refresh, Action reload) {
        _refresh = refresh;

        List<InformationData> result = new List<InformationData>();

        result.Add(new InformationData {
            Prefix = nameof(Number),
            Content = Number.ToString(),
        });

        if (!string.IsNullOrEmpty(Description)) {
            Action onDescriptionDropdown = () => {
                _showDescription = !_showDescription;
                _refresh();
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
            Content = $"NPCs ({NPCs.Count})",
            OnDropdown = (NPCs.Count > 0) ? onNPCDropdown : null,
            OnAdd = (IDataEntry.GetAvailableEntries<NPC>(NPCs, Data.NPCs.Values).Count > 0) ?
                () => IDataEntry.AddEntry<NPC>(NPCs, Data.NPCs.Values, UpdateNPCs) :
                (Action)null,
            Expanded = _showNPCs
        });

        if (_showNPCs) {
            foreach (var npc in NPCs) {
                if (!Data.NPCs.ContainsKey(npc)) {
                    continue;
                }

                result.Add(new InformationData {
                    Content = $"{npc} ({Data.NPCs[npc].Alignment})",
                    OnDelete = () => {
                        MessagePopup.ShowConfirmationPopup(
                            $"Remove {npc} from the engagement?",
                            onYes: () => NPCs.Remove(npc)
                        );
                        _refresh();
                    },
                    OnMoreInfo = async () => {
                        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
                        Refresh();

                        void Refresh() {
                            listPopup.Populate(() => Data.NPCs[npc].RetrieveData(Refresh, Refresh), npc, null);
                        }
                    }
                });
            }
        }

        Action onPCDropdown = () => {
            _showPCs = !_showPCs;
            _refresh();
        };

        result.Add(new InformationData {
            Content = $"PCs ({PCs.Count})",
            OnDropdown = (PCs.Count > 0) ? onPCDropdown : null,
            OnAdd = (IDataEntry.GetAvailableEntries<PC>(PCs, SelectedCampaign.PCs.Values).Count > 0) ?
                () => IDataEntry.AddEntry<PC>(PCs, SelectedCampaign.PCs.Values, UpdatePCs) :
                (Action)null,
            Expanded = _showPCs
        });

        if (_showPCs) {
            foreach (var pc in PCs) {
                if (!SelectedCampaign.PCs.ContainsKey(pc)) {
                    continue;
                }

                result.Add(new InformationData {
                    Content = pc,
                    OnDelete = () => {
                        MessagePopup.ShowConfirmationPopup(
                            $"Remove {pc} from the engagement?",
                            onYes: () => PCs.Remove(pc)
                        );
                        _refresh();
                    },
                    OnMoreInfo = async () => {
                        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
                        Refresh();

                        void Refresh() {
                            listPopup.Populate(() => SelectedCampaign.PCs[pc].RetrieveData(Refresh, Refresh), pc, null);
                        }
                    }
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

    public List<NPC> GetNPCsData() {
        List<NPC> npcs = new List<NPC>(NPCs.Count);

        foreach (var npcName in NPCs) {
            if (SelectedCampaign.NPCs.ContainsKey(npcName)) {
                npcs.Add(SelectedCampaign.NPCs[npcName]);
            }
        }

        return npcs;
    }

    public List<PC> GetPCsData() {
        List<PC> pcs = new List<PC>(PCs.Count);

        foreach (var pcName in PCs) {
            if (SelectedCampaign.PCs.ContainsKey(pcName)) {
                pcs.Add(SelectedCampaign.PCs[pcName]);
            }
        }

        return pcs;
    }

    public List<string> GetEngagedNPCs() {
        List<string> results = new List<string>(NPCs.Count);

        foreach (var engagment in Engagements) {
            results.AddRange(engagment.NPCs);
        }

        return results;
    }
    
    public List<string> GetEngagedPCs() {
        List<string> results = new List<string>(PCs.Count);

        foreach (var engagment in Engagements) {
            results.AddRange(engagment.PCs);
        }

        return results;
    }
    
    public async void RemoveEngagements(Action onDone) {
        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
        List<string> engagementsToRemove = new List<string>(Engagements.Count);
        var infoList = new List<InformationData>(Engagements.Count);

        Refresh();

        void Refresh() {
            infoList.Clear();
            foreach (var engagement in Engagements) {
                infoList.Add(new InformationData {
                    Content = engagement.Name,
                    IsToggleOn = engagementsToRemove.Contains(engagement.Name),
                    OnToggle = isOn => {
                        if (isOn) {
                            engagementsToRemove.Add(engagement.Name);
                        } else {
                            engagementsToRemove.Remove(engagement.Name);
                        }
                        Refresh();
                    }
                });
            }

            listPopup.Populate(() => infoList,
                "Remove engagements",
                () => {
                    Engagements.RemoveAll(x => engagementsToRemove.Contains(x.Name));
                    onDone?.Invoke();
                }
            );
        }
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