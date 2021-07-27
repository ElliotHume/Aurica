using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DebugPlayer : MonoBehaviourPun
{

    // Update is called once per frame
    void Update() {
        if (Input.GetKey(KeyCode.RightShift)) {
            if (Input.GetKeyDown("n")) {
                CastDebugSpell("Spell_ManaBall_Fire");
            } else if (Input.GetKeyDown("m")) {
                CastDebugSpell("Spell_ShadeSmoke");
            } else if (Input.GetKeyDown("b")) {
                CastDebugSpell("Spell_AuricBolt");
            } else if (Input.GetKeyDown("v")) {
                CastDebugSpell("Spell_Condense");
            } else if (Input.GetKeyDown("l")) {
                CastDebugSpell("Spell_EarthBound");
            } else if (Input.GetKeyDown("j")) {
                CastDebugSpell("Spell_ManaRealm_Chaos");
            }
        }

        if (Input.GetKeyDown("/")) {
            gameObject.SetActive(false);
        }
        if (Input.GetKeyDown(".")) {
            GetComponent<LoadoutObject>().BindLoadout();
        }
    }

    public GameObject CastDebugSpell(string prefabName) {
        return PhotonNetwork.Instantiate(prefabName, transform.position+ transform.forward*2f, transform.rotation);
    }
}
