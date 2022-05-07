using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

public class AppData {
    const string UserDataPref= "user_data";
    private UserData User;

    // Playbooks
    [JsonProperty("ncps")]
    private Dictionary<string, NPC> _dataNPCs = new Dictionary<string, NPC>();
    private Dictionary<string, NPC> _npcs;
    public Dictionary<string, NPC> NPCs
    {
        get {
            if (_npcs == null) {
                _npcs = new Dictionary<string, NPC>();

                foreach (var npc in _dataNPCs) {
                    _npcs.Add(npc.Key, npc.Value);
                }

                foreach (var npc in User.NPCs) {
                    _npcs.Add(npc.Key, npc.Value);
                }
            }

            return _npcs;
        }
        set {
            User.NPCs = new Dictionary<string, NPC>(value);

            foreach (var npc in _dataNPCs) {
                User.NPCs.Remove(npc.Key);
            }
            
            _npcs = value;
        }
    }

    public bool IsEditable(NPC npc) {
        return !_dataNPCs.ContainsKey(npc.Name);
    }

    [JsonProperty("conditions")]
    private Dictionary<string, Condition> _dataConditions = new Dictionary<string, Condition>();
    private Dictionary<string, Condition> _conditions;
    public Dictionary<string, Condition> Conditions
    {
        get {
            if (_conditions == null) {
                _conditions = new Dictionary<string, Condition>();

                foreach (var condition in _dataConditions) {
                    _conditions.Add(condition.Key, condition.Value);
                }

                foreach (var condition in User.Conditions) {
                    _conditions.Add(condition.Key, condition.Value);
                }
            }

            return _conditions;
        }
        set {
            User.Conditions = new Dictionary<string, Condition>(value);

            foreach (var condition in _dataConditions) {
                User.Conditions.Remove(condition.Key);
            }
            
            _conditions = value;
        }
    }

    [JsonProperty("techniques")]
    private Dictionary<string, Technique> _dataTechniques = new Dictionary<string, Technique>();
    private Dictionary<string, Technique> _techniques;
    public Dictionary<string, Technique> Techniques
    {
        get {
            if (_techniques == null) {
                _techniques = new Dictionary<string, Technique>();

                foreach (var technique in _dataTechniques) {
                    _techniques.Add(technique.Key, technique.Value);
                }

                foreach (var technique in User.Techniques) {
                    _techniques.Add(technique.Key, technique.Value);
                }
            }

            return _techniques;
        }
        set {
            User.Techniques = new Dictionary<string, Technique>(value);

            foreach (var technique in _dataTechniques) {
                User.Techniques.Remove(technique.Key);
            }
            
            _techniques = value;
        }
    }

    public bool IsEditable(Technique technique) {
        return !_dataTechniques.ContainsKey(technique.Name);
    }

    [JsonProperty("statuses")]
    private Dictionary<string, Status> _dataStatuses = new Dictionary<string, Status>();
    private Dictionary<string, Status> _statuses;
    public Dictionary<string, Status> Statuses
    {
        get {
            if (_statuses == null) {
                _statuses = new Dictionary<string, Status>();

                foreach (var status in _dataStatuses) {
                    _statuses.Add(status.Key, status.Value);
                }

                foreach (var status in User.Statuses) {
                    _statuses.Add(status.Key, status.Value);
                }
            }

            return _statuses;
        }
        set {
            User.Statuses = new Dictionary<string, Status>(value);

            foreach (var status in _dataStatuses) {
                User.Statuses.Remove(status.Key);
            }
            
            _statuses = value;
        }
    }

    public bool IsEditable(Status status) {
        return !_dataStatuses.ContainsKey(status.Name);
    }
    
    public Dictionary<string, NPC> Enemies {
        get => User.Enemies;
        set => User.Enemies = value;
    }


    public AppData() {
        string data = PlayerPrefs.GetString(UserDataPref, "{}");
        User = JsonConvert.DeserializeObject<UserData>(data) ?? new UserData();
    }
    
    public void RecordData() {
        string data = JsonConvert.SerializeObject(User);
        PlayerPrefs.SetString(UserDataPref, data);
    }
}