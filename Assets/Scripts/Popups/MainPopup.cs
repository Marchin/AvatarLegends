using TMPro;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class MainPopup : Popup {
    const string DataPath = "Assets/Data/Data.json";
    public enum Tab {
        NPCs
    }

    [SerializeField] private Button _addCharacter = default;
    [SerializeField] private ButtonList _nameList = default;
    [SerializeField] private TextMeshProUGUI _name = default;
    [SerializeField] private InformationList _infoList = default;
    [SerializeField] private AddCharacterPopup _addCharacterPopup = default;
    [SerializeField] private GameObject _infoContainer = default;
    [SerializeField] private GameObject _noCharacterMsg = default;
    private GameData _gameData;

    private void Awake() {
        string data = File.ReadAllText(DataPath);
        _gameData = JsonConvert.DeserializeObject<GameData>(data) ?? new GameData();
        
        _addCharacter.onClick.AddListener(AddCharacter);
        Refresh();
    }

    private void OnDestroy() {
        string data = JsonConvert.SerializeObject(_gameData);
        File.WriteAllText(DataPath, data);
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
    }

    public void Populate() {

    }

    private void AddCharacter() {
        _addCharacterPopup.Populate(OnCharacterCreation);
        _addCharacterPopup.gameObject.SetActive(true);
    }

    private void OnCharacterCreation(NPC npc) {
        _gameData.NPCs.Add(npc);
        Refresh();
    }

    public override object GetRestorationData() {
        throw new System.NotImplementedException();
    }

    public override void Restore(object data) {
        throw new System.NotImplementedException();
    }
}