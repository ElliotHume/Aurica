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


    public float RewardsForWin = 0.01f, RewardsForLoss = 0.002f, DecreasedRewardsForWin = 0.002f, DecreasedRewardsForLoss = 0.001f;
    public int PlayerThresholdForEnhancedRewards = 4;
    public Transform BlueSideSpawnPoint, RedSideSpawnPoint;
    public Color blueNameColor = Color.blue, redNameColor = Color.red;
    public Text blueLifeCounter, redLifeCounter, blueSidePlayerCountText, redSidePlayerCountText;
    public Material blueSidePlayerMaterial, redSidePlayerMaterial;
    public GameObject DeathMatchGamePanel, readyButton, resultsPanel, bluesidewinUI, redsidewinUI, teamSelectPanel;
    // public Button blueSideTeamSelectButton, redSideTeamSelectButton;
    public List<GameObject> ToggleObjects;

    public int LivesPerPlayer = 1;
    public float RespawnTimer;
    public AudioSource MatchMusic, TeamSelectClip;
    int blueSideLives, redSideLives;



    List<PlayerManager> blueSide, redSide;
    bool matchStarted = false, isBlueTeam = false, matchStarter = false;
    PlayerManager localPlayer;

    string blueSideNames = "", redSideNames = "";
    int blueSidePlayerCount, redSidePlayerCount;


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
        blueSideLives = blueSideLifeCount * LivesPerPlayer + 1;
        redSideLives = redSideLifeCount * LivesPerPlayer + 1;

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
        teamSelectPanel.SetActive(false);

        StartMatch();
    }

    public void SendGameStart() {
        matchStarter = true;
        photonView.RPC("ShowTeamSelect", RpcTarget.All);
        
        StartCoroutine(StartGameTimer());
    }

    IEnumerator StartGameTimer() {
        yield return new WaitForSeconds(7f);
        PlayerManager[] ps = FindObjectsOfType<PlayerManager>();
        int totalPlayerCount = ps.Length;
        // Don't start until all of the players have chosen a side, and neither team is empty
        while ((blueSidePlayerCount == 0 || redSidePlayerCount == 0) || (redSidePlayerCount + blueSidePlayerCount < totalPlayerCount)) {
            Debug.Log("No players on one of the teams, rebalance teams until there is atleast one player on each team.");
            yield return new WaitForSeconds(3f);
        }
        StartGame();
    }

    public void StartGame() {
        int index = 0, blueSideLifeCount = 0, redSideLifeCount = 0;
        
        PlayerManager[] ps = FindObjectsOfType<PlayerManager>();
        List<PlayerManager> playerManagers = ps.OrderBy(x => Random.Range(0, 10)).ToList();
        foreach (var p in playerManagers) {
            // If players have already selected a team, up the life count and then skip them.
            if ( blueSideNames.Contains(p.GetUniqueName()) ){
                blueSideLifeCount++;
                continue;
            } else if ( redSideNames.Contains(p.GetUniqueName()) ){
                redSideLifeCount++;
                continue;
            }

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

        if (blueSideLifeCount < redSideLifeCount) blueSideLifeCount += 2 * (redSideLifeCount - blueSideLifeCount);
        if (redSideLifeCount < blueSideLifeCount) redSideLifeCount += 2 * (blueSideLifeCount - redSideLifeCount);

        Debug.Log("ASSIGN PLAYERS: \n   BLUE: "+blueSideNames+"\n   RED: "+redSideNames);

        photonView.RPC("AssignPlayers", RpcTarget.All, blueSideNames, redSideNames, blueSideLifeCount, redSideLifeCount);
        teamSelectPanel.SetActive(false);
    }

    [PunRPC]
    public void ShowTeamSelect() {
        readyButton.SetActive(false);
        teamSelectPanel.SetActive(true);
        if (TeamSelectClip != null) TeamSelectClip.Play();
    }

    public void SelectTeam(bool isBlue) {
        if (localPlayer == null) localPlayer = PlayerManager.LocalInstance;
        photonView.RPC("SendTeamSelection", RpcTarget.All, localPlayer.GetUniqueName(), isBlue);
    }

    [PunRPC]
    public void SendTeamSelection(string uniquePlayerCode, bool isBlue) {
        if (!matchStarter) return;

        Debug.Log("Player: ["+uniquePlayerCode+"] assign to "+ (isBlue ? "BLUE" : "RED"));

        if (isBlue && !blueSideNames.Contains(uniquePlayerCode)) {
            blueSideNames += uniquePlayerCode;
            blueSidePlayerCount += 1;
        } else if (!isBlue && !redSideNames.Contains(uniquePlayerCode)){
            redSideNames += uniquePlayerCode;
            redSidePlayerCount += 1;
        }

        blueSidePlayerCountText.text = "Players: "+blueSidePlayerCount;
        redSidePlayerCountText.text = "Players: "+redSidePlayerCount;


        // // Keep teams reasonably balanced [DISABLED FOR NOW]
        // if (blueSidePlayerCount > redSidePlayerCount) {
        //     blueSideTeamSelectButton.interactable = false;
        // } else {
        //     blueSideTeamSelectButton.interactable = true;
        // }

        // if (redSidePlayerCount > blueSidePlayerCount) {
        //     redSideTeamSelectButton.interactable = false;
        // } else {
        //     redSideTeamSelectButton.interactable = true;
        // }


        photonView.RPC("SendTeamPlayerCount", RpcTarget.All, blueSidePlayerCount, redSidePlayerCount);
    }

    [PunRPC]
    public void SendTeamPlayerCount(int blue, int red) {
        if (matchStarter) return;

        blueSidePlayerCount = blue;
        redSidePlayerCount = red;

        blueSidePlayerCountText.text = "Players: "+blueSidePlayerCount;
        redSidePlayerCountText.text = "Players: "+redSidePlayerCount;
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
        if (!matchStarted) return;

        if (winningTeam != 2) resultsPanel.SetActive(true);
        if (winningTeam == 0) {
            bluesidewinUI.SetActive(true);
            redsidewinUI.SetActive(false);
        } else if (winningTeam == 1) {
            redsidewinUI.SetActive(true);
            bluesidewinUI.SetActive(false);
        }

        blueSideLives = LivesPerPlayer;
        redSideLives = LivesPerPlayer;
        blueSide.Clear();
        redSide.Clear();
        blueSideNames = "";
        redSideNames = "";
        blueLifeCounter.text = blueSideLives.ToString();
        redLifeCounter.text = redSideLives.ToString();
        blueSidePlayerCountText.text = "Players: 0";
        redSidePlayerCountText.text = "Players: 0";
        blueSidePlayerCount = 0;
        redSidePlayerCount = 0;

        matchStarter = false;

        PlayerManager[] ps = FindObjectsOfType<PlayerManager>();

        readyButton.SetActive(true);

        SpawnLocalPlayer();

        matchStarted = false;
        foreach(var obj in ToggleObjects) obj.SetActive(!obj.activeInHierarchy);
        if (MatchMusic != null) MatchMusic.Stop();
    }

    public void playerDeath(PlayerManager player) {
        if (PhotonNetwork.IsMasterClient &&matchStarted) {
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
        localPlayer.Teleport(isBlueTeam ? BlueSideSpawnPoint : RedSideSpawnPoint);
        localPlayer.HardReset();
    }

    IEnumerator RespawnPlayer(PlayerManager player) {
        yield return new WaitForSeconds(RespawnTimer);
        player.Respawn();
        player.Teleport(isBlueTeam ? BlueSideSpawnPoint : RedSideSpawnPoint);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) {
        if (PhotonNetwork.IsMasterClient && matchStarted) photonView.RPC("EndMatch", RpcTarget.All, 2);
    }
}
