using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class DeathmatchGameManager : MonoBehaviourPunCallbacks {

    public static DeathmatchGameManager Instance;

    public List<PlayerManager> players;
    public Transform BlueSideSpawnPoint, RedSideSpawnPoint;
    public Text blueLifeCounter, redLifeCounter;

    public int StartingTeamLives = 5;
    public float RespawnTimer;
    int blueSideLives, redSideLives;



    List<PlayerManager> blueSide, redSide;
    bool matchStarted = false;


    // Start is called before the first frame update
    void Start() {
        Instance = this;

        blueSide = new List<PlayerManager>();
        redSide = new List<PlayerManager>();

        blueSideLives = StartingTeamLives;
        redSideLives = StartingTeamLives;
    }

    [PunRPC]
    public void AddPlayer() {
        PlayerManager[] ps = FindObjectsOfType<PlayerManager>();
        PlayerManager p = null;
        foreach( var pr in ps) {
            if (!players.Contains(pr)) {
                p = pr;
                break;
            }
        }
        Debug.Log("ADDING REMOTE PLAYER TO DEATHMATCH "+p);
        players.Add(p);

        if (players.Count % 2 == 1) {
            // ODD player, add to blue side
            Debug.Log("Assigned to blue team");
            blueSide.Add(p);
        } else {
            // EVEN player, add to red side
            Debug.Log("Assigned to red team");
            redSide.Add(p);
        }

        if (players.Count >= 2 && !matchStarted) {
            Debug.Log("Starting the match....");
            StartMatch();
        }
    }

    public void AddLocalPlayer() {
        PlayerManager p = PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager>();
        players.Add(p);
        Debug.Log("ADDING LOCAL PLAYER TO DEATHMATCH "+p.gameObject);

        if (players.Count % 2 == 1) {
            // ODD player, add to blue side
            Debug.Log("Assigned to blue team");
            blueSide.Add(p);
        } else {
            // EVEN player, add to red side
            Debug.Log("Assigned to red team");
            redSide.Add(p);
        }

        if (players.Count >= 2 && !matchStarted) {
            Debug.Log("Starting the match....");
            StartMatch();
        }

        photonView.RPC("AddPlayer", RpcTarget.All);
    }


    public void StartMatch() {
        foreach (var player in blueSide) {
            player.Teleport(BlueSideSpawnPoint.position);
            player.HardReset();
        }
        foreach (var player in redSide) {
            player.Teleport(RedSideSpawnPoint.position);
            player.HardReset();
        }

        DisplayBeginMessage();
        matchStarted = true;
    }

    public void DisplayBeginMessage() {
        // TODO
    }

    public void EndMatch(int winningTeam) {
        // TODO
    }

    public void playerDeath(PlayerManager player) {
        if (!matchStarted) {
            StartCoroutine(RespawnPlayer(player, BlueSideSpawnPoint));
            return;
        }

        if (blueSide.Contains(player)) {
            blueSideLives -= 1;
            blueLifeCounter.text = blueSideLives.ToString();
            StartCoroutine(RespawnPlayer(player, BlueSideSpawnPoint));
        } else if (redSide.Contains(player)) {
            redSideLives -= 1;
            redLifeCounter.text = redSideLives.ToString();
            StartCoroutine(RespawnPlayer(player, RedSideSpawnPoint));
        }

        if (blueSideLives <= 0) {
            EndMatch(0);
        }
        if (redSideLives <= 0) {
            EndMatch(1);
        }
    }

    IEnumerator RespawnPlayer(PlayerManager player, Transform spawnPoint) {
        yield return new WaitForSeconds(RespawnTimer);
        player.Respawn();
        player.Teleport(spawnPoint.position);
    }
}
