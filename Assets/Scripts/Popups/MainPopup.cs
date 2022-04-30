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
    [SerializeField] private ButtonList _nameList = default;
    [SerializeField] private TextMeshProUGUI _name = default;
    [SerializeField] private InformationList _infoList = default;
    [SerializeField] private GameObject _infoContainer = default;
    [SerializeField] private GameObject _noCharacterMsg = default;
    private GameData _gameData;
    private NPC _selectedNPC;

    private void Awake() {
        _addCharacter.onClick.AddListener(AddCharacter);
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
            SetCharacter(_gameData.NPCs[0]);
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
        var addCharacterPopup = await PopupManager.Instance.GetOrLoadPopup<AddCharacterPopup>();
        addCharacterPopup.Populate(OnCharacterCreation);
        addCharacterPopup.gameObject.SetActive(true);
    }

    private void OnCharacterCreation(NPC npc) {
        _gameData.NPCs.Add(npc);
        Refresh();
    }

    public override object GetRestorationData() {
        PopupData data = new PopupData {
            Data = _gameData
        };

        return data;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            Populate(popupData.Data);
            if (popupData.SelectedNPC != null) {
                SetCharacter(popupData.SelectedNPC);
            }
        }
    }
}