using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class PC : IDataEntry {
    public const int ConditionsAmount = 4;

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("player")]
    public string Player { get; set; }

    [JsonProperty("backstory")]
    public string Backstory;

    [JsonProperty("training")]
    public ETraining Training;

    [JsonProperty("playbook")]
    public string Playbook;

    public Action OnMoreInfo => null;
    private Action _refresh;
    private AppData Data => ApplicationManager.Instance.Data;

    public List<InformationData> RetrieveData(Action refresh) {
        _refresh = refresh;

        List<InformationData> result = new List<InformationData>();

        if (!string.IsNullOrEmpty(Player)) {
            result.Add(new InformationData {
                Prefix = nameof(Player),
                Content = Player,
            });
        }

        if (!string.IsNullOrEmpty(Backstory)) {
            result.Add(new InformationData {
                Prefix = nameof(Backstory),
                Content = Backstory,
            });
        }

        result.Add(new InformationData {
            Prefix = nameof(Training),
            Content = Training.ToString(),
        });

        if (Data.Playbooks.ContainsKey(Playbook)) {
            result.Add(new InformationData {
                Prefix = nameof(Playbook),
                Content = Playbook,
                OnMoreInfo = async () => {
                    var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
                    Refresh();

                    void Refresh() {
                        listPopup.Populate(Data.Playbooks[Playbook].RetrieveData(Refresh), Playbook, null);
                    }
                }
            });
        }

        return result;
    }
}
