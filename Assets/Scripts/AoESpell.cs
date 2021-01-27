using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AoESpell : Spell
{
    public float LastingDamage = 0f;
    public bool OneShotEffect = true, LastingEffect = false;
    public float DestroyTimeDelay = 15f;
    public GameObject[] DeactivateObjectsAfterDuration;


    void Awake() {
        if (photonView.IsMine) {
            Invoke("DestroySelf", DestroyTimeDelay);
            Invoke("DisableCollisions", Duration);
        }
        Invoke("DisableParticlesAfterDuration", Duration);
    }

    void OnTriggerEnter(Collider other) {
        if (!OneShotEffect) return;
        if (!LastingEffect) GetComponent<Collider>().enabled = false;

        // TODO: Call local collision response to generate collision VFX
        // ContactPoint hit = collision.GetContact(0);
        // LocalCollisionBehaviour(hit.point, hit.normal);

        if (photonView.IsMine) {
            if (other.gameObject.tag == "Player") {
                PlayerManager pm = other.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    PhotonView pv = PhotonView.Get(pm);
                    if (pv != null) pv.RPC("OnSpellCollide", RpcTarget.All, Damage * GetSpellStrength(), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson());
                }
            }
        }
    }

    void OnTriggerStay(Collider other) {
        if (!LastingEffect) return;
        
        // TODO: Call local collision response to generate collision VFX
        // ContactPoint hit = collision.GetContact(0);
        // LocalCollisionBehaviour(hit.point, hit.normal);

        if (photonView.IsMine) {
            if (other.gameObject.tag == "Player") {
                PlayerManager pm = other.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    PhotonView pv = PhotonView.Get(pm);
                    if (pv != null) pv.RPC("OnSpellCollide", RpcTarget.All, LastingDamage * 0.002f * GetSpellStrength(), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson());
                }
            }
        }
    }

    void DestroySelf() {
        PhotonNetwork.Destroy(gameObject);
    }

    void DisableCollisions() {
        GetComponent<Collider>().enabled = false;
    }

    void DisableParticlesAfterDuration() {
        foreach (var effect in DeactivateObjectsAfterDuration) {
            if (effect != null) effect.SetActive(false);
        }
    }
}
