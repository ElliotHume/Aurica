using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;

public class TeleportEffect : MonoBehaviourPun {

    public Vector3 direction = Vector3.zero;
    public float randomSphereRadius = 0f;

    public bool useNavMesh = true;
    public bool playerOriented = false;
    public bool canHitSelf = false, onlyHitSelf;
    public bool singleUse = false;
    public bool isAffectedBySpellStrength = true;

    public string NetworkEffectOnTeleport;

    private bool isCollided = false;
    private Spell attachedSpell;

    void Start() {
        attachedSpell = GetComponent<Spell>();
    }

    public void ManualActivation(GameObject playerGO) {
        if (!photonView.IsMine) return;
        if (playerGO != null) {
            Activate(playerGO);
        }
    }

    void OnCollisionEnter(Collision collision) {
        Debug.Log("Collided with: "+collision.gameObject);
        if (photonView.IsMine && !isCollided) {
            if (collision.gameObject.tag == "Player" && (collision.gameObject != PlayerManager.LocalPlayerGameObject || canHitSelf) && !(onlyHitSelf && collision.gameObject != PlayerManager.LocalPlayerGameObject)) {
                Activate(collision.gameObject);
            }
        }
    }

    void OnTriggerEnter(Collider collision) {
        if (photonView.IsMine && !isCollided) {
            if (collision.gameObject.tag == "Player" && (collision.gameObject != PlayerManager.LocalPlayerGameObject || canHitSelf) && !(onlyHitSelf && collision.gameObject != PlayerManager.LocalPlayerGameObject)) {
                Activate(collision.gameObject);
            }
        }
    }

    void Activate(GameObject player) {
        PhotonView pv = PhotonView.Get(player);
        if (pv != null) {
            if (singleUse) isCollided = true;

            if (attachedSpell == null) attachedSpell = GetComponent<Spell>();
            float multiplier = attachedSpell != null && isAffectedBySpellStrength ? attachedSpell.GetSpellStrength() : 1f;

            Vector3 fixedDirection = direction * multiplier;
            Vector3 destination = player.transform.position + (randomSphereRadius > 0f ? Random.insideUnitSphere * randomSphereRadius * multiplier :
                    (playerOriented 
                        ? (player.transform.forward * fixedDirection.z + player.transform.right * fixedDirection.x + player.transform.up * fixedDirection.y)
                        : (transform.forward * fixedDirection.z + transform.right * fixedDirection.x + transform.up * fixedDirection.y)
                    ));

            

            if (useNavMesh) {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(destination, out hit, 5f, NavMesh.AllAreas)) {
                    pv.RPC("TeleportEffect", RpcTarget.All, hit.position);
                    PhotonNetwork.Instantiate(NetworkEffectOnTeleport, hit.position, Quaternion.identity);
                    return;
                }
            }
    
            // If you arent using navmesh immediately, check if the player fits at the destination, if they don't use navmesh
            RaycastHit rayHit;
            if (!Physics.CheckSphere(destination, 2f, 0, QueryTriggerInteraction.Ignore) && Physics.SphereCast(destination, 2, Vector3.down, out rayHit, Mathf.Max(randomSphereRadius, 25f))){
                pv.RPC("TeleportEffect", RpcTarget.All, destination);
                if (NetworkEffectOnTeleport != "") PhotonNetwork.Instantiate(NetworkEffectOnTeleport, destination, Quaternion.identity);
                return;
            } else {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(destination, out hit, Mathf.Max(randomSphereRadius, 10f), NavMesh.AllAreas)) {
                    pv.RPC("TeleportEffect", RpcTarget.All, hit.position);
                    if (NetworkEffectOnTeleport != "") PhotonNetwork.Instantiate(NetworkEffectOnTeleport, hit.position, Quaternion.identity);
                    return;
                }
            }
            PhotonNetwork.Instantiate("XCollision_Fizzle", player.transform.position, Quaternion.identity);
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, randomSphereRadius);
        if (!useNavMesh && !playerOriented && randomSphereRadius == 0f) {
            Gizmos.DrawWireSphere(transform.position + (transform.forward * direction.z + transform.right * direction.x + transform.up * direction.y), 2f);
        }
    }
}
