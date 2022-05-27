using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[JsonObject(MemberSerialization.OptIn)]
public class Playbook : IDataEntry {
    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("description")]
    public string Description;

    [JsonProperty("principles")]
    public string Principles;

    [JsonProperty("conditions")]
    public List<string> Conditions;

    private Action _refresh;
    private bool _showDescription;
    private bool _showConditions;
    private AppData Data => ApplicationManager.Instance.Data;

    public List<InformationData> RetrieveData(Action refresh, Action reload) {
        _refresh = refresh;

        List<InformationData> result = new List<InformationData>();

        if (!string.IsNullOrEmpty(Principles)) {
            result.Add(new InformationData {
                Prefix = nameof(Principles),
                Content = Principles,
            });
        }

        Action onConditionDropdown = () => {
            _showConditions = !_showConditions;
            _refresh();
        };

        result.Add(new InformationData {
            Content = nameof(Conditions),
            OnDropdown = (Conditions.Count > 0) ? onConditionDropdown : null,
        });

        if (_showConditions) {
            Conditions.Sort();
            foreach (var conditionName in Conditions) {
                if (!Data.Conditions.ContainsKey(conditionName)) {
                    continue;
                }

                result.Add(new InformationData {
                    Content = conditionName,
                    OnHoverIn = () => TooltipManager.Instance.ShowMessage(Data.Conditions[conditionName].InfoDisplay),
                    OnHoverOut = TooltipManager.Instance.Hide,
                    IndentLevel = 1
                });
            }
        }

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
