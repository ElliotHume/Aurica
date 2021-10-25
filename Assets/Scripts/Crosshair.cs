using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    public static Crosshair Instance;
    public Vector3 playerHitOffset = new Vector3(0,1f,0);

    // Raycast will hit everything but spells
    public LayerMask WorldPointLayermask, PlayerVisibleLayermask;

    public Image HitMarker;
    public float HitMarkerFadeMultiplier = 1f;

    bool hitMarkerVisible = false;

    // private Vector3 startPos;
    void Start() {
        Instance = this;
    }

    void Update() {
        if (Input.GetButtonDown("Fire1") && Cursor.lockState == CursorLockMode.Locked) {
            PressHitButtons();
        }

        if (hitMarkerVisible) {
            HitMarker.color = new Color(HitMarker.color.r, HitMarker.color.g, HitMarker.color.b, HitMarker.color.a - (Time.deltaTime * HitMarkerFadeMultiplier));
        }
        if (HitMarker.color.a <= 0f) {
            HitMarker.color = new Color(HitMarker.color.r, HitMarker.color.g, HitMarker.color.b, 0f);
            hitMarkerVisible = false;
        }
    }

    public Vector3 GetWorldPoint() {
        Ray ray = Camera.main.ScreenPointToRay( transform.position );
        RaycastHit hit;
        if( Physics.Raycast( ray, out hit, 1000f, WorldPointLayermask) ) {
            // Debug.Log("Point hit: "+hit.point);
            return hit.point;
        }

        return Camera.main.transform.position + Camera.main.transform.forward * 100f;
    }

    public GameObject GetPlayerHit(float radius = 5f) {
        Ray ray = Camera.main.ScreenPointToRay( transform.position );
        RaycastHit[] hits = Physics.SphereCastAll( ray, radius, 1000f, 1 << 3);
        foreach(var hit in hits) {
            Vector3 cameraPos = Camera.main.transform.position;
            Vector3 hitPos = hit.collider.gameObject.transform.position+playerHitOffset;
            bool isVisibilityBlocked = Physics.Raycast(cameraPos, hitPos-cameraPos, (hitPos-cameraPos).magnitude, PlayerVisibleLayermask);
            if (!isVisibilityBlocked && hit.collider.gameObject != PlayerManager.LocalPlayerGameObject) return hit.collider.gameObject;
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

    public void FlashHitMarker(bool majorDamage) {
        HitMarker.color = new Color(HitMarker.color.r, HitMarker.color.g, HitMarker.color.b, majorDamage ? 1f : 0.33f);
        hitMarkerVisible = true;
    }

    void OnDrawGizmosSelected(){
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach( var player in players) {
            Vector3 cameraPos = Camera.main.transform.position;
            Vector3 hitPos = player.transform.position+playerHitOffset;
            bool isVisibilityBlocked = Physics.Raycast(cameraPos, hitPos-cameraPos, (hitPos-cameraPos).magnitude, PlayerVisibleLayermask);
            Gizmos.color = isVisibilityBlocked? Color.red : Color.green;
            Gizmos.DrawLine(cameraPos, hitPos);
        }
	}
}
