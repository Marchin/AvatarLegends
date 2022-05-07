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

    public async void ShowDescription() {
        var msgPopup = await PopupManager.Instance.GetOrLoadPopup<MessagePopup>(restore: false);
        msgPopup.Populate(Description, "Description");
    }
}