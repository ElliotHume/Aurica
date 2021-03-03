using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public static Crosshair Instance;

    // Raycast will hit everything but spells
    int layermask = ~(1<<6);

    // private Vector3 startPos;
    void Start() {
        Instance = this;
    }

    public Vector3 GetWorldPoint() {
        Ray ray = Camera.main.ScreenPointToRay( transform.position );
        RaycastHit hit;
        if( Physics.Raycast( ray, out hit, 1000f, layermask) ) {
            // Debug.Log("Point hit: "+hit.point);
            return hit.point;
        }

        return transform.forward * 100f;
    }

    public GameObject GetPlayerHit(float radius = 5f) {
        Ray ray = Camera.main.ScreenPointToRay( transform.position );
        RaycastHit hit;
        if( Physics.SphereCast( ray, radius, out hit, 1000f, 1 << 3) ) {
            Debug.Log("Player hit: "+hit.collider.gameObject);
            return hit.collider.gameObject;
        }

        return null;
    }
}
