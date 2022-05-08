using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class Engagement : IDataEntry {
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("enemies")]
    public List<NPC> Enemies = new List<NPC>();

    [JsonProperty("npcs")]
    public List<NPC> NPCs = new List<NPC>();

    [JsonProperty("note")]
    public string Note;

    private Action _onRefresh;
    private AppData Data => ApplicationManager.Instance.Data;
    private bool _showNPCs;
    private bool _showEnemies;

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
            Content = "NPCs",
            OnDropdown = (NPCs.Count > 0) ? onNPCDropdown : null,
            OnAdd = (NPCs.Count < Data.NPCs.Count) ?
                AddNPC :
                (Action)null,
            Expanded = _showNPCs
        });

        if (_showNPCs) {
            foreach (var npc in NPCs) {
                result.Add(new InformationData {
                    Content = npc.Name,
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

        Action onEnemyDropdown = () => {
            _showEnemies = !_showEnemies;
            _onRefresh();
        };

        result.Add(new InformationData {
            Content = "Enemies",
            OnDropdown = (Enemies.Count > 0) ? onEnemyDropdown : null,
            OnAdd = (Enemies.Count < Data.Enemies.Count) ?
                AddEnemy :
                (Action)null,
            Expanded = _showEnemies
        });

        if (_showEnemies) {
            foreach (var enemy in Enemies) {
                result.Add(new InformationData {
                    Content = enemy.Name,
                    OnDelete = async () => {
                        await MessagePopup.ShowConfirmationPopup(
                            $"Remove {enemy.Name} from the engagement?",
                            onYes: () => Enemies.Remove(enemy)
                        );
                        _onRefresh();
                    },
                    OnMoreInfo = async () => {
                        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
                        Refresh();

                        void Refresh() {
                            listPopup.Populate(enemy.RetrieveData(Refresh), enemy.Name, null);
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
        List<NPC> availableNPCs = new List<NPC>(Data.NPCs.Values);

        foreach (var npc in NPCs) {
            availableNPCs.Remove(availableNPCs.Find(x => x.Name == npc.Name));
        }

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
    private async void AddEnemy() {
        List<NPC> enemiesToAdd = new List<NPC>(Enemies);
        List<InformationData> infoList = new List<InformationData>(Data.Enemies.Count);
        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>(restore: false);
        var availableEnemies = new List<NPC>(Data.Enemies.Values);

        foreach (var enemy in Enemies) {
            availableEnemies.Remove(availableEnemies.Find(x => x.Name == enemy.Name));
        }

        Refresh();

        void Refresh() {
            infoList.Clear();
            foreach (var enemy in availableEnemies) {
                infoList.Add(new InformationData {
                    Content = enemy.Name,
                    IsToggleOn = enemiesToAdd.Contains(enemy),
                    OnToggle = isOn => {
                        if (isOn) {
                            enemiesToAdd.Add(enemy);
                        } else {
                            enemiesToAdd.Remove(enemy);
                        }

                        Refresh();
                    },
                    OnMoreInfo = string.IsNullOrEmpty(enemy.Description) ?
                        (Action)null :
                        enemy.ShowDescription
                });
            }

            listPopup.Populate(infoList,
                $"Add Enemies",
                () => {
                    Enemies = enemiesToAdd;
                    _onRefresh();
                }
            );
        }
    }
}