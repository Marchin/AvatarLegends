using Newtonsoft.Json;
using System.Collections.Generic;

public class UserData {
    [JsonProperty("ncps")]
    public List<NPC> NPCs;

    [JsonProperty("enemies")]
    public List<NPC> Enemies;

    [JsonProperty("custom_conditions")]
    public List<Condition> CustomConditions;

    [JsonProperty("custom_techniques")]
    public List<Technique> CustomTechniques;
    // PCs
    // Encounters
}
