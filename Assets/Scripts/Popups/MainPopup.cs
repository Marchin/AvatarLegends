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

    [SerializeField] private Button _addCharacter = default;
    [SerializeField] private Button _editCharacter = default;
    [SerializeField] private Button _deleteCharacter = default;
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

    private void Awake() {
        _addCharacter.onClick.AddListener(() => _onAddEntry());
        _editCharacter.onClick.AddListener(() => _onEditEntry());
        _deleteCharacter.onClick.AddListener(DeleteEntry);

        List<ButtonData> tabs = new List<ButtonData>();

        const string enemiesTabText = "Enemies";
        tabs.Add(new ButtonData {
            Text = enemiesTabText,
            Callback = () => {
                SetEntryCollection<NPC>(
                    Data.Enemies,
                    val => Data.Enemies = val,
                    enemiesTabText,
                    onAddEntry: AddNPC,
                    onEditEntry: EditNPC,
                    isEditable: _ => true
                );
            }
        });

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
                        var addTechniquePopup = await PopupManager.Instance.GetOrLoadPopup<AddStatusPopup>(restore: false);
                        addTechniquePopup.Populate(OnEntryCreation, Entries.Keys);
                    },
                    onEditEntry: async () => {
                        var addTechniquePopup = await PopupManager.Instance.GetOrLoadPopup<AddStatusPopup>(restore: false);
                        addTechniquePopup.Populate(OnEntryEdition, Entries.Keys, Entries[_selected] as Status);
                    },
                    isEditable: entry => Data.IsEditable(entry as Status)
                );
            }
        });

        _tabsList.Populate(tabs);
        
        _tabsList[0].Invoke();
        Refresh();
        
        async void AddNPC() {
            var addCharacterPopup = await PopupManager.Instance.GetOrLoadPopup<AddNPCPopup>(restore: false);
            addCharacterPopup.Populate(OnEntryCreation, Entries.Keys);
        }

        async void EditNPC() {
            var addCharacterPopup = await PopupManager.Instance.GetOrLoadPopup<AddNPCPopup>(restore: false);
            addCharacterPopup.Populate(OnEntryEdition, Entries.Keys, Entries[_selected] as NPC);
        }
    }

    private void SetEntryCollection<T>(Dictionary<string, T> entries, 
        Action<Dictionary<string, T>> onSave,
        string tabName,
        Action onAddEntry,
        Action onEditEntry,
        Func<IDataEntry, bool> isEditable
    ) where T : IDataEntry {
        _record?.Invoke();
        Entries = new Dictionary<string, IDataEntry>(entries.Count);
        foreach (var enemy in entries) {
            Entries.Add(enemy.Key, enemy.Value);
        }
        foreach (var tab in _tabsList.Elements) {
            tab.ButtonImage.color = (tab.Text == tabName) ? _tabSelectedColor : _tabUnselectedColor;
        }
        _record = null;
        _selected = null;
        _onAddEntry = onAddEntry;
        _onEditEntry = onEditEntry;
        _isEditable = isEditable;
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
            SetEntry(Entries[_selected]);
        } else {
            _noEntryMsg.SetActive(true);
            _infoContainer.SetActive(false);
        }

        _record?.Invoke();
    }

    private void SetEntry(IDataEntry entry) {
        _name.text = entry.Name;
        Entries[entry.Name] = entry;
        _infoList.Populate(Entries[entry.Name].RetrieveData(Refresh));
        _selected = entry.Name;
        _editCharacter.interactable = _isEditable(entry);
        _deleteCharacter.interactable = _isEditable(entry);
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