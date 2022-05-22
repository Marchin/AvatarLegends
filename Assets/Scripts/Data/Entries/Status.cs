using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[JsonObject(MemberSerialization.OptIn)]
public class Status : IDataEntry {
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description;
    
    [JsonProperty("positive")]
    public bool Positive;
    private Action _onRefresh;

    public List<InformationData> RetrieveData(Action refresh, Action reload) {
        _onRefresh = refresh;

        var result = new List<InformationData>();
        
        if (!string.IsNullOrEmpty(Description)) {
            result.Add(new InformationData {
                Content = nameof(Description),
                OnHoverIn = () => TooltipManager.Instance.ShowMessage(Description),
                OnHoverOut = TooltipManager.Instance.Hide,
            });
        }

        result.Add(new InformationData {
            Prefix = nameof(Positive),
            IsToggleOn = Positive
        });


        return result;
    }

    public Filter GetFilterData() {
        Filter filter = new Filter();
        
        filter.Toggles.Add(new ToggleActionData(
            "Positive",
            action: (list, on) => {
                if (on) {
                    list = list.FindAll(x => (x as Status).Positive);
                }
                return list;
            }
        ));

        filter.Toggles.Add(new ToggleActionData(
            "Negative",
            action: (list, on) => {
                if (on) {
                    list = list.FindAll(x => !(x as Status).Positive);
                }
                return list;
            }
        ));

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