using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ShieldSpell : Spell, IPunObservable {

    public float Health;
    public List<GameObject> spawnEffectsOnBreak;
    public List<string> networkedEffectsOnBreak;
    public List<string> networkedEffectsOnHit;
    public ManaDistribution ShieldDistribution;
    public StatusEffect statusEffectOnBreak;
    public MovementEffect movementEffectOnBreak;

    bool broken = false;

    void Start() {
        Health *= GameManager.GLOBAL_SHIELD_HEALTH_MULTIPLIER;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(Health);
        } else {
            this.Health = (float)stream.ReceiveNext();
        }
    }

    public void ChannelMana(float time, ManaDistribution channeledDistribution) {
        if (photonView.IsMine) {
            Health += ShieldDistribution.GetDamage(time, channeledDistribution);
        }
    }

    [PunRPC]
    public void TakeDamage(float damage, string damageDistributionJson) {
        if (!photonView.IsMine) return;
        ManaDistribution damageDistribution = JsonUtility.FromJson<ManaDistribution>(damageDistributionJson);
        // Invert the shield distribution for damage calc
        // this means a shield dist [0,0,1,1,1,1,0] will completely nullify all elemental damage
        Health -= ShieldDistribution.GetInverted().GetDamage(damage, damageDistribution);
        // Debug.Log("Health: " + Health);

        if (damage > 1f) {
            foreach (string effect in networkedEffectsOnHit) {
                PhotonNetwork.Instantiate(effect, transform.position + transform.up, transform.rotation);
            }
        }
    }

    [PunRPC]
    public void Dispel() {
        if (!photonView.IsMine || broken) return;
        broken = true;
        PlayerManager owner = PlayerManager.LocalPlayerGameObject.GetComponent<PlayerManager>();
        if (owner != null) {
            PhotonView pv = PhotonView.Get(owner);
            if (pv != null) pv.RPC("BreakShield", RpcTarget.All);
        }
        PhotonNetwork.Destroy(gameObject);
    }

    public void Break() {
        if (broken) return;
        broken = true;

        foreach (GameObject effect in spawnEffectsOnBreak) {
            GameObject newEffect = Instantiate(effect, transform.position, transform.rotation);
            Destroy(newEffect, Duration);
        }
        if (photonView.IsMine) {
            foreach (string effect in networkedEffectsOnBreak) {
                GameObject instance = PhotonNetwork.Instantiate(effect, transform.position + transform.up, transform.rotation);
                Spell instanceSpell = instance.GetComponent<Spell>();
                if (instanceSpell != null) {
                    instanceSpell.SetSpellStrength(GetSpellStrength());
                    instanceSpell.SetSpellDamageModifier(GetSpellDamageModifier());
                    instanceSpell.SetOwner(GetOwner());
                }
            }
            PlayerManager owner = PlayerManager.LocalPlayerGameObject.GetComponent<PlayerManager>();
            if (owner != null) {
                PhotonView pv = PhotonView.Get(owner);
                if (pv != null) pv.RPC("BreakShield", RpcTarget.All);
                if (statusEffectOnBreak != null) statusEffectOnBreak.ManualActivation(PlayerManager.LocalPlayerGameObject);
                if (movementEffectOnBreak != null) movementEffectOnBreak.ManualActivation(PlayerManager.LocalPlayerGameObject);
            }

            PhotonNetwork.Destroy(gameObject);
        }
        
    }

    public void SetShieldStrength(float strength) {
        Health *= strength;
    }

    public void SetDistribution(ManaDistribution newDist) {
        ShieldDistribution = newDist;
    }


    void FixedUpdate() {
        if (Health <= 0f) {
            Break();
        }
    }
}
