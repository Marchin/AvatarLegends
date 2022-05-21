using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class Status : IDataEntry {
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description;
    
    [JsonProperty("positive")]
    public bool Positive;
    private Action _onRefresh;
    public Action OnMoreInfo => null;

    public List<InformationData> RetrieveData(Action refresh, Action reload) {
        _onRefresh = refresh;

        var result = new List<InformationData>();
        
        if (!string.IsNullOrEmpty(Description)) {
            result.Add(new InformationData {
                Content = nameof(Description),
                OnMoreInfo = ShowDescription,
            });
        }

        result.Add(new InformationData {
            Prefix = nameof(Positive),
            IsToggleOn = Positive
        });


        return result;
    }

    public void ShowDescription() {
        MessagePopup.ShowMessage(Description, nameof(Description));
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