using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviourSingleton<TooltipManager> {
    [SerializeField] private RectTransform _tooltipContainer = default;
    [SerializeField] private CanvasScaler _canvasScaler = default;
    [SerializeField] private TextMeshProUGUI _tooltipMsg = default;
    [SerializeField] private float _tooltipPad = default;
    private readonly float _halfHeight = 0.5f * Screen.height;
    private const float _cursorHeight = 28f;

    private void Awake() {
        _tooltipContainer.anchorMin = _tooltipContainer.anchorMax = Vector2.zero;
        Hide();
    }

    public void ShowMessage(string msg) {
        _tooltipMsg.text = msg;
        _tooltipContainer.gameObject.SetActive(true);
        enabled = true;
    }

    public void Hide() {
        _tooltipContainer.gameObject.SetActive(false);
        enabled = false;
    }

    private void Update() {
        Vector2 pos = Input.mousePosition;
        Vector2 tooltipsHalfSize = _tooltipContainer.rect.size * _tooltipContainer.lossyScale * 0.5f;
        Vector2 offset = Vector3.up * (_tooltipPad + tooltipsHalfSize.y);

        if (pos.y <= _halfHeight) {
            pos += offset;
        } else {
            pos -= offset + (_cursorHeight * Vector2.up);
        }
        ;
        pos.x = Mathf.Clamp(pos.x, tooltipsHalfSize.x, Screen.width - tooltipsHalfSize.x);
        pos.y = Mathf.Clamp(pos.y, tooltipsHalfSize.y, Screen.height - tooltipsHalfSize.y);
        _tooltipContainer.position = pos;
    }
}
