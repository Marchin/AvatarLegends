using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class Playbook : IDataEntry {
    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("description")]
    public string Description;

    [JsonProperty("principles")]
    public string Principles;

    [JsonProperty("conditions")]
    public List<string> Conditions;

    public Action OnMoreInfo => null;
    private Action _refresh;
    private bool _showConditions;
    private AppData Data => ApplicationManager.Instance.Data;

    public List<InformationData> RetrieveData(Action refresh) {
        _refresh = refresh;

        List<InformationData> result = new List<InformationData>();

        if (!string.IsNullOrEmpty(Description)) {
            result.Add(new InformationData {
                Content = nameof(Description),
                OnMoreInfo = () => MessagePopup.ShowMessage(Description, nameof(Description))
            });
        }

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
                    OnMoreInfo = Data.Conditions[conditionName].ShowInfo,
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
