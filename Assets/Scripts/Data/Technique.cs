using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class Technique : IDataEntry {
    public enum EApproach {
        Attack,
        Defense,
        Evade
    }
    public enum EMastery {
        Universal,
        Group,
        Earth,
        Water,
        Fire,
        Air,
        Weapons,
        Tech
    }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("mastery")]
    public EMastery Mastery;

    [JsonProperty("approach")]
    public EApproach Approach;

    [JsonProperty("description")]
    public string Description;

    [JsonProperty("rare")]
    public bool Rare;
    
    private Action _onRefresh;

    public List<InformationData> RetrieveData(Action refresh) {
        _onRefresh = refresh;

        var result = new List<InformationData>();

        result.Add(new InformationData {
            Prefix = "Mastery",
            Content = Mastery.ToString(),
        });

        result.Add(new InformationData {
            Prefix = "Approach",
            Content = Approach.ToString(),
        });

        if (!string.IsNullOrEmpty(Description)) {
            result.Add(new InformationData {
                Content = "Description",
                OnMoreInfo = ShowDescription,
            });
        }

        result.Add(new InformationData {
            Prefix = "Is Rare",
            IsToggleOn = Rare,
            OnToggle = isOn => Rare = isOn
        });

        return result;
    }
    
    public async void ShowDescription() {
        var msgPopup = await PopupManager.Instance.GetOrLoadPopup<MessagePopup>(restore: false);
        msgPopup.Populate(Description, "Description");
    }
}
