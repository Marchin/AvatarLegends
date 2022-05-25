using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class ConditionState {
    [JsonProperty("name")]
    public string Name;

    [JsonProperty("on")]
    public bool On;
}

[JsonObject(MemberSerialization.OptIn)]
public class Condition : IDataEntry, IOnHover {
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("effect")]
    public string Effect;

    [JsonProperty("clearing_condition")]
    public string ClearingCondition;

    public Action OnHoverIn => () => TooltipManager.Instance.ShowMessage(Effect);
    public Action OnHoverOut => TooltipManager.Instance.Hide;

    private Action _onRefresh;
    
    public string InfoDisplay => $"Effect: {Effect}\n\nClearing Condition: {ClearingCondition}";

    public List<InformationData> RetrieveData(Action refresh, Action reload) {
        _onRefresh = refresh;

        List<InformationData> result = new List<InformationData>();

        if (!string.IsNullOrEmpty(Effect)) {
            result.Add(new InformationData {
                Content = nameof(Effect),
                OnHoverIn = OnHoverIn,
                OnHoverOut = OnHoverOut,
            });
        }

        if (!string.IsNullOrEmpty(ClearingCondition)) {
            result.Add(new InformationData {
                Content = "Clearing Condition",
                OnHoverIn = () => TooltipManager.Instance.ShowMessage(ClearingCondition),
                OnHoverOut = TooltipManager.Instance.Hide
            });
        }

        return result;
    }

    
    public Filter GetFilterData() {
        Filter filter = new Filter();
        
        filter.Toggles.Add(new ToggleActionData(
            "Reverse",
            action: (list, on) => {
                if (on) {
                    list.Reverse();
                }
                return list;
            }
        ));

        return filter;
    }
}
