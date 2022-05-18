using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class Status : IDataEntry {
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description;
    
    [JsonProperty("is_positive")]
    public bool IsPositive;
    private Action _onRefresh;
    public Action OnMoreInfo => null;

    public List<InformationData> RetrieveData(Action refresh) {
        _onRefresh = refresh;

        var result = new List<InformationData>();
        
        if (!string.IsNullOrEmpty(Description)) {
            result.Add(new InformationData {
                Content = "Description",
                OnMoreInfo = ShowDescription,
            });
        }

        result.Add(new InformationData {
            Prefix = "Is Positive",
            IsToggleOn = IsPositive
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
            action: (list, isOn) => {
                if (isOn) {
                    list = list.FindAll(x => (x as Status).IsPositive);
                }
                return list;
            }
        ));

        filter.Toggles.Add(new ToggleActionData(
            "Negative",
            action: (list, isOn) => {
                if (isOn) {
                    list = list.FindAll(x => !(x as Status).IsPositive);
                }
                return list;
            }
        ));

        filter.Toggles.Add(new ToggleActionData(
            "Reverse",
            action: (list, isOn) => {
                if (isOn) {
                    list.Reverse();
                }
                return list;
            }
        ));

        return filter;
    }
}