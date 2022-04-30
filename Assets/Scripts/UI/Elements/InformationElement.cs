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

    private void Awake() {
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
    }

    private void RefreshCounter() {
        _decreaseButton.interactable = _counterValue > 0;
        _increaseButton.interactable = _counterValue < _info.MaxValue;
        _counter.text = _counterValue.ToString();
    }

    public void Populate(InformationData data) {
        _info = data;

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
        _counterValue = data.InitValue;
        RefreshCounter();


        _moreInfoButton.onClick.RemoveAllListeners();
        _moreInfoButton.onClick.AddListener(() => data.OnMoreInfo?.Invoke());
        _moreInfoButton.gameObject.SetActive(data.OnMoreInfo != null);

        _addButton.gameObject.SetActive(data.OnAdd != null);
    }
}
