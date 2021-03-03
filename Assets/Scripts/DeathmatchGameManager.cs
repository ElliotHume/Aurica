using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class DeathmatchGameManager : MonoBehaviourPunCallbacks {

    public static DeathmatchGameManager Instance;

    public List<PlayerManager> players;
    public Transform BlueSideSpawnPoint, RedSideSpawnPoint;



    List<PlayerManager> blueSide, redSide;
    bool matchStarted = false;


    // Start is called before the first frame update
    void Start() {
        Instance = this;
    }

    // Update is called once per frame
    void Update() {

    }

    public void AddPlayer(GameObject playerGO) {
        PlayerManager p = playerGO.GetComponent<PlayerManager>();
        if (p != null) {
            players.Add(p);

            if (players.Count % 2 == 1) {
                // ODD player, add to blue side
                blueSide.Add(p);
            } else {
                // EVEN player, add to red side
                redSide.Add(p);
            }
        }

        if (players.Count >= 2 && !matchStarted) {
            StartMatch();
        }
    }


    public void StartMatch() {
        foreach (var player in blueSide) {
            player.gameObject.transform.position = BlueSideSpawnPoint.position;
            player.HardReset();
        }
        foreach (var player in redSide) {
            player.gameObject.transform.position = RedSideSpawnPoint.position;
            player.HardReset();
        }

        DisplayBeginMessage();
        matchStarted = true;
    }

    public void DisplayBeginMessage() {
        // TODO
    }
}
