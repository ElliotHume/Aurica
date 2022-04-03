using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Teleporter : MonoBehaviour {
    public Transform anchor;
    public bool isKillingPlane = false;
    public string NetworkEffectOnTeleport;

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player" && other.gameObject == PlayerManager.LocalPlayerGameObject) {
            PlayerManager pm = other.gameObject.GetComponent<PlayerManager>();
            if (anchor != null) pm.Teleport(anchor);

            if (isKillingPlane) pm.TakeDamage(100000f, new ManaDistribution());
            if (anchor != null && NetworkEffectOnTeleport != "") PhotonNetwork.Instantiate(NetworkEffectOnTeleport, anchor.position, anchor.rotation);
        }
    }
}
