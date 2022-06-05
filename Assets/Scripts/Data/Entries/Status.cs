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

    private bool _showDescription = true;
    private Action _refresh;

    public List<InformationData> RetrieveData(Action refresh, Action reload) {
        _refresh = refresh;

        var result = new List<InformationData>();
        
        result.Add(new InformationData {
            Prefix = nameof(Positive),
            IsToggleOn = Positive
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