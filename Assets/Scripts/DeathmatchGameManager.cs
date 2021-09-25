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

    public Transform BlueSideSpawnPoint, RedSideSpawnPoint;
    public Color blueNameColor = Color.blue, redNameColor = Color.red;
    public Text blueLifeCounter, redLifeCounter;
    public Material blueSidePlayerMaterial, redSidePlayerMaterial;
    public GameObject DeathMatchGamePanel, readyButton, resultsPanel, bluesidewinUI, redsidewinUI;
    public List<GameObject> ToggleObjects;

    public int LivesPerPlayer = 1;
    public float RespawnTimer;
    public AudioSource MatchMusic;
    int blueSideLives, redSideLives;



    List<PlayerManager> blueSide, redSide;
    bool matchStarted = false, isBlueTeam = false;
    PlayerManager localPlayer;


    // Start is called before the first frame update
    void Start() {
        DeathmatchGameManager.Instance = this;

        blueSide = new List<PlayerManager>();
        redSide = new List<PlayerManager>();

        blueSideLives = LivesPerPlayer;
        redSideLives = LivesPerPlayer;

        localPlayer = PlayerManager.LocalInstance;

        DeathMatchGamePanel.SetActive(true);
    }

    [PunRPC]
    public void AssignPlayers(string blueSideNames, string redSideNames, int blueSideLifeCount, int redSideLifeCount) {
        if (matchStarted) return;
        blueSideLives = blueSideLifeCount * LivesPerPlayer;
        redSideLives = redSideLifeCount * LivesPerPlayer;

        PlayerManager[] ps = FindObjectsOfType<PlayerManager>();
        foreach( PlayerManager player in ps ) {
            if (blueSideNames.Contains(player.GetUniqueName())) {
                blueSide.Add(player);
                player.SetNameColor(blueNameColor);
                if (blueSidePlayerMaterial != null) player.SetPlayerMaterial(blueSidePlayerMaterial);
                player.SetPlayerOutline(blueNameColor);
            } else if (redSideNames.Contains(player.GetUniqueName())) {
                redSide.Add(player);
                player.SetNameColor(redNameColor);
                if (redSidePlayerMaterial != null) player.SetPlayerMaterial(redSidePlayerMaterial);
                player.SetPlayerOutline(redNameColor);
            } else {
                Debug.Log("Player ["+player.GetUniqueName()+"] not found on either team");
            }
        }

        if (localPlayer == null) localPlayer = PlayerManager.LocalInstance;
        Debug.Log("Starting match -- blue lives: "+blueSideLives+"  red lives: "+redSideLives);
        Debug.Log("Local Player name: "+localPlayer.GetUniqueName());
        isBlueTeam = blueSideNames.Contains(localPlayer.GetUniqueName());

        StartMatch();
    }

    public void SendGameStart() {
        int index = 0, blueSideLifeCount = 0, redSideLifeCount = 0;
        string blueSideNames = "", redSideNames = "";

        PlayerManager[] ps = FindObjectsOfType<PlayerManager>();
        List<PlayerManager> playerManagers = ps.OrderBy(x => x.Mana).ToList();
        foreach (var p in playerManagers) {
            if (index % 2 == 1) {
                // ODD player, add to blue side
                blueSideNames += p.GetUniqueName();
                blueSideLifeCount++;
            } else {
                // EVEN player, add to red side
                redSideNames += p.GetUniqueName();
                redSideLifeCount++;
            }
            index++;
        }

        if (blueSideLifeCount < redSideLifeCount) blueSideLifeCount += 2;
        if (redSideLifeCount < blueSideLifeCount) redSideLifeCount += 2;

        Debug.Log("ASSIGN PLAYERS: \n   BLUE: "+blueSideNames+"\n   RED: "+redSideNames);

        photonView.RPC("AssignPlayers", RpcTarget.All, blueSideNames, redSideNames, blueSideLifeCount, redSideLifeCount);
    }

    public void StartMatch() {
        blueLifeCounter.text = blueSideLives.ToString();
        redLifeCounter.text = redSideLives.ToString();

        SpawnLocalPlayer();

        matchStarted = true;
        readyButton.SetActive(false);

        foreach(var obj in ToggleObjects) obj.SetActive(!obj.activeInHierarchy);

        if (MatchMusic != null) MatchMusic.Play();
    }

    [PunRPC]
    public void EndMatch(int winningTeam) {
        resultsPanel.SetActive(true);
        if (winningTeam == 0) {
            bluesidewinUI.SetActive(true);
            redsidewinUI.SetActive(false);
        } else {
            redsidewinUI.SetActive(true);
            bluesidewinUI.SetActive(false);
        }

        blueSideLives = LivesPerPlayer;
        redSideLives = LivesPerPlayer;
        blueLifeCounter.text = blueSideLives.ToString();
        redLifeCounter.text = redSideLives.ToString();

        PlayerManager[] ps = FindObjectsOfType<PlayerManager>();

        readyButton.SetActive(true);

        SpawnLocalPlayer();

        matchStarted = false;
        foreach(var obj in ToggleObjects) obj.SetActive(!obj.activeInHierarchy);
        if (MatchMusic != null) MatchMusic.Stop();
    }

    public void playerDeath(PlayerManager player) {
        if (matchStarted) {
            if (blueSide.Contains(player)) {
                blueSideLives -= 1;
                blueLifeCounter.text = blueSideLives.ToString();
            } else if (redSide.Contains(player)) {
                redSideLives -= 1;
                redLifeCounter.text = redSideLives.ToString();
            }

            if (blueSideLives <= 0) {
                photonView.RPC("EndMatch", RpcTarget.All, 1);
            }
            if (redSideLives <= 0) {
                photonView.RPC("EndMatch", RpcTarget.All, 0);
            }
        }
        StartCoroutine(RespawnPlayer(player));
    }

    public void SpawnLocalPlayer() {
        if (localPlayer == null) localPlayer = PlayerManager.LocalInstance;
        localPlayer.Teleport(isBlueTeam ? BlueSideSpawnPoint.position : RedSideSpawnPoint.position);
        localPlayer.HardReset();
    }

    IEnumerator RespawnPlayer(PlayerManager player) {
        yield return new WaitForSeconds(RespawnTimer);
        player.Respawn();
        player.Teleport(isBlueTeam ? BlueSideSpawnPoint.position : RedSideSpawnPoint.position);
    }
}
