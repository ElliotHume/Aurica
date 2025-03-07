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


    public float RewardsForWin = 0.01f, RewardsForLoss = 0.002f;
    public int PlayerThresholdForEnhancedRewards = 4;
    public float DecreasedRewardsForWin = 0.002f, DecreasedRewardsForLoss = 0.001f;
    public Transform BlueSideSpawnPoint, RedSideSpawnPoint;
    public Color blueNameColor = Color.blue, redNameColor = Color.red;
    public Text blueLifeCounter, redLifeCounter, blueSidePlayerCountText, redSidePlayerCountText, rewardsText;
    public GameObject deathmatchText, redSideText, blueSideText;
    public Material blueSidePlayerMaterial, redSidePlayerMaterial;
    public GameObject DeathMatchGamePanel, readyButton, resultsPanel, bluesidewinUI, redsidewinUI, victoryText, defeatText, teamSelectPanel;
    public Button blueSideTeamSelectButton, redSideTeamSelectButton;
    public List<GameObject> ToggleObjects;

    public int LivesPerPlayer = 1;
    public float RespawnTimer;
    public AudioSource MatchMusic, TeamSelectClip;
    public bool disabled = false;
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
    public void AssignPlayers(string receivedBlueSideNames, string receivedRedSideNames, int blueSideLifeCount, int redSideLifeCount) {
        if (matchStarted) return;
        blueSideLives = blueSideLifeCount * LivesPerPlayer + 1;
        redSideLives = redSideLifeCount * LivesPerPlayer + 1;

        PlayerManager[] ps = FindObjectsOfType<PlayerManager>();
        foreach( PlayerManager player in ps ) {
            if (receivedBlueSideNames.Contains(player.GetUniqueName())) {
                blueSide.Add(player);
                player.SetNameColor(blueNameColor);
                player.SetPlayerOutline(blueNameColor);
            } else if (receivedRedSideNames.Contains(player.GetUniqueName())) {
                redSide.Add(player);
                player.SetNameColor(redNameColor);
                player.SetPlayerOutline(redNameColor);
            } else {
                Debug.Log("Player ["+player.GetUniqueName()+"] not found on either team");
            }
        }

        if (localPlayer == null) localPlayer = PlayerManager.LocalInstance;
        Debug.Log("Starting match -- blue lives: "+blueSideLives+"  red lives: "+redSideLives);
        Debug.Log("Local Player name: "+localPlayer.GetUniqueName());
        isBlueTeam = receivedBlueSideNames.Contains(localPlayer.GetUniqueName());
        deathmatchText.SetActive(false);
        if (isBlueTeam) {
            blueSideText.SetActive(true);
        } else {
            redSideText.SetActive(true);
        }
        teamSelectPanel.SetActive(false);

        blueSideNames = receivedBlueSideNames;
        redSideNames = receivedRedSideNames;

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
            Debug.Log("Rebalance teams until there is atleast one player on each team, and all players have chosen a side.");
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
        disabled = false;
        readyButton.SetActive(false);
        teamSelectPanel.SetActive(true);
        blueSideTeamSelectButton.interactable = true;
        redSideTeamSelectButton.interactable = true;
        if (TeamSelectClip != null) TeamSelectClip.Play();
    }

    public void SelectTeam(bool isBlue) {
        if (localPlayer == null) localPlayer = PlayerManager.LocalInstance;
        blueSideTeamSelectButton.interactable = false;
        redSideTeamSelectButton.interactable = false;
        photonView.RPC("SendTeamSelection", RpcTarget.All, localPlayer.GetUniqueName(), isBlue);
    }

    [PunRPC]
    public void SendTeamSelection(string uniquePlayerCode, bool isBlue) {
        if (!matchStarter) return;

        Debug.Log("Player: ["+uniquePlayerCode+"] assign to "+ (isBlue ? "BLUE" : "RED"));
        if (blueSideNames.Contains(uniquePlayerCode) || redSideNames.Contains(uniquePlayerCode)) {
            Debug.Log("Player has already been assigned a team.");
            return;
        }

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

        if (FreeForAllGameManager.Instance != null) FreeForAllGameManager.Instance.Disable();
    }

    [PunRPC]
    public void EndMatch(int winningTeam) {
        if (!matchStarted) return;

        if (winningTeam != 2) {
            resultsPanel.SetActive(true);
            if (winningTeam == 0) {
                bluesidewinUI.SetActive(true);
                redsidewinUI.SetActive(false);
                if (isBlueTeam) {
                    victoryText.SetActive(true);
                    defeatText.SetActive(false);
                } else {
                    victoryText.SetActive(false);
                    defeatText.SetActive(true);
                }
            } else if (winningTeam == 1) {
                redsidewinUI.SetActive(true);
                bluesidewinUI.SetActive(false);
                if (!isBlueTeam) {
                    victoryText.SetActive(true);
                    defeatText.SetActive(false);
                } else {
                    victoryText.SetActive(false);
                    defeatText.SetActive(true);
                }
            }

            // Distribute rewards
            float rewardsEarned = 0f;
            bool enhancedRewards = (blueSide.Count + redSide.Count) >= PlayerThresholdForEnhancedRewards;
            if (isBlueTeam) {
                if (winningTeam == 0) {
                    rewardsEarned = enhancedRewards ? RewardsForWin : DecreasedRewardsForWin;
                } else {
                    rewardsEarned = enhancedRewards ? RewardsForLoss : DecreasedRewardsForLoss;
                }
            } else {
                if (winningTeam == 1) {
                    rewardsEarned = enhancedRewards ? RewardsForWin : DecreasedRewardsForWin;
                } else {
                    rewardsEarned = enhancedRewards ? RewardsForLoss : DecreasedRewardsForLoss;
                }
            }
            RewardsManager.Instance.AddRewards(rewardsEarned);
            PlayerManager.LocalInstance.PlayCultivationEffect();
            if (enhancedRewards) {
                rewardsText.text = "ENHANCED REWARDS!\nCultivation Earned: "+(Mathf.Round(rewardsEarned * 1000f)).ToString();
            } else {
                rewardsText.text = "\nCultivation Earned: "+(Mathf.Round(rewardsEarned * 1000f)).ToString();
            }

        } else {
            rewardsText.text = "\nABANDON - No Cultivation Earned";
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

        blueSideText.SetActive(false);
        redSideText.SetActive(false);
        deathmatchText.SetActive(true);
            
        matchStarter = false;

        PlayerManager[] ps = FindObjectsOfType<PlayerManager>();

        readyButton.SetActive(true);

        SpawnLocalPlayer();

        matchStarted = false;
        foreach(var obj in ToggleObjects) obj.SetActive(!obj.activeInHierarchy);
        if (MatchMusic != null) MatchMusic.Stop();
        if (FreeForAllGameManager.Instance != null) FreeForAllGameManager.Instance.Enable();
    }

    public void playerDeath(PlayerManager player) {
        if (disabled) return;
        StartCoroutine(RespawnPlayer(player));
    }

    public void localPlayerDeath(string playerID) {
        photonView.RPC("SendKillEvent", RpcTarget.All, playerID);
    }

    [PunRPC]
    public void SendKillEvent(string playerID) {
        if (matchStarted) {
            if (blueSideNames.Contains(playerID)) {
                blueSideLives -= 1;
                blueLifeCounter.text = blueSideLives.ToString();
            } else if (redSideNames.Contains(playerID)) {
                redSideLives -= 1;
                redLifeCounter.text = redSideLives.ToString();
            }

            if (PhotonNetwork.IsMasterClient) {
                if (blueSideLives <= 0) {
                    photonView.RPC("EndMatch", RpcTarget.All, 1);
                }
                if (redSideLives <= 0) {
                    photonView.RPC("EndMatch", RpcTarget.All, 0);
                }
            }
        }
    }


    public void SpawnLocalPlayer() {
        if (localPlayer == null) localPlayer = PlayerManager.LocalInstance;
        localPlayer.Teleport(isBlueTeam ? BlueSideSpawnPoint : RedSideSpawnPoint);
        localPlayer.HardReset();
    }

    IEnumerator RespawnPlayer(PlayerManager player) {
        yield return new WaitForSeconds(RespawnTimer);
        player.Teleport(isBlueTeam ? BlueSideSpawnPoint : RedSideSpawnPoint);
        player.Respawn();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) {
        if (PhotonNetwork.IsMasterClient && matchStarted) photonView.RPC("EndMatch", RpcTarget.All, 2);
    }

    public void Disable() {
        disabled = true;
        DeathMatchGamePanel.SetActive(false);
    }

    public void Enable() {
        disabled = false;
        DeathMatchGamePanel.SetActive(true);
    }
}
