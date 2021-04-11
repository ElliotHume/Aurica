using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    public static Crosshair Instance;

    // Raycast will hit everything but spells
    public LayerMask WPLayermask;

    // private Vector3 startPos;
    void Start() {
        Instance = this;
    }

    void Update() {
        if (Input.GetButtonDown("Fire1")) {
            PressHitButtons();
        }
    }

    public Vector3 GetWorldPoint() {
        Ray ray = Camera.main.ScreenPointToRay( transform.position );
        RaycastHit hit;
        if( Physics.Raycast( ray, out hit, 1000f, WPLayermask) ) {
            // Debug.Log("Point hit: "+hit.point);
            return hit.point;
        }

        return Camera.main.transform.position + Camera.main.transform.forward * 100f;
    }

    public GameObject GetPlayerHit(float radius = 5f) {
        Ray ray = Camera.main.ScreenPointToRay( transform.position );
        RaycastHit hit;
        if( Physics.SphereCast( ray, radius, out hit, 1000f, 1 << 3) ) {
            // Debug.Log("Player hit: "+hit.collider.gameObject);
            return hit.collider.gameObject;
        }

        return null;
    }

    public void PressHitButtons() {
        PointerEventData touch = new PointerEventData(EventSystem.current);
        touch.position = Input.mousePosition;
        List<RaycastResult> hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(touch, hits);
        Button b;
        foreach( var hit in hits) {
            b = hit.gameObject.GetComponent<Button>();
            if (b != null) b.onClick.Invoke();
        }
    }
}
