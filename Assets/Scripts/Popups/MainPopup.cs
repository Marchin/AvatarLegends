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
    [SerializeField] private GameObject _noCharacterMsg = default;
    [SerializeField] private Color _tabSelectedColor = default;
    [SerializeField] private Color _tabUnselectedColor = default;
    private GameData _gameData;
    private Dictionary<string, IDataEntry> Entries;// => _gameData.NPCs;
    private string _selected;
    private Action _record;

    private void Awake() {
        _addCharacter.onClick.AddListener(AddCharacter);
        _editCharacter.onClick.AddListener(EditCharacter);
        _deleteCharacter.onClick.AddListener(DeleteCharacter);

        List<ButtonData> tabs = new List<ButtonData>();

        const string enemiesTabText = "Enemies";
        tabs.Add(new ButtonData {
            Text = enemiesTabText,
            Callback = () => {
                _record?.Invoke();
                Entries = new Dictionary<string, IDataEntry>(_gameData.User.Enemies.Count);
                foreach (var npc in _gameData.User.Enemies) {
                    Entries.Add(npc.Key, npc.Value);
                }
                foreach (var tab in _tabsList.Elements) {
                    tab.ButtonImage.color = (tab.Text == enemiesTabText) ? _tabSelectedColor : _tabUnselectedColor;
                }
                _record = null;
                _selected = null;
                Refresh();
                _record = () => {
                    _gameData.User.Enemies = new Dictionary<string, NPC>(Entries.Count);
                    foreach (var entry in Entries) {
                        _gameData.User.Enemies.Add(entry.Key, entry.Value as NPC);
                    }
                };
            }
        });

        const string npcsTabText = "NPCs";
        tabs.Add(new ButtonData {
            Text = npcsTabText,
            Callback = () => {
                _record?.Invoke();
                Entries = new Dictionary<string, IDataEntry>(_gameData.User.NPCs.Count);
                foreach (var npc in _gameData.User.NPCs) {
                    Entries.Add(npc.Key, npc.Value);
                }
                foreach (var tab in _tabsList.Elements) {
                    tab.ButtonImage.color = (tab.Text == npcsTabText) ? _tabSelectedColor : _tabUnselectedColor;
                }
                _record = null;
                _selected = null;
                Refresh();
                _record = () => {
                    _gameData.User.NPCs = new Dictionary<string, NPC>(Entries.Count);
                    foreach (var entry in Entries) {
                        _gameData.User.NPCs.Add(entry.Key, entry.Value as NPC);
                    }
                };
            }
        });

        _tabsList.Populate(tabs);
    }

    private void Refresh() {
        var names = new List<string>(Entries.Keys);
        names.Sort();
        List<ButtonData> buttons = new List<ButtonData>(Entries.Count);
        foreach (var name in names) {
            _selected = _selected ?? name;
            buttons.Add(new ButtonData {
                Text = name,
                Callback = () => SetCharacter(Entries[name])
            });
        }
        _nameList.Populate(buttons);

        if (Entries.Count > 0) {
            _noCharacterMsg.SetActive(false);
            _infoContainer.SetActive(true);
            SetCharacter(Entries[_selected]);
        } else {
            _noCharacterMsg.SetActive(true);
            _infoContainer.SetActive(false);
        }

        _record?.Invoke();
    }

    private void SetCharacter(IDataEntry entry) {
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

    private async void AddCharacter() {
        var addCharacterPopup = await PopupManager.Instance.GetOrLoadPopup<AddCharacterPopup>(restore: false);
        addCharacterPopup.Populate(OnCharacterCreation, Entries.Keys);
    }

    private async void EditCharacter() {
        var addCharacterPopup = await PopupManager.Instance.GetOrLoadPopup<AddCharacterPopup>(restore: false);
        addCharacterPopup.Populate(OnCharacterEdition, Entries.Keys, Entries[_selected]);
    }

    private async void DeleteCharacter() {
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

    private void OnCharacterCreation(IDataEntry entry) {
        Entries.Add(entry.Name, (IDataEntry)entry);
        _selected = entry.Name;
        Refresh();
    }

    private void OnCharacterEdition(IDataEntry entry) {
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
                SetCharacter(Entries[popupData.Selected]);
            }
        }
    }
}