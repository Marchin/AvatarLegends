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

    private Action _onRefresh;
    private bool _showNPCs;
    private bool _showPCs;
    private AppData Data => ApplicationManager.Instance.Data;
    private Campaign SelectedCampaign => Data.User.SelectedCampaign;
    public string NoteDisplay => !string.IsNullOrEmpty(Note) ? Note : "(Empty)";

    public List<InformationData> RetrieveData(Action refresh, Action reload) {
        _onRefresh = refresh;

        List<InformationData> result = new List<InformationData>();

        result.Add(new InformationData {
            Prefix = nameof(Number),
            Content = Number.ToString(),
        });

        if (!string.IsNullOrEmpty(Description)) {
            result.Add(new InformationData {
                Prefix = "Description",
                OnHoverIn = () => TooltipManager.Instance.ShowMessage(Description),
                OnHoverOut = TooltipManager.Instance.Hide,
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
                        _onRefresh();
                    },
                    OnMoreInfo = async () => {
                        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
                        Refresh();

                        void Refresh() {
                            listPopup.Populate(Data.NPCs[npc].RetrieveData(Refresh, Refresh), npc, null);
                        }
                    }
                });
            }
        }

        Action onPCDropdown = () => {
            _showPCs = !_showPCs;
            _onRefresh();
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
                        _onRefresh();
                    },
                    OnMoreInfo = async () => {
                        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
                        Refresh();

                        void Refresh() {
                            listPopup.Populate(SelectedCampaign.PCs[pc].RetrieveData(Refresh, Refresh), pc, null);
                        }
                    }
                });
            }
        }

        return result;

        void UpdateNPCs(List<string> newNPCs) {
            NPCs = newNPCs;
            _onRefresh();
        }

        void UpdatePCs(List<string> newPCs) {
            PCs = newPCs;
            _onRefresh();
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