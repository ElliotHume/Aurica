using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TowerAttack : MonoBehaviourPun {

    public float DamagePerTick = 25f;

    private GameObject TargetGO;
    private PlayerManager TargetPM;
    private Vector3 originPoint;
    private Structure structure;

    void Start() {
        originPoint = transform.position;
    }

    // Update is called once per frame
    void Update() {
        if (TargetGO == null) return;
        // Move the laser beam to the origin point
        transform.position = originPoint;
        // Get the position of the targeted player
        Vector3 destinationPoint = TargetGO.transform.position + (Vector3.up * 1.5f);
        // Set the scale of the beam to be the distance to the target player
        transform.localScale = new Vector3(1f, 1f, Vector3.Distance(originPoint, destinationPoint));
        // Rotate the beam to point at the target player
        transform.LookAt(destinationPoint);
    }

    void OnTriggerStay(Collider other) {
        if (!photonView.IsMine) return;
        string ownerID = structure != null ? structure.GetName() : "Tower";
        if (other.gameObject.tag == "Player") {
            PlayerManager pm = other.gameObject.GetComponent<PlayerManager>();
            if (pm != null) {
                PhotonView pv = PhotonView.Get(pm);
                if (pv != null) pv.RPC("OnSpellCollide", RpcTarget.All, DamagePerTick * 0.002f, "", 1f, new ManaDistribution(0f, -1f, 1f, 1f, 1f, 1f, 0f).GetJson(), ownerID);
            }
        } else if (other.gameObject.tag == "Shield") {
            ShieldSpell ss = other.gameObject.GetComponentInParent<ShieldSpell>();
            if (ss != null) {
                PhotonView pv = PhotonView.Get(ss);
                if (pv != null) pv.RPC("TakeDamage", RpcTarget.All, DamagePerTick * 0.01f, new ManaDistribution(0f, -1f, 1f, 1f, 1f, 1f, 0f).GetJson());
            }
        } else if (other.gameObject.tag == "Enemy") {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if (enemy != null) {
                enemy.SetLocalPlayerParticipation();
                PhotonView pv = PhotonView.Get(enemy);
                if (pv != null) {
                    pv.RPC("OnSpellCollide", RpcTarget.All, DamagePerTick * 0.002f, "", 1f, new ManaDistribution(0f, -1f, 1f, 1f, 1f, 1f, 0f).GetJson(), ownerID);
                }
            }
        }
    }

    public void SetStructure(Structure newStructure) {
        structure = newStructure;
    }

    public void SetOriginPoint(Vector3 origin) {
        originPoint = origin;
    }

    public void SetTarget(GameObject targetGO) {
        TargetGO = targetGO;
        TargetPM = targetGO.GetComponent<PlayerManager>();

        if (TargetPM != null) photonView.RPC("NetworkSetPlayerTarget", RpcTarget.All, TargetPM.GetUniqueName());
    }

    [PunRPC]
    public void NetworkSetPlayerTarget(string PlayerID) {
        if (photonView.IsMine) return;
        PlayerManager pm = GameManager.GetPlayerFromID(PlayerID);

        TargetGO = pm.gameObject;
        TargetPM = pm;
    }
}
