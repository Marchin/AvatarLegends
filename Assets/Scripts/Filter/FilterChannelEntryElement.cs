using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

public enum FilterChannelState {
    None,
    Required,
    Excluded,
}

public class FilterChannelEntryData {
    public string Name;
    public FilterChannelState State;
    public Action<FilterChannelState> OnStateChange;

    public FilterChannelEntryData Clone() {
        FilterChannelEntryData newEntryData = new FilterChannelEntryData();
        newEntryData.Name = Name;
        newEntryData.State = State;
        newEntryData.OnStateChange = OnStateChange;

        return newEntryData;
    }
}

public class FilterChannelEntryElement : MonoBehaviour, IDataUIElement<FilterChannelEntryData> {
    [SerializeField] private Toggle _requiredToggle = default;
    [SerializeField] private Toggle _excludeToggle = default;
    [SerializeField] private TextMeshProUGUI _label = default;
    [SerializeField] private ScrollContent _scrollContent = default;
    private FilterChannelEntryData _entryData;
    public bool IsScrollContentOn {
        get => (_scrollContent != null) ? _scrollContent.enabled : false;
        set {
            if (_scrollContent != null) {
                _scrollContent.enabled = value;
            }
        }
    }

    private void Awake() {
        _requiredToggle.onValueChanged.AddListener(isOn => {
            if (_entryData == null) {
                return;
            }

            if (isOn) {
                _entryData.State = FilterChannelState.Required;
            } else if (_excludeToggle.isOn) {
                _entryData.State = FilterChannelState.Excluded;
            } else {
                _entryData.State = FilterChannelState.None;
            }

            _entryData.OnStateChange?.Invoke(_entryData.State);
        });
        _excludeToggle.onValueChanged.AddListener(isOn => {
            if (_entryData == null) {
                return;
            }

            if (isOn) {
                _entryData.State = FilterChannelState.Excluded;
            } else if (_requiredToggle.isOn) {
                _entryData.State = FilterChannelState.Required;
            } else {
                _entryData.State = FilterChannelState.None;
            }

            _entryData.OnStateChange?.Invoke(_entryData.State);
        });
    }

    public void Populate(FilterChannelEntryData data) {
        _entryData = data;
        _label.text = _entryData.Name;

        switch (data.State) {
            case FilterChannelState.None: {
                _excludeToggle.isOn = false;
                _requiredToggle.isOn = false;
            } break;
            
            case FilterChannelState.Required: {
                _excludeToggle.isOn = false;
                _requiredToggle.isOn = true;
            } break;
            
            case FilterChannelState.Excluded: {
                _excludeToggle.isOn = true;
                _requiredToggle.isOn = false;
            } break;

            default: {
                Debug.LogError($"{data.Name} - invalid filter state");
            } break;
        }
    }
}
