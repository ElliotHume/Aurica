using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUIElement : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler {

    public Canvas canvas;
    private RectTransform rectTransform;

    // Start is called before the first frame update
    void Start() {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData) {
        Debug.Log("Pointer Down");
    }

    public void OnBeginDrag(PointerEventData eventData) {
        Debug.Log("start drag");
    }

    public void OnEndDrag(PointerEventData eventData) {
        Debug.Log("end drag");
    }

    public void OnDrag(PointerEventData eventData) {
        Debug.Log("on drag");
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
}
