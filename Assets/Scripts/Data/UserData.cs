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

    [JsonProperty("playbook")]
    public Dictionary<string, Playbook> Playbooks = new Dictionary<string, Playbook>();

    public string SelectedCampaignName;
    public Campaign SelectedCampaign => 
        (!string.IsNullOrEmpty(SelectedCampaignName) && Campaigns.ContainsKey(SelectedCampaignName)) ?
            Campaigns[SelectedCampaignName] : null;
    public Session CurrentSession => SelectedCampaign?.CurrentSession;

    public bool IsClear =>
        (Campaigns.Count == 0) &&
        (Conditions.Count == 0) &&
        (Statuses.Count == 0) &&
        (Playbooks.Count == 0);
}
