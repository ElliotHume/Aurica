using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpellDestroyer : MonoBehaviourPun {
    public string NetworkEffectOnDestroy;

    void OnTriggerEnter(Collider other) {
        if (photonView.IsMine) {
            if (other.gameObject.tag == "Spell") {
                Spell sp = other.gameObject.GetComponent<Spell>();
                if (sp != null) {
                    PhotonView pv = PhotonView.Get(sp);
                    if (pv != null) pv.RPC("DestroySpell", RpcTarget.All);
                    if (NetworkEffectOnDestroy != "") PhotonNetwork.Instantiate(NetworkEffectOnDestroy, other.gameObject.transform.position, other.gameObject.transform.rotation);
                }
            } else if (other.gameObject.tag == "Shield") {
                ShieldSpell ss = other.gameObject.GetComponentInParent<ShieldSpell>();
                if (ss != null) {
                    PhotonView pv = PhotonView.Get(ss);
                    if (pv != null) pv.RPC("Dispel", RpcTarget.All);
                } else {
                    Debug.Log("Spell has hit a shield but cannot find ShieldSpell Component");
                }
            } 
        }
    }

    void OnTriggerStay(Collider other) {
        if (photonView.IsMine) {
            if (other.gameObject.tag == "Spell") {
                Spell sp = other.gameObject.GetComponent<Spell>();
                if (sp != null) {
                    PhotonView pv = PhotonView.Get(sp);
                    if (pv != null) pv.RPC("DestroySpell", RpcTarget.All);
                    if (NetworkEffectOnDestroy != "") PhotonNetwork.Instantiate(NetworkEffectOnDestroy, other.gameObject.transform.position, other.gameObject.transform.rotation);
                }
            } else if (other.gameObject.tag == "Shield") {
                ShieldSpell ss = other.gameObject.GetComponentInParent<ShieldSpell>();
                if (ss != null) {
                    PhotonView pv = PhotonView.Get(ss);
                    if (pv != null) pv.RPC("Dispel", RpcTarget.All);
                } else {
                    Debug.Log("Spell has hit a shield but cannot find ShieldSpell Component");
                }
            } 
        }
    }
}
