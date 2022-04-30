using Newtonsoft.Json;
using System.Collections.Generic;

public class NPC {
    public enum ETraining {
        Earth,
        Water,
        Fire,
        Air,
        Weapons,
        Tech
    }

    public enum EType {
        Minor,
        Major,
        Master,
        Legendary,
    }

    [JsonProperty("name")]
    public string Name;
    
    [JsonProperty("description")]
    public string Description;

    [JsonProperty("type")]
    public EType Type;

    [JsonProperty("is_group")]
    public bool IsGroup;

    [JsonProperty("training")]
    public ETraining Training;

    [JsonProperty("principle")]
    public string Principle;

    [JsonProperty("balance")]
    public int Balance;
    
    [JsonProperty("techniques")]
    public List<Technique> Techniques = new List<Technique>();
    
    [JsonProperty("statuses")]
    public List<Status> Statuses = new List<Status>();

    [JsonProperty("conditions")]
    public List<Condition> Conditions = new List<Condition>();


    public List<InformationData> RetrieveData() {
        List<InformationData> result = new List<InformationData>();

        result.Add(new InformationData {
            Prefix = "Type",
            Content = Type.ToString(),
        });

        result.Add(new InformationData {
            Prefix = "Training",
            Content = Training.ToString(),
        });

        result.Add(new InformationData {
            Prefix = "Principle",
            Content = Principle,
            InitValue = Balance,
            MaxValue = GetMaxBalance(),
            OnValueChange = ChangeBalance
        });

        return result;
    }

    public void ChangeBalance(int value) {
        int maxBalance = GetMaxBalance();
        UnityEngine.Debug.Assert((value >= 0) && (value <= maxBalance), "Invalid Balance");

        Balance = UnityEngine.Mathf.Clamp(value, 0, maxBalance);
    }

    public int GetMaxBalance() {
        int result = 0;

        switch (Type) {
            case EType.Minor: {
                result = 1;
            } break;
            case EType.Major: {
                result = 2;
            } break;
            case EType.Master: {
                result = 3;
            } break;
            case EType.Legendary: {
                result = 4;
            } break;
        }

        return result;
    }
}