using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class MOBAUIMenu : MonoBehaviour {

    public static MOBAUIMenu Instance;

    [Tooltip("Prefab to instantiate when a player joins a team")]
    [SerializeField]
    private GameObject TeamPlayerListDisplayPrefab;

    [Tooltip("MOBA HUD display")]
    [SerializeField]
    private MOBAUIDisplay HUDDisplay;

    [Tooltip("Novus team player list")]
    [SerializeField]
    private GameObject NovusTeamPlayerList;

    [Tooltip("Elden team player list")]
    [SerializeField]
    private GameObject EldenTeamPlayerList;

    [Tooltip("Button to join Novus team")]
    [SerializeField]
    private Button JoinNovusTeamButton;

    [Tooltip("Button to join Elden team")]
    [SerializeField]
    private Button JoinEldenTeamButton;

    [Tooltip("Button to start the match, only the master client will see this")]
    [SerializeField]
    private GameObject StartMatchButton;

    private MOBAMatchManager matchManager;
    private List<GameObject> playerListInstances = new List<GameObject>();
    private bool requestPending = false;

    void Awake() {
        MOBAUIMenu.Instance = this;
    }

    void OnEnable() {
        matchManager = MOBAMatchManager.Instance;
        if (matchManager == null) {
            gameObject.SetActive(false);
        } else {
            PhotonView pv = PhotonView.Get(matchManager);
            StartMatchButton.SetActive(pv != null && pv.IsMine);

            // Clear player list when enabled
            foreach (GameObject item in playerListInstances) Destroy(item);
        }
    }

    // Clients run this function when they press a join team button
    public void ClientJoinTeam(string teamName) {
        if (matchManager == null) matchManager = MOBAMatchManager.Instance;
        if (matchManager != null && !requestPending) {
            requestPending = true;
            MOBATeam.Team team = (MOBATeam.Team)System.Enum.Parse( typeof(MOBATeam.Team), teamName );
            matchManager.NetworkClientJoinTeam(team);
        }
    }

    public void DisplayTeamPlayers(List<MOBAPlayer> NovusTeamPlayers, List<MOBAPlayer> EldenTeamPlayers) {
        // Remove current player list items
        foreach (GameObject item in playerListInstances) Destroy(item);
        
        // Re-instantiate Novus team player list
        foreach (MOBAPlayer player in NovusTeamPlayers) {
            GameObject newInstance = Instantiate(TeamPlayerListDisplayPrefab, NovusTeamPlayerList.transform.position, NovusTeamPlayerList.transform.rotation, NovusTeamPlayerList.transform);
            playerListInstances.Add(newInstance);
            newInstance.GetComponent<PlayerInfoUIDisplay>().SetPlayer(player);
        }

        // Re-instantiate Elden team player list
        foreach (MOBAPlayer player in EldenTeamPlayers) {
            GameObject newInstance = Instantiate(TeamPlayerListDisplayPrefab, EldenTeamPlayerList.transform.position, EldenTeamPlayerList.transform.rotation, EldenTeamPlayerList.transform);
            playerListInstances.Add(newInstance);
            newInstance.GetComponent<PlayerInfoUIDisplay>().SetPlayer(player);
        }

        // Disable the join team button for the team that the local player is on
        if (MOBAPlayer.LocalPlayer != null) {
            JoinNovusTeamButton.interactable = !NovusTeamPlayers.Contains(MOBAPlayer.LocalPlayer);
            JoinEldenTeamButton.interactable = !EldenTeamPlayers.Contains(MOBAPlayer.LocalPlayer);
        }

        // Join request has been processed, reset pending state
        requestPending = false;
    }

    public void MasterStartMatch() {
        if (matchManager == null) matchManager = MOBAMatchManager.Instance;
        if (matchManager != null) {
            matchManager.NetworkMasterStartMatch();
        }
    }

    public void ClientGameStart() {
        if (matchManager == null) matchManager = MOBAMatchManager.Instance;
        if (matchManager != null) {
            matchManager.NetworkClientCallGameStart();
        }
    }

    public void ToggleStartButton(bool flag) {
        HUDDisplay.ToggleStartButton(flag);
    }
}
