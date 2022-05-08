using Newtonsoft.Json;
using System.Collections.Generic;

public class UserData {
    [JsonProperty("ncps")]
    public Dictionary<string, NPC> NPCs = new Dictionary<string, NPC>();

    [JsonProperty("conditions")]
    public Dictionary<string, Condition> Conditions = new Dictionary<string, Condition>();

    [JsonProperty("techniques")]
    public Dictionary<string, Technique> Techniques = new Dictionary<string, Technique>();

    [JsonProperty("status")]
    public Dictionary<string, Status> Statuses = new Dictionary<string, Status>();
    
    [JsonProperty("engagement")]
    public Dictionary<string, Engagement> Engagements = new Dictionary<string, Engagement>();
    // PCs
}
