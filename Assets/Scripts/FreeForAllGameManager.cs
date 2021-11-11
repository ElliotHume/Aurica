using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class FreeForAllGameManager : MonoBehaviourPunCallbacks, IPunObservable {

    public static FreeForAllGameManager Instance;

    public List<Transform> Spawnpoints;
    public Color enemyColor = Color.red;
    public GameObject FreeForAllGamePanel, readyButton, resultsPanel;
    public Text scoreText, timerText;
    public List<GameObject> ToggleObjects;

    public float PointsForKill = 1f, PointsForObjective=2f, PointLimit=8f, TimerSeconds=180f;
    public float RespawnTimer = 8f;
    public AudioSource MatchMusic;

    public Dictionary<string, float> playerScores;

    public Text VictoryText, DefeatText, ReasonText, WinnerText, SecondPlaceText, ThirdPlaceText;

    float timer;

    bool matchStarted = false;
    PlayerManager localPlayer;
    float score;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (!matchStarted) return;
        if (stream.IsWriting) {
            stream.SendNext(timer);
        } else {
            this.timer = (float)stream.ReceiveNext();
        }
    }


    // Start is called before the first frame update
    void Start() {
        FreeForAllGameManager.Instance = this;

        localPlayer = PlayerManager.LocalInstance;

        FreeForAllGamePanel.SetActive(true);

        playerScores = new Dictionary<string, float>();

        var ts = TimeSpan.FromSeconds(TimerSeconds);
        timerText.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
        scoreText.text = "Point victory: "+PointLimit;
    }

    void FixedUpdate() {
        if (!matchStarted) return;
        var ts = TimeSpan.FromSeconds(timer);
        timerText.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);

        if (score >= PointLimit) {
            photonView.RPC("EndMatch", RpcTarget.All, 1);
        }
    }


    public void StartGame() {
        if (matchStarted) return;
        photonView.RPC("SendGameStart", RpcTarget.All);
    }

    [PunRPC]
    public void SendGameStart() {
        if (matchStarted) return;

        if (localPlayer == null) localPlayer = PlayerManager.LocalInstance;
        PlayerManager[] ps = FindObjectsOfType<PlayerManager>();
        playerScores.Clear();
        foreach( PlayerManager player in ps ) {
            playerScores.Add(player.GetUniqueName(), 0f);
            if (player != localPlayer) {
                player.SetNameColor(enemyColor);
                player.SetPlayerOutline(enemyColor);
            }
        }

        SpawnLocalPlayer();

        ObjectiveSphere[] objectiveSpheres = FindObjectsOfType<ObjectiveSphere>();
        foreach( ObjectiveSphere os in objectiveSpheres) os.Reset(); 

        matchStarted = true;

        // Start the timer if we are the master client
        if (photonView.IsMine) StartCoroutine(StartTimer());

        readyButton.SetActive(false);
        scoreText.text = "Score: 0";

        foreach(var obj in ToggleObjects) obj.SetActive(!obj.activeInHierarchy);

        if (MatchMusic != null) MatchMusic.Play();
    }

    IEnumerator StartTimer() {
        timer = TimerSeconds;
        while (timer > 0f) {
            yield return new WaitForFixedUpdate();
            timer -= Time.deltaTime;
        }
        photonView.RPC("EndMatch", RpcTarget.All, 0);
    }


    public void playerDeath(PlayerManager player) {
        StartCoroutine(RespawnPlayer(player));
    }

    public void localPlayerDeath(string killerID) {
        photonView.RPC("SendKillEvent", RpcTarget.All, killerID);
    }

    [PunRPC]
    public void SendKillEvent(string killerID) {
        if (matchStarted) {
            Debug.Log("Player ["+killerID+"] got a kill!");
            if (localPlayer == null) localPlayer = PlayerManager.LocalInstance;

            if (killerID == localPlayer.GetUniqueName()){
                score += PointsForKill;
                scoreText.text = "Score: "+score;
                photonView.RPC("SendScore", RpcTarget.All, localPlayer.GetUniqueName(), score);
            }
        }
    }

    public void ObjectiveScore(string playerName) {
        if (!matchStarted) return;
        photonView.RPC("SendScore", RpcTarget.All, playerName, playerScores[playerName]+PointsForObjective);
    }

    [PunRPC]
    public void SendScore(string playerID, float remoteScore){
        Debug.Log("Set player ["+playerID+"] score to: "+remoteScore);
        if (playerID == localPlayer.GetUniqueName()) {
            // Don't set your score if it's lower than what you currently have.
            if (remoteScore < score) return;
            score = remoteScore;
            playerScores[playerID] = remoteScore;
            scoreText.text = "Score: "+score;
        } else {
            playerScores[playerID] = remoteScore;
        }
        
    }


    public void SpawnLocalPlayer() {
        if (localPlayer == null) localPlayer = PlayerManager.LocalInstance;
        int spawnPoint = UnityEngine.Random.Range (0, Spawnpoints.Count);
        localPlayer.Teleport(Spawnpoints[spawnPoint]);
        localPlayer.HardReset();
    }

    IEnumerator RespawnPlayer(PlayerManager player) {
        yield return new WaitForSeconds(RespawnTimer);
        player.Respawn();

        int spawnPoint = UnityEngine.Random.Range (0, Spawnpoints.Count);
        player.Teleport(Spawnpoints[spawnPoint]);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) {
        if (PhotonNetwork.IsMasterClient) photonView.RPC("EndMatch", RpcTarget.All, 2);
    }

    [PunRPC]
    public void EndMatch(int reason) {
        if (!matchStarted) return;

        //Display the results
        resultsPanel.SetActive(true);

        switch(reason) {
            case 0:
                ReasonText.text = "TIME UP";
                break;
            case 1:
                ReasonText.text = "POINT VICTORY";
                break;
            case 2:
                ReasonText.text = "PLAYER ABANDONED";
                break;
        }

        List<string> winners = new List<string>();
        float winningScore = 0f;
        foreach(string key in playerScores.Keys) {
            if (playerScores[key] > winningScore) {
                winningScore = playerScores[key];
            }
        }
        foreach(string key in playerScores.Keys) {
            if (playerScores[key] == winningScore) {
                winners.Add(key);
            }
        }
        if (winners.Contains(localPlayer.GetUniqueName())) {
            VictoryText.gameObject.SetActive(true);
            DefeatText.gameObject.SetActive(false);
        } else {
            VictoryText.gameObject.SetActive(false);
            DefeatText.gameObject.SetActive(true);
        }
        
        if (winners.Count > 1) {
            WinnerText.text = "Tied Winners: "+System.String.Join(", ", winners)+" - "+winningScore+"pts";
        } else {
            WinnerText.text = "Winner: "+winners[0]+" - "+winningScore+"pts";
        }

        List<string> secondPlace = new List<string>();
        float secondPlaceScore = 0f;
        if (playerScores.Count > 1 && (winners.Count < playerScores.Count)) {
            
            SecondPlaceText.gameObject.SetActive(true);
            foreach(string key in playerScores.Keys) {
                if (playerScores[key] < winningScore && playerScores[key] > secondPlaceScore) {
                    secondPlaceScore = playerScores[key];
                }
            }
            foreach(string key in playerScores.Keys) {
                if (playerScores[key] == secondPlaceScore) {
                    secondPlace.Add(key);
                }
            }
            
            if (secondPlace.Count > 1) {
                SecondPlaceText.text = "Tied second place: "+System.String.Join(", ", secondPlace)+" - "+secondPlaceScore+"pts";
            } else {
                SecondPlaceText.text = "Second place: "+secondPlace[0]+" - "+secondPlaceScore+"pts";
            }
        } else {
            SecondPlaceText.gameObject.SetActive(false);
        }

        List<string> thirdPlace = new List<string>();
        float thirdPlaceScore = 0f;
        if (playerScores.Count > 2 && (winners.Count + secondPlace.Count < playerScores.Count)) {
            ThirdPlaceText.gameObject.SetActive(true);
            
            foreach(string key in playerScores.Keys) {
                if (playerScores[key] < secondPlaceScore && playerScores[key] > thirdPlaceScore) {
                    thirdPlaceScore = playerScores[key];
                }
            }
            foreach(string key in playerScores.Keys) {
                if (playerScores[key] == thirdPlaceScore) {
                    thirdPlace.Add(key);
                }
            }
            
            if (thirdPlace.Count > 1) {
                ThirdPlaceText.text = "Tied third place: "+System.String.Join(", ", thirdPlace)+" - "+thirdPlaceScore+"pts";
            } else {
                ThirdPlaceText.text = "Third place: "+thirdPlace[0]+" - "+thirdPlaceScore+"pts";
            }
        } else {
            ThirdPlaceText.gameObject.SetActive(false);
        }
        

        PlayerManager[] ps = FindObjectsOfType<PlayerManager>();

        readyButton.SetActive(true);

        SpawnLocalPlayer();

        matchStarted = false;
        score = 0f;
        scoreText.text = "Score: "+score;
        playerScores.Clear();
        foreach(var obj in ToggleObjects) obj.SetActive(!obj.activeInHierarchy);
        if (MatchMusic != null) MatchMusic.Stop();
    }
}
