using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class InformationData {
    public Action<int> OnValueChange;
    public Action<bool> OnToggle;
    public Action OnMoreInfo;
    public Action OnDropdown;
    public Action OnAdd;
    public string Prefix;
    public string Content;
    public int IndentLevel;
    public int InitValue;
    public int MaxValue;
    public bool IsToggleOn;
    public bool Expanded;
}

public class InformationElement : MonoBehaviour, IDataUIElement<InformationData> {
    [SerializeField] private int _indentWidth = default;
    [SerializeField] private GameObject _prefixContainer = default;
    [SerializeField] private TextMeshProUGUI _prefix = default;
    [SerializeField] private TextMeshProUGUI _content = default;
    [SerializeField] private TextMeshProUGUI _counter = default;
    [SerializeField] private Button _moreInfoButton = default;
    [SerializeField] private Button _decreaseButton = default;
    [SerializeField] private Button _increaseButton = default;
    [SerializeField] private Button _dropdownButton = default;
    [SerializeField] private Button _addButton = default;
    [SerializeField] private Toggle _toggle = default;
    [SerializeField] private LayoutGroup _layoutGroup = default;
    [SerializeField] private ScrollContent _scrollContent = default;
    private InformationData _info;
    private int _counterValue;

    private void Start() {
        _decreaseButton.onClick.AddListener(() => {
            if (_counterValue > 0) {
                --_counterValue;
                _info.OnValueChange(_counterValue);
                RefreshCounter();
            }
        });
        _increaseButton.onClick.AddListener(() => {
            if (_counterValue < _info.MaxValue) {
                ++_counterValue;
                _info.OnValueChange(_counterValue);
                RefreshCounter();
            }
        });
        _dropdownButton.onClick.AddListener(() => {
            _info.OnDropdown();
        });
        _moreInfoButton.onClick.AddListener(() => _info.OnMoreInfo());
        _addButton.onClick.AddListener(() => _info.OnAdd());
        _toggle.onValueChanged.AddListener(value => _info?.OnToggle?.Invoke(value));
    }

    private void RefreshCounter() {
        _decreaseButton.interactable = _counterValue > 0;
        _increaseButton.interactable = _counterValue < _info.MaxValue;
        _counter.text = _counterValue.ToString();
    }

    public void Populate(InformationData data) {
        _info = data;

        _dropdownButton.transform.localScale = data.Expanded ?
            new Vector3(1, -1, 1) :
            Vector3.one;

        if (!string.IsNullOrEmpty(data.Prefix)) {
            if (_prefix != null) {
                _prefix.gameObject.SetActive(true);
                _prefix.text = $"{data.Prefix}:";
                _content.text = data.Content;
            } else {
                _content.text = $"{data.Prefix}: {data.Content}";
            }
        } else {
            if (_prefix != null) {
                _prefix.gameObject.SetActive(false);
            }
            _content.text = data.Content;
        }

        if (_prefixContainer != null) {
            _prefixContainer.SetActive(!string.IsNullOrEmpty(data.Prefix));
        }

        if (_scrollContent != null) {
            _scrollContent.Refresh();
        }

        _layoutGroup.padding.left = (data.IndentLevel * _indentWidth);

        _dropdownButton.gameObject.SetActive(data.OnDropdown != null);
        _decreaseButton.gameObject.SetActive(data.OnValueChange != null);
        _increaseButton.gameObject.SetActive(data.OnValueChange != null);
        _counter.gameObject.SetActive(data.OnValueChange != null);
        _toggle.gameObject.SetActive(data.OnToggle != null);
        _toggle.isOn = data.IsToggleOn;
        _counterValue = data.InitValue;
        RefreshCounter();

        _moreInfoButton.gameObject.SetActive(data.OnMoreInfo != null);

        _addButton.gameObject.SetActive(data.OnAdd != null);
    }
}
