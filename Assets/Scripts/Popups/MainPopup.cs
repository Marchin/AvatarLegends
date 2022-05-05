using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainPopup : Popup {
    public class PopupData {
        public GameData Data;
        public string Selected;
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
    private GameData _gameData;
    private Dictionary<string, IDataEntry> Entries;// => _gameData.NPCs;
    private string _selected;
    private Action _record;
    private Action _onAddEntry;
    private Action _onEditEntry;

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
                    _gameData.User.Enemies,
                    val => _gameData.User.Enemies = val,
                    enemiesTabText,
                    onAddEntry: AddNPC,
                    onEditEntry: EditNPC
                );
            }
        });

        const string npcsTabText = "NPCs";
        tabs.Add(new ButtonData {
            Text = npcsTabText,
            Callback = () => {
                SetEntryCollection<NPC>(
                    _gameData.User.NPCs,
                    val => _gameData.User.NPCs = val,
                    npcsTabText,
                    onAddEntry: AddNPC,
                    onEditEntry: EditNPC
                );
            }
        });

        const string techniquesTabText = "Techniques";
        tabs.Add(new ButtonData {
            Text = techniquesTabText,
            Callback = () => {
                SetEntryCollection<Technique>(
                    _gameData.User.CustomTechniques,
                    val => _gameData.User.CustomTechniques = val,
                    techniquesTabText,
                    onAddEntry: async () => {
                        var addTechniquePopup = await PopupManager.Instance.GetOrLoadPopup<AddTechniquePopup>(restore: false);
                        addTechniquePopup.Populate(OnEntryCreation, Entries.Keys);
                    },
                    onEditEntry: async () => {
                        var addTechniquePopup = await PopupManager.Instance.GetOrLoadPopup<AddTechniquePopup>(restore: false);
                        addTechniquePopup.Populate(OnEntryEdition, Entries.Keys, Entries[_selected] as Technique);
                    }
                );
            }
        });

        _tabsList.Populate(tabs);
        
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
        Action onEditEntry
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
    }

    public void Populate(GameData gameData) {
        _gameData = gameData;
        _tabsList[0].Invoke();
        Refresh();
    }

    private async void DeleteEntry() {
        List<ButtonData> buttons = new List<ButtonData>();

        buttons.Add(new ButtonData {
            Text = "Yes",
            Callback = () => {
                var names = new List<string>(Entries.Keys);
                int nextNameIndex = names.IndexOf(_selected);
                Entries.Remove(_selected);
                names.Remove(_selected);
                nextNameIndex = Mathf.Min(nextNameIndex, names.Count - 1);
                _selected = names[nextNameIndex];
                Refresh();

                _ = PopupManager.Instance.Back();
            }
        });

        buttons.Add(new ButtonData {
            Text = "No",
            Callback = () => {
                _ = PopupManager.Instance.Back();
            }
        });

        var msgPopup = await PopupManager.Instance.GetOrLoadPopup<MessagePopup>(restore: false);
        msgPopup.Populate($"Do you want to delete {_selected}?", "Delete", buttonsList: buttons);
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
            Data = _gameData,
            Selected = _selected
        };

        return data;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            Populate(popupData.Data);
            if (popupData.Selected != null) {
                SetEntry(Entries[popupData.Selected]);
            }
        }
    }
}