using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DropdownData {
    public string Text;
    public List<string> Options;
    public Action<int> Callback;

    public DropdownData() {}

    public DropdownData(string text, List<string> options, Action<int> callback) {
        Text = text;
        Options = options;
        Callback = callback;
    }
}

public class DropdownElement : MonoBehaviour, IDataUIElement<DropdownData> {
    [SerializeField] private TextMeshProUGUI _text = default;
    [SerializeField] private TMP_Dropdown _dropdown = default;
    public int Value {
        get => _dropdown.value;
        set => _dropdown.value = value;
    }
    public string SelectedOption => _dropdown.options[Value].text;

    public void Populate(DropdownData data) {
        _dropdown.ClearOptions();
        _dropdown.AddOptions(data.Options);

        _dropdown.onValueChanged.RemoveAllListeners();
        if (data.Callback != null) {
            _dropdown.onValueChanged.AddListener(index => data.Callback.Invoke(index));
        }

        if (_text != null && data.Text != null) {
            _text.text = data.Text;
        }
    }
}