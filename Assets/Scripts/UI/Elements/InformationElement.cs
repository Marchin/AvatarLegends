using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

public class InformationData {
    public Action<int> OnValueChange;
    public Action<bool> OnToggle;
    public Action OnMoreInfo;
    public Action OnDropdown;
    public Action OnAdd;
    public Action OnDelete;
    public Action OnEdit;
    public Action OnHoverIn;
    public Action OnHoverOut;
    public string Prefix;
    public string Content;
    public int IndentLevel;
    public int InitValue;
    public int MinValue;
    public int MaxValue;
    public bool LoopValue;
    public bool? IsToggleOn;
    public bool Expanded;
}

public class InformationElement : MonoBehaviour, IDataUIElement<InformationData> {
    [SerializeField] private int _indentWidth = default;
    [SerializeField] private GameObject _prefixContainer = default;
    [SerializeField] private TextMeshProUGUI _prefix = default;
    [SerializeField] private TextMeshProUGUI _content = default;
    [SerializeField] private TextMeshProUGUI _counter = default;
    [SerializeField] private Button _moreInfoButton = default;
    [SerializeField] private Button _onEditButton = default;
    [SerializeField] private Button _decreaseButton = default;
    [SerializeField] private Button _increaseButton = default;
    [SerializeField] private Button _dropdownButton = default;
    [SerializeField] private Button _addButton = default;
    [SerializeField] private Button _deleteButton = default;
    [SerializeField] private PointerDetector _onHover = default;
    [SerializeField] private Toggle _toggle = default;
    [SerializeField] private LayoutGroup _layoutGroup = default;
    [SerializeField] private ScrollContent _scrollContent = default;
    private InformationData _info;
    private int _counterValue;

    private void Start() {
        _decreaseButton.onClick.AddListener(() => {
            if (!_info.LoopValue && (_counterValue <= _info.MinValue)) {
                return;
            }
            
            --_counterValue;

            if (_counterValue < _info.MinValue) {
                _counterValue = _info.MaxValue;
            }

            _info.OnValueChange(_counterValue);
            RefreshCounter();
        });
        _increaseButton.onClick.AddListener(() => {
            if (!_info.LoopValue && (_counterValue >= _info.MaxValue)) {
                return;
            }

            ++_counterValue;

            if (_counterValue > _info.MaxValue) {
                _counterValue = _info.MinValue;
            }

            _info.OnValueChange(_counterValue);
            RefreshCounter();
        });
        _dropdownButton.onClick.AddListener(() => {
            _info.OnDropdown();
        });
        _deleteButton.onClick.AddListener(() => _info.OnDelete());
        _moreInfoButton.onClick.AddListener(() => _info.OnMoreInfo());
        _onEditButton.onClick.AddListener(() => _info.OnEdit());
        _addButton.onClick.AddListener(() => _info.OnAdd());
        _onHover.OnPointerEnterEvent += () => _info.OnHoverIn?.Invoke();
        _onHover.OnPointerExitEvent += () => _info.OnHoverOut?.Invoke();
    }

    private void RefreshCounter() {
        _decreaseButton.interactable = _info.LoopValue || (_counterValue > _info.MinValue);
        _increaseButton.interactable = _info.LoopValue || (_counterValue < _info.MaxValue);
        _counter.text = $"{_counterValue}/{_info.MaxValue}";
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

        _onEditButton.gameObject.SetActive(data.OnEdit != null);
        _moreInfoButton.gameObject.SetActive(data.OnMoreInfo != null);
        _addButton.gameObject.SetActive(data.OnAdd != null);
        _deleteButton.gameObject.SetActive(data.OnDelete != null);
        _dropdownButton.gameObject.SetActive(data.OnDropdown != null);
        _decreaseButton.gameObject.SetActive(data.OnValueChange != null);
        _increaseButton.gameObject.SetActive(data.OnValueChange != null);
        _counter.transform.parent.gameObject.SetActive(data.OnValueChange != null);
        _toggle.onValueChanged.RemoveAllListeners();
        _toggle.gameObject.SetActive((data.OnToggle != null) || data.IsToggleOn.HasValue);
        _toggle.isOn = data.IsToggleOn ?? false;
        _toggle.interactable = (data.OnToggle != null);
        _toggle.onValueChanged.AddListener(value => _info?.OnToggle?.Invoke(value));
        _onHover.gameObject.SetActive((data.OnHoverIn != null) || (data.OnHoverOut != null));
        _counterValue = data.InitValue;
        RefreshCounter();
    }
}
