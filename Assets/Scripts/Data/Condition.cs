using Newtonsoft.Json;

public class Condition {
    [JsonProperty("name")]
    public string Name;

    [JsonProperty("effect")]
    public string Effect;

    [JsonProperty("clearing_condition")]
    public string ClearingCondition;
}
