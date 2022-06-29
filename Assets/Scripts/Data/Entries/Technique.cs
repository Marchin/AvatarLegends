using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public static class TechniqueUtils {
    public static string GetColoredText(this Technique.EApproach approach) {
        string result = "";

        switch (approach) {
            case Technique.EApproach.Attack: {
                result = "<color=#d72929>Attack</color>";
            } break;
            case Technique.EApproach.Defend: {
                result = "<color=#33aa33>Defend</color>";
            } break;
            case Technique.EApproach.Evade: {
                result = "<color=#d2c592>Evade</color>";
            } break;
        }

        return result;
    }

    public static string GetDisplayText(this Technique.EApproach approach) {
        string result = "";

        switch (approach) {
            case Technique.EApproach.Attack: {
                result = "Advance & Attack";
            } break;
            case Technique.EApproach.Defend: {
                result = "Defend & Maneuver";
            } break;
            case Technique.EApproach.Evade: {
                result = "Evade & Observe";
            } break;
        }

        return result;
    }
}

[JsonObject(MemberSerialization.OptIn)]
public class Technique : IDataEntry, IOnHover {
    public enum EApproach {
        Attack,
        Defend,
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
        Technology
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

    public string ColoredName {
        get {
            string result = Name;

            switch (Mastery) {
                case EMastery.Earth: {
                    result = $"<color=#6ece00>{result}</color>";
                    break;
                }
                case EMastery.Water: {
                    result = $"<color=#66ACff>{result}</color>";
                    break;
                }
                case EMastery.Air: {
                    result = $"<color=#ffe166>{result}</color>";
                    break;
                }
                case EMastery.Fire: {
                    result = $"<color=#FF6666>{result}</color>";
                    break;
                }
                case EMastery.Weapons: {
                    result = $"<color=#BBBBBB>{result}</color>";
                    break;
                }
                case EMastery.Technology: {
                    result = $"<color=#a28fda>{result}</color>";
                    break;
                }
                case EMastery.Group: {
                    result = $"{result} (Group)";
                    break;
                }
            }

            result += $" ({Approach.GetColoredText()})";

            return result;
        }
    }
    
    public string InfoDisplay {
        get {
            string infoContent = $"Mastery: {Mastery}\nApproach: {Approach.GetDisplayText()}\n\n{Description}";

            if (Rare) {
                infoContent += "\n\n(Rare Technique)";
            }

            return infoContent;
        }
    }
    
    public Action OnHoverIn => () => TooltipManager.Instance.ShowMessage(InfoDisplay);
    public Action OnHoverOut => TooltipManager.Instance.Hide;

    private bool _showDescription = true;
    private Action _refresh;

    public List<InformationData> RetrieveData(Action refresh, Action reload) {
        _refresh = refresh;

        var result = new List<InformationData>();

        result.Add(new InformationData {
            Prefix = nameof(Mastery),
            Content = Mastery.ToString(),
        });

        result.Add(new InformationData {
            Prefix = nameof(Approach),
            Content = Approach.GetDisplayText(),
        });

        result.Add(new InformationData {
            Prefix = nameof(Rare),
            IsToggleOn = Rare
        });

        if (!string.IsNullOrEmpty(Description)) {
            Action onDescriptionDropdown = () => {
                _showDescription = !_showDescription;
                _refresh();
            };

            result.Add(new InformationData {
                OnDropdown = onDescriptionDropdown,
                Content = nameof(Description),
                Expanded = _showDescription
            });

            if (_showDescription) {
                result.Add(new InformationData {
                    ExpandableContent = Description,
                    IndentLevel = 1
                });
            }
        }

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
