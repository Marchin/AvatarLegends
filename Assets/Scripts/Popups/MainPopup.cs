using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainPopup : Popup {
    public class PopupData {
        public string Selected;
        public int TabIndex;
    }

    [SerializeField] private Button _addEntry = default;
    [SerializeField] private Button _editEntry = default;
    [SerializeField] private Button _deleteEntry = default;
    [SerializeField] private Button _deleteAll = default;
    [SerializeField] private ButtonList _nameList = default;
    [SerializeField] private ButtonList _tabsList = default;
    [SerializeField] private TextMeshProUGUI _name = default;
    [SerializeField] private InformationList _infoList = default;
    [SerializeField] private GameObject _infoContainer = default;
    [SerializeField] private GameObject _noEntryMsg = default;
    [SerializeField] private Color _tabSelectedColor = default;
    [SerializeField] private Color _tabUnselectedColor = default;
    private AppData Data => ApplicationManager.Instance.Data;
    private Dictionary<string, IDataEntry> Entries;// => _gameData.NPCs;
    private string _selected;
    private Action _record;
    private Action _onAddEntry;
    private Action _onEditEntry;
    private Func<IDataEntry, bool> _isEditable;
    private Action _onDeleteAll;

    private void Awake() {
        _addEntry.onClick.AddListener(() => _onAddEntry());
        _editEntry.onClick.AddListener(() => _onEditEntry());
        _deleteAll.onClick.AddListener(() => _onDeleteAll());
        _deleteEntry.onClick.AddListener(DeleteEntry);

        List<ButtonData> tabs = new List<ButtonData>();

        const string npcsTabText = "NPCs";
        tabs.Add(new ButtonData {
            Text = npcsTabText,
            Callback = () => {
                SetEntryCollection<NPC>(
                    Data.NPCs,
                    val => Data.NPCs = val,
                    npcsTabText,
                    onAddEntry: AddNPC,
                    onEditEntry: EditNPC,
                    isEditable: entry => Data.IsEditable(entry as NPC)
                );
            }
        });

        const string techniquesTabText = "Techniques";
        tabs.Add(new ButtonData {
            Text = techniquesTabText,
            Callback = () => {
                SetEntryCollection<Technique>(
                    Data.Techniques,
                    val => Data.Techniques = val,
                    techniquesTabText,
                    onAddEntry: async () => {
                        var addTechniquePopup = await PopupManager.Instance.GetOrLoadPopup<AddTechniquePopup>(restore: false);
                        addTechniquePopup.Populate(OnEntryCreation, Entries.Keys);
                    },
                    onEditEntry: async () => {
                        var addTechniquePopup = await PopupManager.Instance.GetOrLoadPopup<AddTechniquePopup>(restore: false);
                        addTechniquePopup.Populate(OnEntryEdition, Entries.Keys, Entries[_selected] as Technique);
                    },
                    isEditable: entry => Data.IsEditable(entry as Technique)
                );
            }
        });

        const string statusesTabText = "Statuses";
        tabs.Add(new ButtonData {
            Text = statusesTabText,
            Callback = () => {
                SetEntryCollection<Status>(
                    Data.Statuses,
                    val => Data.Statuses = val,
                    statusesTabText,
                    onAddEntry: async () => {
                        var addStatusPopup = await PopupManager.Instance.GetOrLoadPopup<AddStatusPopup>(restore: false);
                        addStatusPopup.Populate(OnEntryCreation, Entries.Keys);
                    },
                    onEditEntry: async () => {
                        var addStatusPopup = await PopupManager.Instance.GetOrLoadPopup<AddStatusPopup>(restore: false);
                        addStatusPopup.Populate(OnEntryEdition, Entries.Keys, Entries[_selected] as Status);
                    },
                    isEditable: entry => Data.IsEditable(entry as Status)
                );
            }
        });

        const string engagementsTabText = "Engagements";
        tabs.Add(new ButtonData {
            Text = engagementsTabText,
            Callback = () => {
                SetEntryCollection<Engagement>(
                    Data.User.Engagements,
                    val => Data.User.Engagements = val,
                    engagementsTabText,
                    onAddEntry: async () => {
                        var addEngagementPopup = await PopupManager.Instance.GetOrLoadPopup<AddEngagementPopup>(restore: false);
                        addEngagementPopup.Populate(OnEntryCreation, Entries.Keys);
                    },
                    onEditEntry: async () => {
                        var addEngagementPopup = await PopupManager.Instance.GetOrLoadPopup<AddEngagementPopup>(restore: false);
                        addEngagementPopup.Populate(OnEntryEdition, Entries.Keys, Entries[_selected] as Engagement);
                    },
                    isEditable: _ => true,
                    onDeleteAll: async () => {
                        bool confirmed = await MessagePopup.ShowConfirmationPopup(
                            "Delete all engagements?",
                            onYes: () => {
                                Entries.Clear();
                                _selected = null;
                                Refresh();
                            },
                            restore: false
                        );
                    }
                );
            }
        });

        _tabsList.Populate(tabs);
        
        _tabsList[0].Invoke();
        Refresh();
        
        async void AddNPC() {
            var addNPCPopup = await PopupManager.Instance.GetOrLoadPopup<AddNPCPopup>(restore: false);
            addNPCPopup.Populate(OnEntryCreation, Entries.Keys);
        }

        async void EditNPC() {
            var addNPCPopup = await PopupManager.Instance.GetOrLoadPopup<AddNPCPopup>(restore: false);
            addNPCPopup.Populate(OnEntryEdition, Entries.Keys, Entries[_selected] as NPC);
        }
    }

    private void SetEntryCollection<T>(Dictionary<string, T> entries, 
        Action<Dictionary<string, T>> onSave,
        string tabName,
        Action onAddEntry,
        Action onEditEntry,
        Func<IDataEntry, bool> isEditable,
        Action onDeleteAll = null
    ) where T : IDataEntry {
        _record?.Invoke();
        Entries = new Dictionary<string, IDataEntry>(entries.Count);
        foreach (var entry in entries) {
            Entries.Add(entry.Key, entry.Value);
        }
        foreach (var tab in _tabsList.Elements) {
            tab.ButtonImage.color = (tab.Text == tabName) ? _tabSelectedColor : _tabUnselectedColor;
        }
        _record = null;
        _selected = null;
        _onAddEntry = onAddEntry;
        _onEditEntry = onEditEntry;
        _isEditable = isEditable;
        _onDeleteAll = onDeleteAll;
        _deleteAll.gameObject.SetActive(onDeleteAll != null);
        Refresh();
        _record = () => {
            entries = new Dictionary<string, T>(Entries.Count);
            foreach (var entry in Entries) {
                entries.Add(entry.Key, (T)entry.Value);
            }
            onSave(entries);
        };
    }

    private void Refresh() {
        var names = new List<string>(Entries.Keys);
        names.Sort();
        List<ButtonData> buttons = new List<ButtonData>(Entries.Count);
        foreach (var name in names) {
            _selected = _selected ?? name;
            buttons.Add(new ButtonData {
                Text = name,
                Callback = () => SetEntry(Entries[name])
            });
        }
        _nameList.Populate(buttons);

        if (Entries.Count > 0) {
            _noEntryMsg.SetActive(false);
            _infoContainer.SetActive(true);
            _deleteAll.interactable = true;
            SetEntry(Entries[_selected]);
        } else {
            _noEntryMsg.SetActive(true);
            _infoContainer.SetActive(false);
            _deleteAll.interactable = false;
        }

        _record?.Invoke();
    }

    private void SetEntry(IDataEntry entry) {
        _name.text = entry.Name;
        Entries[entry.Name] = entry;
        _infoList.Populate(Entries[entry.Name].RetrieveData(Refresh));
        _selected = entry.Name;
        _editEntry.interactable = _isEditable(entry);
        _deleteEntry.interactable = _isEditable(entry);
    }

    private void DeleteEntry() {
        _ = MessagePopup.ShowConfirmationPopup(
            $"Do you want to delete {_selected}?",
            onYes: () => {
                var names = new List<string>(Entries.Keys);
                int nextNameIndex = names.IndexOf(_selected);
                Entries.Remove(_selected);
                names.Remove(_selected);
                nextNameIndex = Mathf.Min(nextNameIndex, names.Count - 1);
                _selected = names[nextNameIndex];
                Refresh();

                _ = PopupManager.Instance.Back();
            }
        );
    }

    private void OnEntryCreation(IDataEntry entry) {
        Entries.Add(entry.Name, (IDataEntry)entry);
        _selected = entry.Name;
        Refresh();
    }

    private void OnEntryEdition(IDataEntry entry) {
        if (_selected != entry.Name) {
            Entries.Remove(_selected);
            _selected = entry.Name;
        }
        Entries[entry.Name] = entry;
        Refresh();
    }

    public override object GetRestorationData() {
        PopupData data = new PopupData {
            Selected = _selected
        };

        return data;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            if (popupData.Selected != null) {
                SetEntry(Entries[popupData.Selected]);
            }
        }
    }
}