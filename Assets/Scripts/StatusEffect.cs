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

    // TODO: Lower or Raise Damage/Health of spells
    // public bool weaken;
    // public float weakenDuration, weakenPercentage = 0f;
    // public bool strengthen;
    // public float strengthenDuration, strengthenPercentage = 0f;

    // Increase or decrease the amount of damage taken
    public bool fragile;
    public float fragileDuration, fragilePercentage = 0f;
    public bool tough;
    public float toughDuration, toughPercentage = 0f;

    public bool isContinuous = false;

    private bool isCollided = false;


    void OnCollisionEnter(Collision collision) {
        if (photonView.IsMine && !isCollided) {
            if (collision.gameObject.tag == "Player" && collision.gameObject != PlayerManager.LocalPlayerInstance) {
                PlayerManager pm = collision.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    PhotonView pv = PhotonView.Get(pm);
                    if (pv != null) {
                        if (slow) pv.RPC("Slow", RpcTarget.All, slowDuration, slowPercentage);
                        if (hasten) pv.RPC("Hasten", RpcTarget.All, hastenDuration, hastenPercentage);
                        if (root) pv.RPC("Root", RpcTarget.All, rootDuration);
                        if (silence) pv.RPC("Silence", RpcTarget.All, silenceDuration);
                        if (stun) pv.RPC("Stun", RpcTarget.All, stunDuration);
                        // if (weaken) pv.RPC("Weaken", RpcTarget.All, weakenDuration, weakenPercentage);
                        // if (strengthen) pv.RPC("Strengthen", RpcTarget.All, strengthenDuration, strengthenPercentage);
                        if (fragile) pv.RPC("Fragile", RpcTarget.All, fragileDuration, fragilePercentage);
                        if (tough) pv.RPC("Tough", RpcTarget.All, toughDuration, toughPercentage);
                    }
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
                    if (pv != null) {
                        if (slow) pv.RPC("Slow", RpcTarget.All, slowDuration, slowPercentage);
                        if (hasten) pv.RPC("Hasten", RpcTarget.All, hastenDuration, hastenPercentage);
                        if (root) pv.RPC("Root", RpcTarget.All, rootDuration);
                        if (silence) pv.RPC("Silence", RpcTarget.All, silenceDuration);
                        if (stun) pv.RPC("Stun", RpcTarget.All, stunDuration);
                        // if (weaken) pv.RPC("Weaken", RpcTarget.All, weakenDuration, weakenPercentage);
                        // if (strengthen) pv.RPC("Strengthen", RpcTarget.All, strengthenDuration, strengthenPercentage);
                        if (fragile) pv.RPC("Fragile", RpcTarget.All, fragileDuration, fragilePercentage);
                        if (tough) pv.RPC("Tough", RpcTarget.All, toughDuration, toughPercentage);
                    }
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
                    if (pv != null) {
                        if (slow) pv.RPC("Slow", RpcTarget.All, slowDuration, slowPercentage);
                        if (hasten) pv.RPC("Hasten", RpcTarget.All, hastenDuration, hastenPercentage);
                        if (root) pv.RPC("Root", RpcTarget.All, rootDuration);
                        if (silence) pv.RPC("Silence", RpcTarget.All, silenceDuration);
                        if (stun) pv.RPC("Stun", RpcTarget.All, stunDuration);
                        // if (weaken) pv.RPC("Weaken", RpcTarget.All, weakenDuration, weakenPercentage);
                        // if (strengthen) pv.RPC("Strengthen", RpcTarget.All, strengthenDuration, strengthenPercentage);
                        if (fragile) pv.RPC("Fragile", RpcTarget.All, fragileDuration, fragilePercentage);
                        if (tough) pv.RPC("Tough", RpcTarget.All, toughDuration, toughPercentage);
                    }
                }
            }
        }
    }
}
