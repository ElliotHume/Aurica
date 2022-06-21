using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class SpellLoadoutDraggable :MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler {

    public string BindKeyID;
    public bool CloudLoadoutSlot;

    public Canvas canvas;
    private Image ring;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private string spellText;
    private Vector3 startPosition;

    // Start is called before the first frame update
    void Start() {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        startPosition = rectTransform.anchoredPosition;
        ring = GetComponent<Image>();
        canvasGroup.alpha = 0f;
    }

    public void SetSpell(string spellString) {
        spellText = spellString;
    }

    public string GetSpellText() {
        return spellText;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        canvasGroup.blocksRaycasts = false;
        RetrieveSpellText();
        canvasGroup.alpha = 0.6f;
    }

    public void OnEndDrag(PointerEventData eventData) {
        canvasGroup.blocksRaycasts = true;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        foreach(RaycastResult result in raycastResults) {
            // Debug.Log("RayCastHit "+result.gameObject);
            SpellLoadoutDropSlot s = result.gameObject.GetComponent<SpellLoadoutDropSlot>();
            if (s != null) {
                s.OnSpellDrop(spellText);
            }
        }
        rectTransform.anchoredPosition = startPosition;
        canvasGroup.alpha = 0f;
    }

    public void OnDrag(PointerEventData eventData) {
        rectTransform.anchoredPosition += eventData.delta;
    }

    public void RetrieveSpellText() {
        if (CloudLoadoutSlot) {
            spellText = CloudLoadoutManager.Instance.GetLoadoutKey(BindKeyID);
        } else {
            spellText = AuricaCaster.LocalCaster.GetCachedSpellText(BindKeyID);
        }
    }
}
