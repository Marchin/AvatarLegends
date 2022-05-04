using Newtonsoft.Json;
using System.Collections.Generic;

public class UserData {
    [JsonProperty("ncps")]
    public Dictionary<string, NPC> NPCs = new Dictionary<string, NPC>();

    [JsonProperty("enemies")]
    public Dictionary<string, NPC> Enemies = new Dictionary<string, NPC>();

    [JsonProperty("custom_conditions")]
    public Dictionary<string, Condition> CustomConditions = new Dictionary<string, Condition>();

    [JsonProperty("custom_techniques")]
    public Dictionary<string, Technique> CustomTechniques = new Dictionary<string, Technique>();
    // PCs
    // Encounters
}
