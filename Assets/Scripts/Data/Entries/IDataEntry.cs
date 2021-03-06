using System;
using System.Collections.Generic;

public enum ETraining {
    Earth,
    Water,
    Fire,
    Air,
    Weapons,
    Technology
}

public interface IDisplayName {
    string DisplayName { get; }
}

public interface IOnMoreInfo {
    Action OnMoreInfo { get; }
}

public interface IOnHover {
    Action OnHoverIn { get; }
    Action OnHoverOut { get; }
}

public interface IDataEntry {
    string Name { get; set; }
    List<InformationData> RetrieveData(Action refresh, Action reload);
    Filter GetFilterData();


    static async void AddEntry<T>(
        IReadOnlyCollection<string> current, 
        IReadOnlyCollection<T> pool,
        Action<List<string>> onDone,
        string title = "Add Entries",
        int maxCap = -1,
        Func<IDataEntry, string> customName = null
    ) where T : IDataEntry {
        List<string> entriesToAdd = new List<string>(current.Count);
        List<InformationData> infoList = new List<InformationData>(pool.Count);
        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();
        List<T> availableEntries = GetAvailableEntries<T>(current, pool);
        int slotsAvailable = maxCap - current.Count;

        Refresh();

        void Refresh() {
            var scrollData = listPopup.GetScrollData();
            infoList.Clear();
            foreach (var entry in availableEntries) {
                infoList.Add(new InformationData {
                    Content = customName?.Invoke(entry) ?? entry.Name,
                    IsToggleOn = entriesToAdd.Contains(entry.Name),
                    OnToggle = on => {
                        if (!on || (maxCap == -1) || (entriesToAdd.Count < slotsAvailable)) {
                            if (on) {
                                entriesToAdd.Add(entry.Name);
                            } else {
                                entriesToAdd.Remove(entry.Name);
                            }

                            Refresh();
                        } else {
                            MessagePopup.ShowMessage(
                                "You've already reached the maximum amount possible.",
                                "Maxed"
                            );
                        }
                    },
                    OnMoreInfo = (entry as IOnMoreInfo)?.OnMoreInfo,
                    OnHoverIn = (entry as IOnHover)?.OnHoverIn,
                    OnHoverOut = (entry as IOnHover)?.OnHoverOut
                });
            }

            string popupTitle = title;

            if (maxCap > 0) {
                popupTitle += $" ({entriesToAdd.Count}/{slotsAvailable})";
            }

            listPopup.Populate(
                () => infoList,
                popupTitle,
                () => onDone(entriesToAdd),
                scrollData
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
