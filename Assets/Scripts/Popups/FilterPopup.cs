using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class FilterPopup : Popup {
    class PopupData {
        public Filter Filter;
        public Action<Filter> Callback;
    }

    [SerializeField] private Button _applyButton = default;
    [SerializeField] private Button _clearButton = default;
    [SerializeField] private Button _closeButton = default;
    [SerializeField] private FilterChannelDataList _filterList = default;
    [SerializeField] private ToggleList _toggleList = default;
    [SerializeField] private FilterChannelEntryList _filterEntryList = default;
    private Filter _filter;
    private Action<Filter> _callback;

    private void Awake() {
        _closeButton.onClick.AddListener(OnBackPressed);
        _applyButton.onClick.AddListener(() => {
            _callback?.Invoke(_filter);
            PopupManager.Instance.OverrideBack -= OnBackPressed;
            PopupManager.Instance.Back();
        });
        _clearButton.onClick.AddListener(() => {
            foreach (var filter in _filter.Filters) {
                foreach (var element in filter.Elements) {
                    element.State = FilterChannelState.None;
                }
            }
            _filterList.Populate(_filter.Filters);

            List<ToggleData> togglesData = new List<ToggleData>(_filter.Toggles.Count);
            foreach (var toggle in _filter.Toggles) {
                toggle.On = false;
                togglesData.Add(toggle);
            }
            _toggleList.Populate(togglesData);
            _filterEntryList.gameObject.SetActive(false);
        });
    }

    private void OnEnable() {
        PopupManager.Instance.OverrideBack += OnBackPressed;
        _filterEntryList.ListBackground.SetActive(false);
    }

    private void OnDisable() {
        PopupManager.Instance.OverrideBack -= OnBackPressed;
    }

    public void Populate(
        Filter filter,
        Action<Filter> callback
    ) {
        _callback = callback;

        _filter = new Filter();

        if (filter.Filters != null) {
            _filter.Filters = new List<FilterChannelData>(filter.Filters.Count);
            foreach (var filterData in filter.Filters) {
                filterData.List = _filterEntryList;
                _filter.Filters.Add(filterData.Clone());
            }
            _filterList.Populate(_filter.Filters);
        }

        _filterList.gameObject.SetActive(_filter.Filters?.Count > 0);

        if (filter.Toggles != null) {
            _filter.Toggles = new List<ToggleActionData>(filter.Toggles.Count);
            List<ToggleData> togglesData = new List<ToggleData>(_filter.Toggles.Count);
            foreach (var toggle in filter.Toggles) {
                var toggleData = toggle.Clone() as ToggleActionData;
                _filter.Toggles.Add(toggleData);
                togglesData.Add(toggleData);
            }
            _toggleList.Populate(togglesData);
        }

        _toggleList.gameObject.SetActive(_filter.Toggles?.Count > 0);

        _filterEntryList.gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    private void OnBackPressed() {
        if (_filterEntryList.gameObject.activeSelf) {
            _filterEntryList.gameObject.SetActive(false);
            _filterEntryList.ListBackground.SetActive(false);
        } else {
            PopupManager.Instance.OverrideBack -= OnBackPressed;
            PopupManager.Instance.Back();
        }
    }

    public override object GetRestorationData() {
        PopupData data = new PopupData {
            Filter = _filter,
            Callback = _callback
        };  

        return data;
    }

    public override void Restore(object data) {
        if (data is PopupData popupData) {
            Populate(popupData.Filter, popupData.Callback);
        }
    }
}
