using UnityEngine;
using UnityEngine.UI;

public class AdjustLayoutToRect : MonoBehaviour {
    public enum Axis {
        Horizontal,
        Vertical
    }

    [SerializeField] private LayoutElement _layout = default;
    [SerializeField] private RectTransform _targetRect = default;
    [SerializeField] private Axis _axis = default;

    private void Awake() {
        RectTransform rect = (transform as RectTransform);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
    }

    private void Update() {
        switch (_axis) {
            case Axis.Horizontal: {
                if (_layout.minWidth != _targetRect.sizeDelta.x) {
                    _layout.minWidth = _targetRect.sizeDelta.x;
                }
            } break;
            case Axis.Vertical: {
                if (_layout.minHeight != _targetRect.sizeDelta.y) {
                    _layout.minHeight = _targetRect.sizeDelta.y;
                }
            } break;
        }
    }
}
