using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonData {
    public string Text;
    public Action Callback;

    public ButtonData() {}
    
    public ButtonData(string text, Action callback) {
        Text = text;
        Callback = callback;
    }
}

public class ButtonElement : MonoBehaviour, IDataUIElement<ButtonData> {
    [SerializeField] private TextMeshProUGUI _text = default;
    [SerializeField] private Button _button = default;
    public void Invoke() => _button.onClick.Invoke();
    public Graphic ButtonImage => _button.targetGraphic;
    public string Text => _text.text;

    public void Populate(ButtonData data) {
        _text.text = data.Text;
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => data.Callback());
    }
}
