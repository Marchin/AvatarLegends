using Newtonsoft.Json;
using System.Collections.Generic;

public class GameData {
    // Playbooks
    [JsonProperty("ncps")]
    public List<NPC> NPCs = new List<NPC>();

    [JsonProperty("conditions")]
    public List<Condition> Conditions = new List<Condition>();

    [JsonProperty("techniques")]
    public List<Technique> Techniques = new List<Technique>();

    [JsonProperty("statuses")]
    public List<Status> Statuses = new List<Status>();

    [JsonProperty("user")]
    public UserData User;
}