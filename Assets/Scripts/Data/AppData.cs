using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

[JsonObject(MemberSerialization.OptIn)]
public class AppData {
    const string UserDataPref= "user_data";
    public UserData User => UserDataManager.Instance.Data;

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

                foreach (var npc in User.SelectedCampaign.NPCs) {
                    _npcs.Add(npc.Key, npc.Value);
                }
            }

            return _npcs;
        }
        set {
            User.SelectedCampaign.NPCs = new Dictionary<string, NPC>(value);

            foreach (var npc in _dataNPCs) {
                User.SelectedCampaign.NPCs.Remove(npc.Key);
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

    public bool IsEditable(Condition condition) {
        return !_dataConditions.ContainsKey(condition.Name);
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
    
    [JsonProperty("playbooks")]
    private Dictionary<string, Playbook> _dataPlaybooks = new Dictionary<string, Playbook>();
    private Dictionary<string, Playbook> _playbooks;
    public Dictionary<string, Playbook> Playbooks
    {
        get {
            if (_playbooks == null) {
                _playbooks = new Dictionary<string, Playbook>();

                foreach (var playbook in _dataPlaybooks) {
                    _playbooks.Add(playbook.Key, playbook.Value);
                }

                foreach (var playbook in User.Playbooks) {
                    _playbooks.Add(playbook.Key, playbook.Value);
                }
            }

            return _playbooks;
        }
        set {
            User.Playbooks = new Dictionary<string, Playbook>(value);

            foreach (var playbook in _dataPlaybooks) {
                User.Playbooks.Remove(playbook.Key);
            }
            
            _playbooks = value;
        }
    }

    public bool IsEditable(Playbook status) {
        return !_dataPlaybooks.ContainsKey(status.Name);
    }

    public void ClearCache() {
        _npcs = null;
        _techniques = null;
        _conditions = null;
        _statuses = null;
        _playbooks = null;
    }
}