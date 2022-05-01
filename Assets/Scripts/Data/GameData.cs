using Newtonsoft.Json;
using System.Collections.Generic;

public class GameData {
    // Playbooks
    [JsonProperty("ncps")]
    public Dictionary<string, NPC> NPCs = new Dictionary<string, NPC>();

    [JsonProperty("conditions")]
    public Dictionary<string, Condition> Conditions = new Dictionary<string, Condition>();

    [JsonProperty("techniques")]
    public Dictionary<string, Technique> Techniques = new Dictionary<string, Technique>();

    [JsonProperty("statuses")]
    public Dictionary<string, Status> Statuses = new Dictionary<string, Status>();

    [JsonProperty("user")]
    public UserData User;
}