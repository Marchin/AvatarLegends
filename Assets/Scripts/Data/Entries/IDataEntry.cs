using System;
using System.Collections.Generic;

public enum ETraining {
    Earth,
    Water,
    Fire,
    Air,
    Weapons,
    Tech
}

public interface IDataEntry {
    string Name { get; set; }
    List<InformationData> RetrieveData(Action refresh, Action reload);
    Action OnMoreInfo { get; }
    Filter GetFilterData();


    static async void AddEntry<T>(
        IReadOnlyCollection<string> current, 
        IReadOnlyCollection<T> pool,
        Action<List<string>> onDone
    ) where T : IDataEntry {
        List<string> entriesToAdd = new List<string>(current);
        List<InformationData> infoList = new List<InformationData>(pool.Count);
        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>(restore: false);
        List<T> availableEntries = GetAvailableEntries<T>(current, pool);

        Refresh();

        void Refresh() {
            infoList.Clear();
            foreach (var entry in availableEntries) {
                infoList.Add(new InformationData {
                    Content = entry.Name,
                    IsToggleOn = entriesToAdd.Contains(entry.Name),
                    OnToggle = on => {
                        if (on) {
                            entriesToAdd.Add(entry.Name);
                        } else {
                            entriesToAdd.Remove(entry.Name);
                        }

                        Refresh();
                    },
                    OnMoreInfo = entry.OnMoreInfo
                });
            }

            listPopup.Populate(infoList,
                $"Add Entries",
                () => onDone(entriesToAdd)
            );
        }
    }

    static List<T> GetAvailableEntries<T>(IReadOnlyCollection<string> current, IReadOnlyCollection<T> pool) where T : IDataEntry {
        List<T> availableEntries = new List<T>(pool);

        foreach (var entry in current) {
            availableEntries.Remove(availableEntries.Find(x => x.Name == entry));
        }

        return availableEntries;
    }
}
