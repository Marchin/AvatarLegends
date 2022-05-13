using Newtonsoft.Json;
using System.Collections.Generic;

public class UserData {
    [JsonProperty("campaigns")]
    public Dictionary<string, Campaign> Campaigns = new Dictionary<string, Campaign>();

    [JsonProperty("conditions")]
    public Dictionary<string, Condition> Conditions = new Dictionary<string, Condition>();

    [JsonProperty("techniques")]
    public Dictionary<string, Technique> Techniques = new Dictionary<string, Technique>();

    [JsonProperty("status")]
    public Dictionary<string, Status> Statuses = new Dictionary<string, Status>();

    public string SelectedCampaignName;
    public Campaign SelectedCampaign => Campaigns[SelectedCampaignName];
    public Session CurrentSession => SelectedCampaign.CurrentSession;
}
