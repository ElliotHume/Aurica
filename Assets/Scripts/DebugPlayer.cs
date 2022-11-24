using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DebugPlayer : MonoBehaviourPun
{
    public float movementSpeed = 1f;
    bool moveAround = false, movingForward = true;

    float distanceTravelled = 0f, distanceLimit = 10f;

    // Update is called once per frame
    void Update() {
        if (Input.GetKey(KeyCode.RightShift)) {
            if (Input.GetKeyDown("n")) {
                CastDebugSpell("Spell_BitingWind");
            } else if (Input.GetKeyDown("m")) {
                CastDebugSpell("XCollision_AngelWisp");
            } else if (Input.GetKeyDown("b")) {
                CastDebugSpell("Spell_ManaBall_Fire");
            } else if (Input.GetKeyDown("v")) {
                CastDebugSpell("Spell_CMB_Condense");
            } else if (Input.GetKeyDown("k")) {
                CastDebugSpell("Spell_Dissolution");
            } else if (Input.GetKeyDown("j")) {
                CastDebugSpell("Spell_ManaRealm_Chaos");
            }
        }

        if (Input.GetKeyDown(",")) {
            moveAround = !moveAround;
        } else if (Input.GetKeyDown("=")) {
            movementSpeed += 1f;
        } else if (Input.GetKeyDown("-")) {
            movementSpeed -= 1f;
        }

        if (Input.GetKeyDown("/")) {
            gameObject.SetActive(false);
        }

        if (moveAround) {
            if (movingForward) {
                transform.position += transform.forward * movementSpeed * Time.deltaTime;
                distanceTravelled += (transform.forward * movementSpeed * Time.deltaTime).magnitude;
                if (distanceTravelled > distanceLimit) {
                    movingForward = false;
                    distanceTravelled = 0f;
                }
            } else {
                transform.position -= transform.forward * movementSpeed *Time.deltaTime;
                distanceTravelled += (transform.forward * movementSpeed * Time.deltaTime).magnitude;
                if (distanceTravelled > distanceLimit) {
                    movingForward = true;
                    distanceTravelled = 0f;
                }
            }
        }
    }

    public void CastDebugSpell(string prefabName) {
        GameObject s = PhotonNetwork.Instantiate(prefabName, transform.position + transform.up + transform.forward + transform.forward, transform.rotation);
        BasicProjectileSpell bps = s.GetComponent<BasicProjectileSpell>();
        if (bps != null) {
            bps.AimAssistedProjectile = false;
            bps.TrackingProjectile = false;
            bps.PerfectHomingProjectile = false;
        }
    }

    // void OnCollisionEnter(Collision collision) {
    //     Debug.Log("DEBUG HIT BY: "+collision.gameObject);
    // }
}
