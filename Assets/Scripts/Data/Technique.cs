using Newtonsoft.Json;

public class Technique {
    public enum EApproach {
        Attack,
        Defense,
        Evade
    }
    public enum EMastery {
        Universal,
        Group,
        Earth,
        Water,
        Fire,
        Air,
        Weapons,
        Tech
    }

    [JsonProperty("name")]
    public string Name;

    [JsonProperty("description")]
    public string Description;

    [JsonProperty("approach")]
    public EApproach Approach;

    [JsonProperty("mastery")]
    public EMastery Mastery;

    [JsonProperty("rare")]
    public bool Rare;
}
