using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimpointAnchor : MonoBehaviour {

    public static AimpointAnchor Instance;

    void Start() {
        AimpointAnchor.Instance = this;
    }

    public Vector3 GetHitNormal() {
        Collider coll = GetComponent<Collider>();

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.2f);
        if (hitColliders.Length == 0) return Vector3.zero;

        Collider otherObject = hitColliders[0];
        Vector3 direction;
        float distance;
        
        bool overlapped = Physics.ComputePenetration(
                coll, transform.position, transform.rotation,
                otherObject, otherObject.gameObject.transform.position, otherObject.gameObject.transform.rotation,
                out direction, out distance
            );

        // Debug.Log("HIT NORMAL: "+direction);
        return direction.normalized;
    }
}
