using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public GameObject readyButton, resultsPanel, bluesidewinUI, redsidewinUI;

    public int LivesPerPlayer = 1;
    public float RespawnTimer;
    public AudioSource MatchMusic;
    int blueSideLives, redSideLives;



    List<PlayerManager> blueSide, redSide;
    bool matchStarted = false;


    // Start is called before the first frame update
    void Start() {
        Instance = this;

        blueSide = new List<PlayerManager>();
        redSide = new List<PlayerManager>();

        blueSideLives = LivesPerPlayer;
        redSideLives = LivesPerPlayer;
    }

    [PunRPC]
    public void AssignPlayers() {

        // Clear all in case of retry
        players.Clear();
        blueSide.Clear();
        redSide.Clear();

        PlayerManager[] ps = FindObjectsOfType<PlayerManager>();
        List<PlayerManager> playerManagers = ps.OrderBy(x => x.Mana).ToList();
        foreach( var p in playerManagers) {
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
        }

        if (players.Count >= 2 && !matchStarted) {
            Debug.Log("Starting the match....");
            StartMatch();
        }
    }

    public void SendGameStart() {
        photonView.RPC("AssignPlayers", RpcTarget.All);
    }


    public void StartMatch() {

        blueSideLives = blueSide.Count * LivesPerPlayer;
        redSideLives = redSide.Count * LivesPerPlayer;

        if (blueSide.Count < redSide.Count) blueSideLives += 3;
        if (redSide.Count < blueSide.Count) redSideLives += 3;

        blueLifeCounter.text = blueSideLives.ToString();
        redLifeCounter.text = redSideLives.ToString();

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
        readyButton.SetActive(false);

        if (MatchMusic != null)MatchMusic.Play();
    }

    public void DisplayBeginMessage() {
        // TODO
    }

    [PunRPC]
    public void EndMatch(int winningTeam) {
        // TODO
        resultsPanel.SetActive(true);
        if (winningTeam == 0) {
            bluesidewinUI.SetActive(true);
        } else {
            redsidewinUI.SetActive(true);
        }

        blueSideLives = LivesPerPlayer;
        redSideLives = LivesPerPlayer;
        blueLifeCounter.text = blueSideLives.ToString();
        redLifeCounter.text = redSideLives.ToString();

        readyButton.SetActive(true);

        foreach (var player in blueSide) {
            player.Teleport(BlueSideSpawnPoint.position);
            player.HardReset();
        }
        foreach (var player in redSide) {
            player.Teleport(RedSideSpawnPoint.position);
            player.HardReset();
        }

        matchStarted = false;
        if (MatchMusic != null) MatchMusic.Stop();

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
            photonView.RPC("EndMatch", RpcTarget.All, 1);
        }
        if (redSideLives <= 0) {
            photonView.RPC("EndMatch", RpcTarget.All, 0);
        }
    }

    IEnumerator RespawnPlayer(PlayerManager player, Transform spawnPoint) {
        yield return new WaitForSeconds(RespawnTimer);
        player.Respawn();
        player.Teleport(spawnPoint.position);
    }
}
