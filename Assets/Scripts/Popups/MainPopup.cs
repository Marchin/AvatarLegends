using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainPopup : Popup {
    public class PopupData {
        public GameData Data;
        public string SelectedNPC;
    }

    public enum Tab {
        NPCs
    }

    [SerializeField] private Button _addCharacter = default;
    [SerializeField] private Button _editCharacter = default;
    [SerializeField] private Button _deleteCharacter = default;
    [SerializeField] private ButtonList _nameList = default;
    [SerializeField] private TextMeshProUGUI _name = default;
    [SerializeField] private InformationList _infoList = default;
    [SerializeField] private GameObject _infoContainer = default;
    [SerializeField] private GameObject _noCharacterMsg = default;
    private GameData _gameData;
    private Dictionary<string, NPC> NPCs => _gameData.NPCs;
    private string _selectedNPC;

    private void Awake() {
        _addCharacter.onClick.AddListener(AddCharacter);
        _editCharacter.onClick.AddListener(EditCharacter);
        _deleteCharacter.onClick.AddListener(DeleteCharacter);
    }

    private void Refresh() {
        var names = new List<string>(NPCs.Keys);
        names.Sort();
        List<ButtonData> buttons = new List<ButtonData>(NPCs.Count);
        foreach (var name in names) {
            _selectedNPC = _selectedNPC ?? name;
            buttons.Add(new ButtonData {
                Text = name,
                Callback = () => SetCharacter(NPCs[name])
            });
        }
        _nameList.Populate(buttons);

        if (_gameData.NPCs.Count > 0) {
            _noCharacterMsg.SetActive(false);
            _infoContainer.SetActive(true);
            SetCharacter(NPCs[_selectedNPC]);
        } else {
            _noCharacterMsg.SetActive(true);
            _infoContainer.SetActive(false);
        }

    }

    private void SetCharacter(NPC npc) {
        _name.text = npc.Name;
        NPCs[npc.Name] = npc;
        _infoList.Populate(NPCs[npc.Name].RetrieveData(Refresh));
        _selectedNPC = npc.Name;
    }

    public void Populate(GameData gameData) {
        _gameData = gameData;
        Refresh();
    }

    private async void AddCharacter() {
        var addCharacterPopup = await PopupManager.Instance.GetOrLoadPopup<AddCharacterPopup>(restore: false);
        addCharacterPopup.Populate(OnCharacterCreation, NPCs.Keys);
    }

    private async void EditCharacter() {
        var addCharacterPopup = await PopupManager.Instance.GetOrLoadPopup<AddCharacterPopup>(restore: false);
        addCharacterPopup.Populate(OnCharacterEdition, NPCs.Keys, _gameData.NPCs[_selectedNPC]);
    }

    private async void DeleteCharacter() {
        List<ButtonData> buttons = new List<ButtonData>();

        buttons.Add(new ButtonData {
            Text = "Yes",
            Callback = () => {
                var names = new List<string>(NPCs.Keys);
                int nextNameIndex = names.IndexOf(_selectedNPC);
                NPCs.Remove(_selectedNPC);
                names.Remove(_selectedNPC);
                nextNameIndex = Mathf.Min(nextNameIndex, names.Count - 1);
                _selectedNPC = names[nextNameIndex];
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
        msgPopup.Populate($"Do you want to delete {_selectedNPC}?", "Delete", buttonsList: buttons);
    }

    private void OnCharacterCreation(NPC npc) {
        _gameData.NPCs.Add(npc.Name, npc);
        _selectedNPC = npc.Name;
        Refresh();
    }

    private void OnCharacterEdition(NPC npc) {
        if (_selectedNPC != npc.Name) {
            NPCs.Remove(_selectedNPC);
            _selectedNPC = npc.Name;
        }
        NPCs[npc.Name] = npc;
        Refresh();
    }

    public override object GetRestorationData() {
        PopupData data = new PopupData {
            Data = _gameData,
            SelectedNPC = _selectedNPC
        };

        return data;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            Populate(popupData.Data);
            if (popupData.SelectedNPC != null) {
                SetCharacter(NPCs[popupData.SelectedNPC]);
            }
        }
    }
}