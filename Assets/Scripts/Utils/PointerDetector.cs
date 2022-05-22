using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointerDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public bool IsPointerIn { get; private set; }
    public event Action OnPointerEnterEvent;
    public event Action OnPointerExitEvent;

    public void OnPointerEnter(PointerEventData eventData) {
        IsPointerIn = true;
        OnPointerEnterEvent?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData) {
        IsPointerIn = false;
        OnPointerExitEvent?.Invoke();
    }
}
