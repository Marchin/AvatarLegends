using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class Engagement : IDataEntry {
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("npcs")]
    public List<NPC> NPCs = new List<NPC>();

    [JsonProperty("note")]
    public string Note;

    private Action _onRefresh;
    private bool _showNPCs;
    public Action OnMoreInfo => null;
    private AppData Data => ApplicationManager.Instance.Data;

    public List<InformationData> RetrieveData(Action refresh) {
        _onRefresh = refresh;

        var result = new List<InformationData>();

        result.Add(new InformationData {
            Content = "Note",
            OnMoreInfo = ShowNote
        });

        Action onNPCDropdown = () => {
            _showNPCs = !_showNPCs;
            _onRefresh();
        };

        result.Add(new InformationData {
            Content = $"NPCs ({NPCs.Count})",
            OnDropdown = (NPCs.Count > 0) ? onNPCDropdown : null,
            OnAdd = (GetAvailableNPCs().Count > 0) ?
                AddNPC :
                (Action)null,
            Expanded = _showNPCs
        });

        if (_showNPCs) {
            foreach (var npc in NPCs) {
                result.Add(new InformationData {
                    Content = $"{npc.Name} ({npc.Alignment})",
                    OnDelete = async () => {
                        await MessagePopup.ShowConfirmationPopup(
                            $"Remove {npc.Name} from the engagement?",
                            onYes: () => NPCs.Remove(npc)
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

        return result;
    }

    public async void ShowNote() {
        var msgPopup = await PopupManager.Instance.GetOrLoadPopup<MessagePopup>(restore: false);
        msgPopup.Populate(Note, "Note");
    }

    private async void AddNPC() {
        List<NPC> npcsToAdd = new List<NPC>(NPCs);
        List<InformationData> infoList = new List<InformationData>(Data.NPCs.Count);
        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>(restore: false);
        List<NPC> availableNPCs = GetAvailableNPCs();

        Refresh();

        void Refresh() {
            infoList.Clear();
            foreach (var npc in availableNPCs) {
                infoList.Add(new InformationData {
                    Content = npc.Name,
                    IsToggleOn = npcsToAdd.Contains(npc),
                    OnToggle = isOn => {
                        if (isOn) {
                            npcsToAdd.Add(npc);
                        } else {
                            npcsToAdd.Remove(npc);
                        }

                        Refresh();
                    },
                    OnMoreInfo = string.IsNullOrEmpty(npc.Description) ?
                        (Action)null :
                        npc.ShowDescription
                });
            }

            listPopup.Populate(infoList,
                $"Add NPCs",
                () => {
                    NPCs = npcsToAdd;
                    _onRefresh();
                }
            );
        }
    }

    private List<NPC> GetAvailableNPCs() {
        List<NPC> availableNPCs = new List<NPC>(Data.NPCs.Values);

        foreach (var engagement in Data.User.CurrentSession.Engagements) {
            foreach (var npc in engagement.Value.NPCs) {
                availableNPCs.Remove(availableNPCs.Find(x => x.Name == npc.Name));
            }
        }

        return availableNPCs;
    }
}