using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[JsonObject(MemberSerialization.OptIn)]
public static class ApproachUtils {
    public static string GetDisplayText(this Technique.EApproach approach) {
        string result = "";

        switch (approach) {
            case Technique.EApproach.Attack: {
                result = "Advance & Attack";
            } break;
            case Technique.EApproach.Defense: {
                result = "Defense & Maneuver";
            } break;
            case Technique.EApproach.Evade: {
                result = "Evade & Observe";
            } break;
        }

        return result;
    }
}

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
    
    public string InfoDisplay {
        get {
            string infoContent = $"Mastery: {Mastery}\nApproach: {Approach.GetDisplayText()}\n\n{Description}";

            if (Rare) {
                infoContent += "\n\n(Rare Technique)";
            }

            return infoContent;
        }
    }
    
    private Action _onRefresh;

    public List<InformationData> RetrieveData(Action refresh, Action reload) {
        _onRefresh = refresh;

        var result = new List<InformationData>();

        result.Add(new InformationData {
            Prefix = nameof(Mastery),
            Content = Mastery.ToString(),
        });

        result.Add(new InformationData {
            Prefix = nameof(Approach),
            Content = Approach.GetDisplayText(),
        });

        if (!string.IsNullOrEmpty(Description)) {
            result.Add(new InformationData {
                Content = nameof(Description),
                OnHoverIn = () => TooltipManager.Instance.ShowMessage(Description),
                OnHoverOut = TooltipManager.Instance.Hide,
            });
        }

        result.Add(new InformationData {
            Prefix = nameof(Rare),
            IsToggleOn = Rare
        });

        return result;
    }
    
    public Filter GetFilterData() {
        Filter filter = new Filter();
        
        var masteryFilter = new FilterChannelData(
            nameof(Mastery),
            entry => new List<int> { (int)(entry as Technique).Mastery }
        );

        string[] masteries = Enum.GetNames(typeof(EMastery));
        masteryFilter.Elements = new List<FilterChannelEntryData>(masteries.Length);
        for (int iMastery = 0; iMastery < masteries.Length; ++iMastery) {
            masteryFilter.Elements.Add(
                new FilterChannelEntryData { Name = ((EMastery)iMastery).ToString() });
        }

        filter.Filters.Add(masteryFilter);


        var approachFilter = new FilterChannelData(
            nameof(Approach),
            entry => new List<int> { (int)(entry as Technique).Approach }
        );

        string[] approaches = Enum.GetNames(typeof(EApproach));
        approachFilter.Elements = new List<FilterChannelEntryData>(approaches.Length);
        for (int iApproach = 0; iApproach < approaches.Length; ++iApproach) {
            approachFilter.Elements.Add(
                new FilterChannelEntryData { Name = ((EApproach)iApproach).GetDisplayText() });
        }

        filter.Filters.Add(approachFilter);

        filter.Toggles.Add(new ToggleActionData(
            "Rare",
            action: (list, on) => {
                if (on) {
                    list = list.FindAll(x => (x as Technique).Rare);
                }
                return list;
            }
        ));

        filter.Toggles.Add(new ToggleActionData(
            "Not Rare",
            action: (list, on) => {
                if (on) {
                    list = list.FindAll(x => !(x as Technique).Rare);
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
