using Newtonsoft.Json;

public class Status {
    [JsonProperty("name")]
    public string Name;

    [JsonProperty("description")]
    public string Description;
    
    [JsonProperty("is_positive")]
    public bool IsPositive;
}