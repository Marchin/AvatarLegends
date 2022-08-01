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


    static void AddEntry<T>(
        IReadOnlyCollection<string> current, 
        IReadOnlyCollection<T> pool,
        Action<List<string>> onDone,
        string title = "Add Entries",
        int maxCap = -1,
        Func<IDataEntry, string> customName = null,
        Func<IReadOnlyList<ButtonData>> buttonsRetriever = null
    ) where T : IDataEntry {
        AddEntry<T>(current, () => pool, onDone, title, maxCap, customName, buttonsRetriever);
    }

    static async void AddEntry<T>(
        IReadOnlyCollection<string> current, 
        Func<IReadOnlyCollection<T>> poolGet,
        Action<List<string>> onDone,
        string title = "Add Entries",
        int maxCap = -1,
        Func<IDataEntry, string> customName = null,
        Func<IReadOnlyList<ButtonData>> buttonsRetriever = null
    ) where T : IDataEntry {
        IReadOnlyCollection<T> pool = poolGet?.Invoke();
        List<InformationData> infoList = new List<InformationData>(pool.Count);
        List<string> entriesToAdd = new List<string>(current.Count);
        var listPopup = await PopupManager.Instance.GetOrLoadPopup<ListPopup>();

        Refresh();

        void Refresh() {
            int slotsAvailable = maxCap - current.Count;
            var scrollData = listPopup.GetScrollData();
            string popupTitle = title;

            if (maxCap > 0) {
                popupTitle += $" ({entriesToAdd.Count}/{slotsAvailable})";
            }

            listPopup.Populate(
                GetData,
                popupTitle,
                () => onDone(entriesToAdd),
                buttonsRetriever: buttonsRetriever,
                scrollData: scrollData
            );

            List<InformationData> GetData() {
                infoList.Clear();
                pool = poolGet?.Invoke();

                List<T> availableEntries = GetAvailableEntries<T>(current, pool);
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

                return infoList;
            }
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
