using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

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
    public List<NPC> NPCs = new List<NPC>();

    [JsonProperty("engagement")]
    public Dictionary<string, Engagement> Engagements = new Dictionary<string, Engagement>();

    private Action _onRefresh;
    private bool _showNPCs;
    private AppData Data => ApplicationManager.Instance.Data;

    public List<InformationData> RetrieveData(Action refresh) {
        _onRefresh = refresh;

        List<InformationData> result = new List<InformationData>();

        result.Add(new InformationData {
            Prefix = "Number",
            Content = Number.ToString(),
        });

        result.Add(new InformationData {
            Prefix = "Description",
            OnMoreInfo = () => MessagePopup.ShowMessage(Description, "Description", false),
        });

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

        result.Add(new InformationData {
            Prefix = "Notes",
            Content = Note,
        });

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

        foreach (var npc in NPCs) {
            availableNPCs.Remove(availableNPCs.Find(x => x.Name == npc.Name));
        }

        return availableNPCs;
    }
}