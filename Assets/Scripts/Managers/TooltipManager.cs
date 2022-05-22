using TMPro;
using UnityEngine;

public class TooltipManager : MonoBehaviourSingleton<TooltipManager> {
    [SerializeField] private RectTransform _tooltipContainer = default;
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
        Vector2 offset = Vector3.up * (_tooltipPad + (_tooltipContainer.rect.height / 2f));

        if (pos.y <= _halfHeight) {
            pos += offset;
        } else {
            pos -= offset + (_cursorHeight * Vector2.up);
        }
        
        float halfWidth = _tooltipContainer.rect.width * 0.5f;
        float halfHeight = _tooltipContainer.rect.height * 0.5f;
        pos.x = Mathf.Clamp(pos.x, halfWidth, Screen.width - halfWidth);
        pos.y = Mathf.Clamp(pos.y, halfHeight, Screen.height - halfHeight);
        _tooltipContainer.position = pos;
    }
}
