using TMPro;
using System;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CampaignViewPopup : Popup {
    public class PopupData {
        public string Selected;
        public int TabIndex;
    }

    [SerializeField] private Button _addEntry = default;
    [SerializeField] private Button _editEntry = default;
    [SerializeField] private Button _deleteEntry = default;
    [SerializeField] private Button _deleteAll = default;
    [SerializeField] private Button _closeButton = default;
    [SerializeField] private Button _filterButton = default;
    [SerializeField] private Button _infoButton = default;
    [SerializeField] private Button _clearSearchButton = default;
    [SerializeField] private ButtonList _nameList = default;
    [SerializeField] private ButtonList _tabsList = default;
    [SerializeField] private ButtonList _buttonsList = default;
    [SerializeField] private TextMeshProUGUI _name = default;
    [SerializeField] private InformationList _infoList = default;
    [SerializeField] private TMP_InputField _searchInput = default;
    [SerializeField] private GameObject _searchIcon = default;
    [SerializeField] private GameObject _infoContainer = default;
    [SerializeField] private GameObject _noEntryMsg = default;
    [SerializeField] private GameObject _activeFilterIndicator = default;
    [SerializeField] private Color _selectedColor = default;
    [SerializeField] private Color _unselectedColor = default;
    [SerializeField] private Color _highlightedColor = default;
    private AppData Data => ApplicationManager.Instance.Data;
    private Campaign SelectedCampaign => Data.User.SelectedCampaign;
    private Session CurrentSession => Data.User.CurrentSession;
    private Dictionary<string, IDataEntry> _entries;
    private List<IDataEntry> _filteredEntries;
    private List<IDataEntry> _searchedEntries;
    private string _highlightedEntry;
    private string _selectedEntry;
    private string _selectedTab;
    private Action _reload;
    private Action _record;
    private Action _onAddEntry;
    private Action _onEditEntry;
    private Action<IDataEntry> _onSetEntry;
    private Func<IDataEntry, bool> _isEditable;
    private Action _onDeleteAll;
    private Action<List<IDataEntry>> _customSort;
    private Func<List<ButtonData>> _getButtons;
    private GameObject _engagementTab;
    private Dictionary<string, string> _tabSelectedEntry = new Dictionary<string, string>();
    private Dictionary<string, Filter> _tabFilter = new Dictionary<string, Filter>();
    
    private void Awake() {
        _addEntry.onClick.AddListener(() => _onAddEntry());
        _editEntry.onClick.AddListener(() => _onEditEntry());
        _deleteAll.onClick.AddListener(() => _onDeleteAll());
        _deleteEntry.onClick.AddListener(DeleteEntry);
        _closeButton.onClick.AddListener(PopupManager.Instance.Back);
        _filterButton.onClick.AddListener(OnFilterPress);
        _clearSearchButton.onClick.AddListener(() => _searchInput.text = "");
        _infoButton.onClick.AddListener(async () => {
            var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
            RefreshPopup();

            void RefreshPopup() {
                listPopup.Populate(
                    SelectedCampaign.RetrieveData(RefreshPopup, RefreshPopup), 
                    SelectedCampaign.Name,
                    Refresh
                );
            }
        });

        _clearSearchButton.gameObject.SetActive(false);
        _searchIcon.SetActive(true);
        _searchInput.onValueChanged.AddListener(OnSearchInputChanged);

        List<ButtonData> tabs = new List<ButtonData>();

        const string sessionsTabText = "Sessions";
        tabs.Add(new ButtonData {
            Text = sessionsTabText,
            Callback = () => {
                SetEntryCollection<Session>(
                    () => Data.User.SelectedCampaign.Sessions,
                    val => Data.User.SelectedCampaign.Sessions = val,
                    sessionsTabText,
                    onSetEntry: entry => {
                        Data.User.SelectedCampaign.CurrentSession = entry as Session;
                    },
                    onAddEntry: async () => {
                        var addSessionPopup = await PopupManager.Instance.GetOrLoadPopup<AddSessionPopup>(restore: false);
                        addSessionPopup.Populate(
                            entry => {
                                Data.User.SelectedCampaign.Sessions.Add(entry.Name, entry as Session);
                                OnEntryCreation(entry);
                            },
                            _entries.Keys,
                            null);
                    },
                    onEditEntry: async () => {
                        var addSessionPopup = await PopupManager.Instance.GetOrLoadPopup<AddSessionPopup>(restore: false);
                        addSessionPopup.Populate(OnEntryEdition, _entries.Keys, _entries[_selectedEntry] as Session);
                    },
                    isEditable: _ => true,
                    customSort: entries => {
                        entries.Sort((x, y) => (y as Session).Number.CompareTo(
                            (x as Session).Number
                        ));
                    }
                );
            }
        });

        const string npcsTabText = "NPCs";
        tabs.Add(new ButtonData {
            Text = npcsTabText,
            Callback = () => {
                SetEntryCollection<NPC>(
                    () => Data.NPCs,
                    val => Data.NPCs = val,
                    npcsTabText,
                    onSetEntry: null,
                    onAddEntry: async () => {
                        List<string> names = new List<string>(Data.NPCs.Keys);
                        names.AddRange(SelectedCampaign.PCs.Keys);
                        var addNPCPopup = await PopupManager.Instance.GetOrLoadPopup<AddNPCPopup>(restore: false);
                        addNPCPopup.Populate(OnEntryCreation, names, null);
                    },
                    onEditEntry: async () => {
                        List<string> names = new List<string>(Data.NPCs.Keys);
                        names.AddRange(SelectedCampaign.PCs.Keys);
                        var addNPCPopup = await PopupManager.Instance.GetOrLoadPopup<AddNPCPopup>(restore: false);
                        addNPCPopup.Populate(OnEntryEdition, names, _entries[_selectedEntry] as NPC);
                    },
                    isEditable: entry => Data.IsEditable(entry as NPC)
                );
            }
        });

        const string pcsTabText = "PCs";
        tabs.Add(new ButtonData {
            Text = pcsTabText,
            Callback = () => {
                SetEntryCollection<PC>(
                    () => SelectedCampaign.PCs,
                    val => SelectedCampaign.PCs = val,
                    pcsTabText,
                    onSetEntry: null,
                    onAddEntry: async () => {
                        List<string> names = new List<string>(Data.NPCs.Keys);
                        names.AddRange(SelectedCampaign.PCs.Keys);
                        var addPCPopup = await PopupManager.Instance.GetOrLoadPopup<AddPCPopup>(restore: false);
                        addPCPopup.Populate(OnEntryCreation, names, null);
                    },
                    onEditEntry: async () => {
                        List<string> names = new List<string>(Data.NPCs.Keys);
                        names.AddRange(SelectedCampaign.PCs.Keys);
                        var addPCPopup = await PopupManager.Instance.GetOrLoadPopup<AddPCPopup>(restore: false);
                        addPCPopup.Populate(OnEntryEdition, names, _entries[_selectedEntry] as PC);
                    },
                    isEditable: entry => true
                );
            }
        });

        const string engagementsTabText = "Engagements";
        tabs.Add(new ButtonData {
            Text = engagementsTabText,
            Callback = () => {
                _highlightedEntry = CurrentSession.CurrentEngagement?.Name;
                SetEntryCollection<Engagement>(
                    () => Data.User.CurrentSession.EngagementsByName,
                    val => Data.User.CurrentSession.EngagementsByName = val,
                    engagementsTabText,
                    onSetEntry: null,
                    onAddEntry: async () => {
                        var addEngagementPopup = await PopupManager.Instance.GetOrLoadPopup<AddEngagementPopup>(restore: false);
                        addEngagementPopup.Populate(OnEntryCreation, _entries.Keys, null);
                    },
                    onEditEntry: async () => {
                        var addEngagementPopup = await PopupManager.Instance.GetOrLoadPopup<AddEngagementPopup>(restore: false);
                        addEngagementPopup.Populate(OnEntryEdition, _entries.Keys, _entries[_selectedEntry] as Engagement);
                    },
                    isEditable: _ => true,
                    onDeleteAll: () => {
                        MessagePopup.ShowConfirmationPopup(
                            "Delete all engagements?",
                            onYes: () => {
                                _entries.Clear();
                                _selectedEntry = null;
                                Refresh(applyFilter: true);
                            },
                            restore: false
                        );
                    },
                    customSort: entries => {
                        entries.Sort((x, y) => 
                            (x as Engagement).Number.CompareTo((y as Engagement).Number)
                        );
                    },
                    getButtons: () => Engagement.GetControllerButtons(() => {
                        _selectedEntry = _highlightedEntry = CurrentSession.CurrentEngagement?.Name;
                        Refresh();
                    })
                );
            }
        });

        const string techniquesTabText = "Techniques";
        tabs.Add(new ButtonData {
            Text = techniquesTabText,
            Callback = () => {
                SetEntryCollection<Technique>(
                    () => Data.Techniques,
                    val => Data.Techniques = val,
                    techniquesTabText,
                    onSetEntry: null,
                    onAddEntry: async () => {
                        var addTechniquePopup = await PopupManager.Instance.GetOrLoadPopup<AddTechniquePopup>(restore: false);
                        addTechniquePopup.Populate(OnEntryCreation, _entries.Keys, null);
                    },
                    onEditEntry: async () => {
                        var addTechniquePopup = await PopupManager.Instance.GetOrLoadPopup<AddTechniquePopup>(restore: false);
                        addTechniquePopup.Populate(OnEntryEdition, _entries.Keys, _entries[_selectedEntry] as Technique);
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
                    () => Data.Statuses,
                    val => Data.Statuses = val,
                    statusesTabText,
                    onSetEntry: null,
                    onAddEntry: async () => {
                        var addStatusPopup = await PopupManager.Instance.GetOrLoadPopup<AddStatusPopup>(restore: false);
                        addStatusPopup.Populate(OnEntryCreation, _entries.Keys, null);
                    },
                    onEditEntry: async () => {
                        var addStatusPopup = await PopupManager.Instance.GetOrLoadPopup<AddStatusPopup>(restore: false);
                        addStatusPopup.Populate(OnEntryEdition, _entries.Keys, _entries[_selectedEntry] as Status);
                    },
                    isEditable: entry => Data.IsEditable(entry as Status)
                );
            }
        });

        const string playbooksTabText = "Playbooks";
        tabs.Add(new ButtonData {
            Text = playbooksTabText,
            Callback = () => {
                SetEntryCollection<Playbook>(
                    () => Data.Playbooks,
                    val => Data.Playbooks = val,
                    playbooksTabText,
                    onSetEntry: null,
                    onAddEntry: async () => {
                        var addPlaybookPopup = await PopupManager.Instance.GetOrLoadPopup<AddPlaybookPopup>(restore: false);
                        addPlaybookPopup.Populate(OnEntryCreation, _entries.Keys, null);
                    },
                    onEditEntry: async () => {
                        var addPlaybookPopup = await PopupManager.Instance.GetOrLoadPopup<AddPlaybookPopup>(restore: false);
                        addPlaybookPopup.Populate(OnEntryEdition, _entries.Keys, _entries[_selectedEntry] as Playbook);
                    },
                    isEditable: entry => Data.IsEditable(entry as Playbook)
                );
            }
        });

        const string conditionsTabText = "Conditions";
        tabs.Add(new ButtonData {
            Text = conditionsTabText,
            Callback = () => {
                SetEntryCollection<Condition>(
                    () => Data.Conditions,
                    val => Data.Conditions = val,
                    conditionsTabText,
                    onSetEntry: null,
                    onAddEntry: async () => {
                        var addConditionPopup = await PopupManager.Instance.GetOrLoadPopup<AddConditionPopup>(restore: false);
                        addConditionPopup.Populate(OnEntryCreation, _entries.Keys, null);
                    },
                    onEditEntry: async () => {
                        var addConditionPopup = await PopupManager.Instance.GetOrLoadPopup<AddConditionPopup>(restore: false);
                        addConditionPopup.Populate(OnEntryEdition, _entries.Keys, _entries[_selectedEntry] as Condition);
                    },
                    isEditable: entry => Data.IsEditable(entry as Condition)
                );
            }
        });

        _tabsList.Populate(tabs);

        foreach (var element in _tabsList.Elements) {
            if ((element as ButtonElement).Text == engagementsTabText) {
                _engagementTab = element.gameObject;
                break;
            }
        }
        
        _tabsList[0].Invoke();
        Refresh(applyFilter: true);
    }

    private void SetEntryCollection<T>(Func<Dictionary<string, T>> entriesFunc, 
        Action<Dictionary<string, T>> onSave,
        string tabName,
        Action<IDataEntry> onSetEntry,
        Action onAddEntry,
        Action onEditEntry,
        Func<IDataEntry, bool> isEditable,
        Action onDeleteAll = null,
        Action<List<IDataEntry>> customSort = null,
        Func<List<ButtonData>> getButtons = null
    ) where T : IDataEntry, new() {
        _record?.Invoke();
        Dictionary<string, T> entries = entriesFunc?.Invoke();
        _entries = new Dictionary<string, IDataEntry>(entries.Count);
        foreach (var entry in entries) {
            _entries.Add(entry.Key, entry.Value);
        }
        foreach (var tab in _tabsList.Elements) {
            tab.ButtonImage.color = (tab.Text == tabName) ? _selectedColor : _unselectedColor;
        }
        if (!_tabSelectedEntry.ContainsKey(tabName)) {
            _tabSelectedEntry.Add(tabName, null);
        }

        _record = null;
        _selectedEntry = _tabSelectedEntry[tabName];
        _selectedTab = tabName;
        _onAddEntry = onAddEntry;
        _onSetEntry = onSetEntry;
        _onEditEntry = onEditEntry;
        _isEditable = isEditable;
        _onDeleteAll = onDeleteAll;
        _customSort = customSort;
        _getButtons = getButtons;
        _deleteAll.gameObject.SetActive(onDeleteAll != null);

        _reload = () => {
            _record = null;
            SetEntryCollection<T>(
                entriesFunc, onSave, tabName, 
                onSetEntry, onAddEntry, onEditEntry, 
                isEditable, onDeleteAll, customSort, 
                getButtons
            );
        };

        if (!_tabFilter.ContainsKey(tabName)) {
            T aux = new T();
            _tabFilter[tabName] = aux.GetFilterData() ?? new Filter();
        }

        Refresh(applyFilter: true);
        _record = () => {
            entries = new Dictionary<string, T>(_entries.Count);
            foreach (var entry in _entries) {
                entries.Add(entry.Key, (T)entry.Value);
            }
            onSave(entries);
        };
    }

    private void RefreshButtons() {
        _buttonsList.Populate(_getButtons?.Invoke());
        _buttonsList.gameObject.SetActive(_buttonsList.Elements.Count > 0);
    }

    private void Refresh() {
        Refresh(applyFilter: false);
    }

    private void Refresh(bool applyFilter) {
        _record?.Invoke();

        if (applyFilter) {
            _filteredEntries = new List<IDataEntry>(_entries.Values);

            _activeFilterIndicator.SetActive(_tabFilter[_selectedTab].Active);
                
            if (_customSort != null) {
                _customSort(_filteredEntries);
            } else {
                _filteredEntries.Sort((x, y) => x.Name.CompareTo(y.Name));
            }
            
            foreach (var toggle in _tabFilter[_selectedTab].Toggles) {
                _filteredEntries = toggle.Apply(_filteredEntries);
            }

            foreach (var filterChannel in _tabFilter[_selectedTab].Filters) {
                _filteredEntries = filterChannel.Apply(_filteredEntries);
            }
        }

        if (string.IsNullOrEmpty(_searchInput.text)) {
            _searchedEntries = _filteredEntries;
        } else {
            _searchedEntries = new List<IDataEntry>(_filteredEntries.Count);
            _searchedEntries.AddRange(
                _filteredEntries.FindAll(x => x.Name.StartsWith(
                    _searchInput.text, true, CultureInfo.InvariantCulture)));
            _searchedEntries.AddRange(_filteredEntries.FindAll(x => 
                (_searchedEntries.Find(y => y.Name == x.Name) == null) &&
                x.Name.Contains(_searchInput.text, StringComparison.InvariantCultureIgnoreCase)));
        }


        if (_searchedEntries.Find(x => x.Name == _selectedEntry) == null) {
            _selectedEntry = null;
        }

        List<ButtonData> buttons = new List<ButtonData>(_entries.Count);
        foreach (var entry in _searchedEntries) {
            _selectedEntry = _selectedEntry ?? entry.Name;
            buttons.Add(new ButtonData {
                Text = entry.Name,
                Callback = () => SetEntry(entry)
            });
        }
        _nameList.Populate(buttons);

        if (_searchedEntries.Count > 0) {
            _noEntryMsg.SetActive(false);
            _infoContainer.SetActive(true);
            _deleteAll.interactable = true;
            SetEntry(_entries[_selectedEntry]);
        } else {
            _noEntryMsg.SetActive(true);
            _infoContainer.SetActive(false);
            _deleteAll.interactable = false;
        }

        _engagementTab.SetActive(Data.User.CurrentSession != null);
        RefreshButtons();
    }

    private void SetEntry(IDataEntry entry) {
        _name.text = entry.Name;
        _entries[entry.Name] = entry;
        _tabSelectedEntry[_selectedTab] = entry.Name;
        _infoList.Populate(_entries[entry.Name].RetrieveData(Refresh, _reload));
        _selectedEntry = entry.Name;
        _editEntry.interactable = _isEditable(entry);
        _deleteEntry.interactable = _isEditable(entry);

        foreach (var element in _nameList.Elements) {
            element.ButtonImage.color = (element.Text == entry.Name) ?
                _selectedColor :
                (element.Text == _highlightedEntry) ?
                    _highlightedColor :
                    _unselectedColor;
        }

        _onSetEntry?.Invoke(entry);
    }

    private void DeleteEntry() {
        MessagePopup.ShowConfirmationPopup(
            $"Do you want to delete {_selectedEntry}?",
            onYes: () => {
                var names = new List<string>(_entries.Keys);
                int nextNameIndex = names.IndexOf(_selectedEntry);
                _entries.Remove(_selectedEntry);
                names.Remove(_selectedEntry);
                nextNameIndex = Mathf.Min(nextNameIndex, names.Count - 1);
                _selectedEntry = (nextNameIndex >= 0) ? names[nextNameIndex] : null;
                Refresh(applyFilter: true);
            }
        );
    }

    private void OnEntryCreation(IDataEntry entry) {
        _entries.Add(entry.Name, (IDataEntry)entry);
        _selectedEntry = entry.Name;
        Refresh(applyFilter: true);
    }

    private void OnEntryEdition(IDataEntry entry) {
        if (_selectedEntry != entry.Name) {
            _entries.Remove(_selectedEntry);
            _selectedEntry = entry.Name;
        }
        _entries[entry.Name] = entry;
        Refresh(applyFilter: true);
    }

    private async void OnFilterPress() {
        var filterPopup = await PopupManager.Instance.GetOrLoadPopup<FilterPopup>(restore: false);
        filterPopup.Populate(_tabFilter[_selectedTab], filter => {
            _tabFilter[_selectedTab] = filter;
            Refresh(applyFilter: true);
        });
    }
    
    private void OnSearchInputChanged(string query) {
        _clearSearchButton.gameObject.SetActive(!string.IsNullOrEmpty(query));
        _searchIcon.SetActive(string.IsNullOrEmpty(query));
        Refresh();
    }
            
    public override object GetRestorationData() {
        PopupData data = new PopupData {
            Selected = _selectedEntry
        };

        return data;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            if ((popupData.Selected != null) && _entries.ContainsKey(popupData.Selected)) {
                SetEntry(_entries[popupData.Selected]);
            }
        }
    }
}