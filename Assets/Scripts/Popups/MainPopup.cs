using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainPopup : Popup {
    public class PopupData {
        public GameData Data;
        public NPC SelectedNPC;
    }

    public enum Tab {
        NPCs
    }

    [SerializeField] private Button _addCharacter = default;
    [SerializeField] private Button _editCharacter = default;
    [SerializeField] private ButtonList _nameList = default;
    [SerializeField] private TextMeshProUGUI _name = default;
    [SerializeField] private InformationList _infoList = default;
    [SerializeField] private GameObject _infoContainer = default;
    [SerializeField] private GameObject _noCharacterMsg = default;
    private GameData _gameData;
    private NPC _selectedNPC;

    private void Awake() {
        _addCharacter.onClick.AddListener(AddCharacter);
        _editCharacter.onClick.AddListener(EditCharacter);
    }

    private void Refresh() {
        List<ButtonData> buttons = new List<ButtonData>(_gameData.NPCs.Count);
        foreach (var npc in _gameData.NPCs) {
            buttons.Add(new ButtonData {
                Text = npc.Name,
                Callback = () => SetCharacter(npc)
            });
        }
        _nameList.Populate(buttons);

        if (_gameData.NPCs.Count > 0) {
            _noCharacterMsg.SetActive(false);
            _infoContainer.SetActive(true);
            SetCharacter(_selectedNPC ?? _gameData.NPCs[0]);
        } else {
            _noCharacterMsg.SetActive(true);
            _infoContainer.SetActive(false);
        }

    }

    private void SetCharacter(NPC npc) {
        _name.text = npc.Name;
        _infoList.Populate(npc.RetrieveData());
        _selectedNPC = npc;
    }

    public void Populate(GameData gameData) {
        _gameData = gameData;
        Refresh();
    }

    private async void AddCharacter() {
        var addCharacterPopup = await PopupManager.Instance.GetOrLoadPopup<AddCharacterPopup>(restore: false);
        addCharacterPopup.Populate(OnCharacterCreation);
    }

    private async void EditCharacter() {
        var addCharacterPopup = await PopupManager.Instance.GetOrLoadPopup<AddCharacterPopup>(restore: false);
        addCharacterPopup.Populate(OnCharacterEdition, _selectedNPC);
    }

    private void SortCharacters() {
        _gameData.NPCs.Sort((x, y) => x.Name.CompareTo(y.Name));
    }

    private void OnCharacterCreation(NPC npc) {
        _gameData.NPCs.Add(npc);
        _selectedNPC = npc;
        SortCharacters();
        Refresh();
    }

    private void OnCharacterEdition(NPC npc) {
        _gameData.NPCs.Remove(_selectedNPC);
        _gameData.NPCs.Add(npc);
        _selectedNPC = npc;
        SortCharacters();
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
        Debug.LogError("restore");
        if (data is PopupData popupData) {
            Populate(popupData.Data);
            if (popupData.SelectedNPC != null) {
                SetCharacter(popupData.SelectedNPC);
            }
        }
    }
}