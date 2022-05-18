using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class Engagement : IDataEntry {
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("npcs")]
    public List<string> NPCs = new List<string>();

    [JsonProperty("pcs")]
    public List<string> PCs = new List<string>();

    [JsonProperty("note")]
    public string Note;

    private Action _onRefresh;
    private bool _showNPCs;
    private bool _showPCs;
    public Action OnMoreInfo => null;
    private AppData Data => ApplicationManager.Instance.Data;
    private Campaign SelectedCampaign => Data.User.SelectedCampaign;
    private Session CurrentSession => Data.User.CurrentSession;

    public List<InformationData> RetrieveData(Action refresh) {
        _onRefresh = refresh;

        var result = new List<InformationData>();

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
            Content = $"NPCs ({NPCs.Count}/{CurrentSession.NPCs.Count})",
            OnDropdown = (NPCs.Count > 0) ? onNPCDropdown : null,
            OnAdd = (IDataEntry.GetAvailableEntries<NPC>(NPCs, CurrentSession.GetNPCsData()).Count > 0) ?
                () => IDataEntry.AddEntry<NPC>(NPCs, CurrentSession.GetNPCsData(), UpdateNPCs)  :
                (Action)null,
            Expanded = _showNPCs
        });

        if (_showNPCs) {
            foreach (var npcName in NPCs) {
                if (!Data.NPCs.ContainsKey(npcName)) {
                    continue;
                }

                NPC npc = Data.NPCs[npcName];
                result.Add(new InformationData {
                    Content = $"{npcName} ({npc.Alignment})",
                    OnDelete = async () => {
                        await MessagePopup.ShowConfirmationPopup(
                            $"Remove {npcName} from the engagement?",
                            onYes: () => NPCs.Remove(npcName)
                        );
                        _onRefresh();
                    },
                    OnMoreInfo = async () => {
                        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
                        Refresh();

                        void Refresh() {
                            listPopup.Populate(npc.RetrieveData(Refresh), npcName, null);
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
            Content = $"PCs ({PCs.Count}/{CurrentSession.PCs.Count})",
            OnDropdown = (PCs.Count > 0) ? onPCDropdown : null,
            OnAdd = (IDataEntry.GetAvailableEntries<PC>(PCs, CurrentSession.GetPCsData()).Count > 0) ?
                () => IDataEntry.AddEntry<PC>(PCs, CurrentSession.GetPCsData(), UpdatePCs)  :
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
                    OnDelete = async () => {
                        await MessagePopup.ShowConfirmationPopup(
                            $"Remove {pcName} from the engagement?",
                            onYes: () => PCs.Remove(pcName)
                        );
                        _onRefresh();
                    },
                    OnMoreInfo = async () => {
                        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
                        Refresh();

                        void Refresh() {
                            listPopup.Populate(pc.RetrieveData(Refresh), pcName, null);
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
    
    public Filter GetFilterData() {
        Filter filter = new Filter();
        
        filter.Toggles.Add(new ToggleActionData(
            "Reverse",
            action: (list, isOn) => {
                if (isOn) {
                    list.Reverse();
                }
                return list;
            }
        ));

        return filter;
    }
}