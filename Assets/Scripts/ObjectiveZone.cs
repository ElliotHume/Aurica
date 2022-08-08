using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public class ObjectiveZone : MonoBehaviourPun {

    public float PointsPerSecond = 1f;
    public UnityEvent OnUnoccupied, OnLocalOccupy, OnEnemyOccupy, OnContested;

    /** State of Zone
        0 = Unoccupied
        1 = Occupied solely by the local player
        2 = Occupied solely by a non-local player
        3 = Occupied by more than one player (Contested)
    **/
    int ZoneState = 0, PreviousZoneState = 0;

    float timer = 0;
    bool localPlayerInZone = false;
    List<string> playersInZone = new List<string>();

    void OnTriggerEnter(Collider collider) {
        if (collider.gameObject.tag == "Player") {
            PlayerManager pm = collider.gameObject.GetComponent<PlayerManager>();
            // If no PlayerManager is found on the GameObject, or the player has already registered as having entered the zone, return
            if (pm == null || playersInZone.Contains(pm.GetUniqueName())) return;

            // Add the player to the list of players in the zone
            playersInZone.Add(pm.GetUniqueName());
            
            // Check if the colliding player is the local player
            if (pm == PlayerManager.LocalInstance) {
                // Debug.Log("Local Player entering objective zone");
                localPlayerInZone = true;
            }
        }
    }

    void OnTriggerExit(Collider collider) {
        if (collider.gameObject.tag == "Player") {
            PlayerManager pm = collider.gameObject.GetComponent<PlayerManager>();
            // If no PlayerManager is found on the GameObject return
            if (pm == null) return;

            // Remove the player from the list of players in the zone
            playersInZone.Remove(pm.GetUniqueName());
            
            // Check if the colliding player is the local player
            if (pm == PlayerManager.LocalInstance) {
                // Debug.Log("Local Player leaving objective zone");
                localPlayerInZone = false;
            }
        }
    }

    void FixedUpdate() {
        // Check the state of the zone (see ZoneState for info)
        // Add time to timer if ZoneState 1, else reset timer
        if (playersInZone.Count == 1) {
            if (localPlayerInZone) {
                timer += Time.deltaTime;
                // Debug.Log("Local Player in objective zone");
                ZoneState = 1;
                // At 1s, Score points for the local player in the zone, reset timer
                if (timer >= 1f) {
                    // Debug.Log("Scoring Points for Local Player: " + PointsPerSecond);
                    FreeForAllGameManager.Instance.ScorePointsForLocalPlayer(PointsPerSecond);
                    timer = 0f;
                }
            } else {
                ZoneState = 2;
                timer = 0f;
            }
        } else if (playersInZone.Count > 1) {
            ZoneState = 3;
            timer = 0f;
        } else if (playersInZone.Count == 0) {
            ZoneState = 0;
            timer = 0f;
        }

        if (PreviousZoneState != ZoneState) {
            switch (ZoneState) {
                case 0:
                    // Set Unoccupied FX
                    OnUnoccupied.Invoke();
                    break;
                case 1:
                    // Set Occupied by local player FX
                    OnLocalOccupy.Invoke();
                    break;
                case 2:
                    // Set Occupied by enemy player FX
                    OnEnemyOccupy.Invoke();
                    break;
                case 3:
                    // Set contested FX
                    OnContested.Invoke();
                    break;
            }
        }

        PreviousZoneState = ZoneState;
    }
}
