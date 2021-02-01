using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class StatusEffect : MonoBehaviourPun {

    // Increase or decrease movement speed
    public bool slow;
    public float slowDuration, slowPercentage = 0f;
    public bool hasten;
    public float hastenDuration, hastenPercentage = 0f;

    // Prevent all movement, including movement spells
    public bool root;
    public float rootDuration;

    // Prevent all spellcasts
    public bool silence;
    public float silenceDuration;

    // Prevent all actions
    public bool stun;
    public float stunDuration;

    // Increase or decrease the amount of damage dealt by given mana types
    public bool weaken;
    public float weakenDuration;
    public ManaDistribution weakenDistribution;
    public bool strengthen;
    public float strengthenDuration;
    public ManaDistribution strengthenDistribution;

    // Increase or decrease the amount of damage taken
    public bool fragile;
    public float fragileDuration, fragilePercentage = 0f;
    public bool tough;
    public float toughDuration, toughPercentage = 0f;

    public bool healing = false;
    public float healFlatAmount = 0f, healPercentAmount = 0f;

    public bool isContinuous = false;
    public bool canHitSelf = false;

    private bool isCollided = false;

    public void ManualActivation(GameObject playerGO) {
        if (!photonView.IsMine) return;

        PlayerManager pm = playerGO.GetComponent<PlayerManager>();
        if (pm != null) {
            PhotonView pv = PhotonView.Get(pm);
            Activate(pv);
        }
    }


    void OnCollisionEnter(Collision collision) {
        if (photonView.IsMine && !isCollided) {
            if (collision.gameObject.tag == "Player" && (collision.gameObject != PlayerManager.LocalPlayerInstance || canHitSelf)) {
                PlayerManager pm = collision.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    PhotonView pv = PhotonView.Get(pm);
                    Activate(pv);
                }
            }
        }
    }

    void OnTriggerEnter(Collider collision) {
        if (photonView.IsMine) {
            if (collision.gameObject.tag == "Player") {
                PlayerManager pm = collision.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    PhotonView pv = PhotonView.Get(pm);
                    Activate(pv);
                }
            }
        }
    }

    void OnTriggerStay(Collider collision) {
        if (photonView.IsMine && isContinuous) {
            if (collision.gameObject.tag == "Player") {
                PlayerManager pm = collision.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    PhotonView pv = PhotonView.Get(pm);
                    Activate(pv);
                }
            }
        }
    }

    void Activate(PhotonView pv) {
        if (pv != null) {
            if (slow) pv.RPC("Slow", RpcTarget.All, slowDuration, slowPercentage/100f);
            if (hasten) pv.RPC("Hasten", RpcTarget.All, hastenDuration, hastenPercentage/100f);
            if (root) pv.RPC("Root", RpcTarget.All, rootDuration);
            if (silence) pv.RPC("Silence", RpcTarget.All, silenceDuration);
            if (stun) pv.RPC("Stun", RpcTarget.All, stunDuration);
            if (weaken) pv.RPC("Weaken", RpcTarget.All, weakenDuration, weakenDistribution.ToString());
            if (strengthen) pv.RPC("Strengthen", RpcTarget.All, strengthenDuration, strengthenDistribution.ToString());
            if (fragile) pv.RPC("Fragile", RpcTarget.All, fragileDuration, fragilePercentage/100f);
            if (tough) pv.RPC("Tough", RpcTarget.All, toughDuration, toughPercentage/100f);
            if (healing) pv.RPC("Heal", RpcTarget.All, healFlatAmount, healPercentAmount/100f);
        }
    }
}
