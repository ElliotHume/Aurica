using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemySpawner : MonoBehaviourPun {

    public string enemyID = "Mob_Ghoul";
    public float respawnTimer = 60f;

    public string networkSpawnFX = "Enemies/SpawnFX/StandardGhoulFX";

    public float radiusTrigger = 0f;
    public LayerMask triggerLayer; 

    public bool preventSpawnPatrolling = false;

    Enemy spawn;
    GameObject spawnGO;
    bool hasSpawned = false, isSpawnAlive = false, respawnInitiated = false;

    // Start is called before the first frame update
    void Start() {
        if (radiusTrigger == 0f) Spawn();
    }

    void FixedUpdate() {
        if (!photonView.IsMine) return;

        if (radiusTrigger != 0f && !hasSpawned) {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            bool playerInRange = false;
            foreach(var player in players) {
                if (Vector3.Distance(player.transform.position, transform.position) < radiusTrigger) {
                    playerInRange = true;
                    break;
                }
            }
            if (playerInRange && !respawnInitiated) Spawn();
        }
        
        if (hasSpawned && spawn != null) {
            isSpawnAlive = spawn.Health > 0f;
            if (!isSpawnAlive && !respawnInitiated) {
                // Debug.Log("Initiate Respawn of: " + enemyID);
                hasSpawned = false;
                StartCoroutine(Respawn());
            }
        }
    }

    public void Spawn() {
        if (hasSpawned || !photonView.IsMine) return;
        // Debug.Log("Spawning "+enemyID);
        spawnGO = PhotonNetwork.Instantiate("Enemies/"+enemyID, transform.position, transform.rotation);
        spawn = spawnGO.GetComponent<Enemy>();
        if (preventSpawnPatrolling && spawn != null) spawn.doesPatrol = false;
        hasSpawned = true;
        isSpawnAlive = true;

        // Create spawn FX
        if (networkSpawnFX != "") spawnGO = PhotonNetwork.Instantiate(networkSpawnFX, transform.position, transform.rotation);
    }

    IEnumerator Respawn() {
        respawnInitiated = true;
        yield return new WaitForSeconds(respawnTimer);
        if (radiusTrigger == 0f) {
            Spawn();
        }
        respawnInitiated = false;
    }

    void OnDrawGizmosSelected(){
        if (radiusTrigger > 0f) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radiusTrigger);
        }
    }
}
