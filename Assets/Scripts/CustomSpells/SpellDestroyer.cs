using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpellDestroyer : MonoBehaviourPun
{
    void OnTriggerEnter(Collider other) {
        if (photonView.IsMine) {
            if (other.gameObject.tag == "Spell") {
                Spell sp = other.gameObject.GetComponent<Spell>();
                if (sp != null) {
                    PhotonView pv = PhotonView.Get(sp);
                    if (pv != null) pv.RPC("DestroySpell", RpcTarget.All);
                }
            } 
        }
    }
}
