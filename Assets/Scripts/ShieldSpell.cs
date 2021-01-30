using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ShieldSpell : Spell, IPunObservable {

    public float Health;
    public List<GameObject> spawnEffectsOnBreak;
    public List<string> networkedEffectsOnBreak;
    public ManaDistribution ShieldDistribution;

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
        Health -= ShieldDistribution.GetDamage(damage, damageDistribution);
        Debug.Log("Health: "+Health);
    }

    public void Break() {
        foreach (GameObject effect in spawnEffectsOnBreak) {
            Instantiate(effect, transform.position, transform.rotation);
        }
        if (photonView.IsMine) {
            foreach (string effect in networkedEffectsOnBreak) {
                PhotonNetwork.Instantiate(effect, transform.position+transform.up, transform.rotation);
            }
            PlayerManager owner = PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager>();
            if (owner != null) {
                PhotonView pv = PhotonView.Get(owner);
                if (pv != null) pv.RPC("BreakShield", RpcTarget.All);
            }

            PhotonNetwork.Destroy(gameObject);
        }
    }

    public void SetShieldStrength(float strength) {
        Debug.Log("Setting shield strength from: "+Health+" x"+strength);
        Health *= strength;
        Debug.Log("to: " + Health);
    }

    public void SetDistribution(ManaDistribution newDist) {
        Debug.Log("Set shield dist to: "+newDist.ToString());
        ShieldDistribution = newDist;
    }


    void FixedUpdate() {
        if (Health <= 0f) {
            Break();
        }
    }
}
