using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ShieldSpell : Spell, IPunObservable {

    public float Health;
    public ManaDistribution ShieldDistribution;
    public List<GameObject> spawnEffectsOnBreak;
    public List<string> networkedEffectsOnBreak;


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
    }

    public void Break() {
        foreach (GameObject effect in spawnEffectsOnBreak) {
            Instantiate(effect, transform.position, transform.rotation);
        }
        if (photonView.IsMine) {
            foreach (string effect in networkedEffectsOnBreak) {
                PhotonNetwork.Instantiate(effect, transform.position, transform.rotation);
            }
            PlayerManager owner = PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager>();
            if (owner != null) {
                PhotonView pv = PhotonView.Get(owner);
                if (pv != null) pv.RPC("BreakShield", RpcTarget.All);
            }

            PhotonNetwork.Destroy(gameObject);
        }
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
