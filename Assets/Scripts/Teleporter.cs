using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour {
    public Transform anchor;
    public bool isKillingPlane = false;

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player" && other.gameObject == PlayerManager.LocalPlayerInstance) {
            PlayerManager pm = other.gameObject.GetComponent<PlayerManager>();
            pm.Teleport(anchor.position);

            if (isKillingPlane) pm.TakeDamage(100000f, new ManaDistribution());
        }
    }
}
