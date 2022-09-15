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

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        bool playerInRange = false;
        foreach(var player in players) {
            if (Vector3.Distance(player.transform.position, transform.position) < radius) {
                playerInRange = true;
                break;
            }
        }

        //Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, layerMask);
        if (playerInRange && !hasActivated) {
            // Fire the event once when the trigger is within the radius
            OnRadiusEnter.Invoke();
            hasActivated = true;
        } else if (!playerInRange && hasActivated) {
            // Reset so that it can fire again
            hasActivated = false;
            OnRadiusExit.Invoke();
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
