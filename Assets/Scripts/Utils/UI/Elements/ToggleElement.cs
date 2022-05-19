using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ToggleData : ICloneable {
    public string Name;
    public bool On;
    public Action<bool> OnValueChanged;

    public ToggleData() {}
    public ToggleData(string name, bool on = false, Action<bool> onValueChanged = null) {
        Name = name;
        On = on;
        OnValueChanged = onValueChanged;
    }

    public virtual object Clone() {
        return this.MemberwiseClone();
    }
}

public class ToggleElement : MonoBehaviour, IDataUIElement<ToggleData> {
    [SerializeField] private TextMeshProUGUI _text = default;
    [SerializeField] private Toggle _toggle = default;
    private ToggleData _toggleData;
    
    public void Populate(ToggleData data) {
        _toggle.onValueChanged.RemoveAllListeners();

        _toggleData = data;
        _text.text = data.Name;
        _toggle.isOn = data.On;
        
        _toggle.onValueChanged.AddListener(on => {
            _toggleData.On = on;
            _toggleData.OnValueChanged?.Invoke(on);
        });
    }
}