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
        } else if (Input.GetKeyDown("b")) {
            CastSpell("Spell_ArcaneThrow");
        } else if (Input.GetKeyDown("v")) {
            CastSpell("Spell_Condense");
        } else if (Input.GetKeyDown("c")) {
            CastSpell("Spell_AngelWisp");
        } else if (Input.GetKeyDown("x")) {
            CastSpell("Spell_SoulStrike");
        } else if (Input.GetKeyDown("l")) {
            CastSpell("Spell_EarthBound");
        } else if (Input.GetKeyDown("j")) {
            CastSpell("Spell_EmberSphere");
        }
    }

    public void CastSpell(string prefabName) {
        PhotonNetwork.Instantiate(prefabName, transform.position+ transform.forward*2f, transform.rotation);
    }
}
