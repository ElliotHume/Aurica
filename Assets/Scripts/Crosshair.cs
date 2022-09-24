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

    [Tooltip("AoE Targeting Indicator")]
    [SerializeField]
    public GameObject TargetingIndicatorPrefab;

    bool hitMarkerVisible = false, targetingIndicatorActive = false, targetingIndicatorUseNormals = true;
    bool isSelfTargeted = false, isOpponentTargeted = false;
    GameObject currentTargetingIndicator;
    Vector3 targetingIndicatorScale = Vector3.one, positionOffset = Vector3.zero;
    AimpointAnchor aimPointAnchor;
    GameObject playerGO, targetObject;

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

        if (targetingIndicatorActive) {
            if (currentTargetingIndicator == null) {
                currentTargetingIndicator = Instantiate(TargetingIndicatorPrefab, GetWorldPoint(), Quaternion.identity);
                currentTargetingIndicator.transform.localScale = targetingIndicatorScale;
            }
            if (aimPointAnchor == null) {
                aimPointAnchor = AimpointAnchor.Instance;
            }
            if (playerGO == null) {
                playerGO = PlayerManager.LocalPlayerGameObject;
            }

            if (isSelfTargeted) {
                currentTargetingIndicator.transform.position = playerGO.transform.position + (playerGO.transform.forward * positionOffset.z + playerGO.transform.right * positionOffset.x + playerGO.transform.up * positionOffset.y);
                currentTargetingIndicator.transform.rotation = Quaternion.LookRotation(playerGO.transform.forward);
            } else if (isOpponentTargeted) {
                targetObject = GetPlayerHit(2f);
                if (targetObject == null && currentTargetingIndicator.activeInHierarchy) currentTargetingIndicator.SetActive(false);
                if (targetObject != null && !currentTargetingIndicator.activeInHierarchy) currentTargetingIndicator.SetActive(true);
                if (targetObject != null) currentTargetingIndicator.transform.position = targetObject.transform.position + positionOffset;
            } else {
                currentTargetingIndicator.transform.position = GetWorldPoint();
                aimPointAnchor.gameObject.transform.position = GetWorldPoint();
                if (targetingIndicatorUseNormals) {
                    currentTargetingIndicator.transform.rotation = Quaternion.LookRotation(aimPointAnchor.GetHitNormal(), (aimPointAnchor.gameObject.transform.position - playerGO.transform.position));
                } else {
                    currentTargetingIndicator.transform.rotation = Quaternion.Euler(0, playerGO.transform.rotation.eulerAngles.y, 0);
                }
            }
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
            if (hit.collider.gameObject == PlayerManager.LocalPlayerGameObject) continue;
            Vector3 cameraPos = Camera.main.transform.position;
            Vector3 hitPos = hit.collider.gameObject.transform.position+(playerHitOffset * hit.collider.gameObject.transform.localScale.y);
            float angle = Vector3.Angle(Camera.main.transform.forward, hitPos - cameraPos);
            // Debug.Log("Character hit: "+hit.collider.gameObject+"     angle: "+angle+"    distance: "+Vector3.Distance(hitPos, cameraPos));
            if (Mathf.Abs(angle) <= 45f && Vector3.Distance(hitPos, cameraPos) > 4f) {
                bool isVisibilityBlocked = Physics.Raycast(cameraPos, hitPos-cameraPos, (hitPos-cameraPos).magnitude, PlayerVisibleLayermask);
                if (!isVisibilityBlocked && hit.collider.gameObject != PlayerManager.LocalPlayerGameObject) return hit.collider.gameObject;
            }   
            
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

    public void ActivateTargetingIndicator(Vector3 scale, bool useNormals, bool targetSelf, bool targetOpponent, Vector3 offsetPosition) {
        targetingIndicatorActive = true;
        targetingIndicatorScale = scale;
        targetingIndicatorUseNormals = useNormals;
        isSelfTargeted = targetSelf;
        isOpponentTargeted = targetOpponent;
        positionOffset = offsetPosition;
    }

    public void DeactivateTargetingIndicator(){
        if (!targetingIndicatorActive && currentTargetingIndicator == null) return;
        if (currentTargetingIndicator != null) Destroy(currentTargetingIndicator);
        targetingIndicatorActive = false;
        targetingIndicatorScale = Vector3.one;
    }

    void OnDrawGizmosSelected(){
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach( var player in players) {
            Vector3 cameraPos = Camera.main.transform.position;
            Vector3 hitPos = player.transform.position+(playerHitOffset * player.transform.localScale.y);
            bool isVisibilityBlocked = Physics.Raycast(cameraPos, hitPos-cameraPos, (hitPos-cameraPos).magnitude, PlayerVisibleLayermask);
            Gizmos.color = isVisibilityBlocked? Color.red : Color.green;
            Gizmos.DrawLine(cameraPos, hitPos);
        }
	}
}
