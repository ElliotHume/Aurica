using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RadiusEvent : MonoBehaviour
{
    public float radius = 3f;
    public bool onlyFiresOnce = false;
    public UnityEvent OnRadiusEnter, OnRadiusExit;
    public LayerMask layerMask;

    private bool hasActivated = false;

    void FixedUpdate() {
        if (onlyFiresOnce && hasActivated) return;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, layerMask);
        if (hitColliders.Length > 0 && !hasActivated) {
            Debug.Log("Fire event");
            // Fire the event once when the trigger is within the radius
            OnRadiusEnter.Invoke();
            hasActivated = true;
        } else if (hitColliders.Length == 0 && hasActivated) {
            Debug.Log("Reset event");
            // Reset so that it can fire again
            hasActivated = false;
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
