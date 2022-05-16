using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ConditionState {
    [JsonProperty("name")]
    public string Name;

    [JsonProperty("is_on")]
    public bool IsOn;
}

public class Condition : IDataEntry {
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("effect")]
    public string Effect;

    [JsonProperty("clearing_condition")]
    public string ClearingCondition;

    private Action _onRefresh;
    public Action OnMoreInfo => null;

    public List<InformationData> RetrieveData(Action refresh) {
        _onRefresh = refresh;

        List<InformationData> result = new List<InformationData>();

        if (!string.IsNullOrEmpty(Effect)) {
            result.Add(new InformationData {
                Content = nameof(Effect),
                OnMoreInfo = () => MessagePopup.ShowMessage(Effect, nameof(Effect))
            });
        }

        if (!string.IsNullOrEmpty(ClearingCondition)) {
            result.Add(new InformationData {
                Content = nameof(ClearingCondition),
                OnMoreInfo = () => MessagePopup.ShowMessage(ClearingCondition, nameof(ClearingCondition))
            });
        }

        return result;
    }

    public void ShowInfo() {
        string infoContent = $"Effect: {Effect}\n\nClearing Condition: {ClearingCondition}";

        MessagePopup.ShowMessage(infoContent, Name);
    }
}
