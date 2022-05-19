using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class Filter {
    public List<FilterChannelData> Filters = new List<FilterChannelData>();
    public List<ToggleActionData> Toggles = new List<ToggleActionData>();

    public bool Active => (Toggles.Find(t => t.On) != null) || 
            (Filters.Find(f => f.Elements.Find(e => e.State != FilterChannelState.None) != null) != null);
}

public class FilterChannelData {
    public string Name;
    public List<FilterChannelEntryData> Elements;
    public FilterChannelEntryList List;
    private Func<IDataEntry, List<int>> _getFilteringComponent;

    public FilterChannelData(string name, Func<IDataEntry, List<int>> getFilteringComponent) {
        Name = name;
        _getFilteringComponent = getFilteringComponent;
    }

    public FilterChannelData Clone() {
        FilterChannelData newFilter = new FilterChannelData(Name, _getFilteringComponent);

        newFilter.List = this.List;
        newFilter.Elements = new List<FilterChannelEntryData>(Elements.Count);
        for (int iElement = 0; iElement < Elements.Count; ++iElement) {
            newFilter.Elements.Add(Elements[iElement].Clone());
        }

        return newFilter;
    }

    public List<T> Apply<T>(List<T> list) where T : IDataEntry {
        List<int> required = new List<int>(Elements.Count);
        List<int> excluded = new List<int>(Elements.Count);
        for (int iEntry = 0; iEntry < Elements.Count; ++iEntry) {
            int index = Elements.IndexOf(Elements[iEntry]);
            if (Elements[iEntry].State == FilterChannelState.Required) {
                required.Add(index);
            } else if (Elements[iEntry].State == FilterChannelState.Excluded) {
                excluded.Add(index);
            }
        }

        var filteredList = new List<T>(list.Count);
        for (int iElement = 0; iElement < list.Count; ++iElement) {
            List<int> filteringComponent = _getFilteringComponent?.Invoke(list[iElement]);

            bool add = true;
            for (int iRequired = 0; iRequired < required.Count; ++iRequired) {
                if (!filteringComponent.Contains(required[iRequired])) {
                    add = false;
                    break;
                }
            }

            if (!add) {
                continue;
            }
            
            for (int iExcluded = 0; iExcluded < excluded.Count; ++iExcluded) {
                if (filteringComponent.Contains(excluded[iExcluded])) {
                    add = false;
                    break;
                }
            }

            if (add) {
                filteredList.Add(list[iElement]);
            }
        }

        return filteredList;
    }
}

public class ToggleActionData : ToggleData {
    public Func<List<IDataEntry>, bool, List<IDataEntry>> _action;

    public ToggleActionData(string name, Func<List<IDataEntry>, bool, List<IDataEntry>> action) : base(name) {
        _action = action;
    }

    public override object Clone() {
        return this.MemberwiseClone();
    }

    public List<T> Apply<T>(List<T> list) where T : IDataEntry {
        List<IDataEntry> entries = new List<IDataEntry>(list.Count);
        for (int iElement = 0; iElement < list.Count; ++iElement) {
            entries.Add(list[iElement]);
        }
        var processedList = _action?.Invoke(entries, On);
        List<T> processedEntries = new List<T>(processedList.Count);
        for (int iElement = 0; iElement < processedList.Count; ++iElement) {
            processedEntries.Add((T)processedList[iElement]);
        }

        return processedEntries;
    }
}

public class FilterChannelElement : MonoBehaviour, IDataUIElement<FilterChannelData>, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private Button _button = default;
    [SerializeField] private TextMeshProUGUI _label = default;
    private FilterChannelData _filterData;

    private void Awake() {
        _button.onClick.AddListener(async () => {
            if (_filterData == null || _filterData.Elements == null || _filterData.List == null) {
                return;
            }

            if (_filterData.List.LastCaller == this && _filterData.List.gameObject.activeSelf) {
                _filterData.List.gameObject.SetActive(false);
                _filterData.List.ListBackground.SetActive(false);
            } else {
                RectTransform rectTransform = transform as RectTransform;
                RectTransform scrollRectTransform = _filterData.List.transform as RectTransform;
                
                _filterData.List.Populate(_filterData.Elements);
                _filterData.List.gameObject.SetActive(true);
                _filterData.List.ListBackground.SetActive(true);
                await UniTask.DelayFrame(1,
                    cancellationToken: this.GetCancellationTokenOnDestroy())
                    .SuppressCancellationThrow();
                _filterData.List.AdjustPosition(transform as RectTransform);
                _filterData.List.ResetScroll();
            }
            
            _filterData.List.LastCaller = this;
        });
    }
    
    public void Populate(FilterChannelData data) {
        _filterData = data;
        RefreshLabel();

        for (int iElement = 0; iElement < _filterData.Elements.Count; ++iElement) {
            _filterData.Elements[iElement].OnStateChange = _ => { RefreshLabel(); };
        }
    }

    public void RefreshLabel() {
        _label.text = _filterData.Name;
        
        if (_filterData != null && _filterData.Elements != null) {
            int activeFilters = 0;

            for (int iElement = 0; iElement < _filterData.Elements.Count; ++iElement) {
                if (_filterData.Elements[iElement].State != FilterChannelState.None) {
                    activeFilters++;
                }
            }

            if (activeFilters > 0) {
                _label.text += $" ({activeFilters})";
            }
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData) {
        if (_filterData.List.LastCaller == this) {
            _filterData.List.IsMouseOut = false;
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (_filterData.List.LastCaller == this) {
            _filterData.List.IsMouseOut = true;
        }
    }
}
