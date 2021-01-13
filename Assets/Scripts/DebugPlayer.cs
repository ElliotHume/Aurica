using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DebugPlayer : MonoBehaviourPun
{

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown("n")) {
            CastSpell("Spell_Fireball");
        } else if (Input.GetKeyDown("m")) {
            CastSpell("Spell_ShadeSmoke");
        }
    }

    public void CastSpell(string prefabName) {
        PhotonNetwork.Instantiate(prefabName, transform.position+ transform.forward*2f, transform.rotation);
    }
}
