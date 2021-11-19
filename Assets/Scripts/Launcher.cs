using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks {
    [Tooltip("The Ui Panel to let the user enter name, connect and play")]
    [SerializeField]
    public GameObject controlPanel;
    [Tooltip("The UI Label to inform the user that the connection is in progress")]
    [SerializeField]
    public GameObject progressLabel;

    public InputField roomNameField;
    public Text ArenaText;

    public UnityEvent OnStart;


    string gameVersion = "0.1";
    string roomName = "default", arena = "Default";
    string roomNamePlayerPrefKey = "RoomName";

    /// <summary>
    /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon,
    /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
    /// Typically this is used for the OnConnectedToMaster() callback.
    /// </summary>
    bool isConnecting;

    // Start is called before the first frame update
    void Start() {
        // Connect();

        if (roomNameField != null) {
            if (PlayerPrefs.HasKey(roomNamePlayerPrefKey)) {
                roomName = PlayerPrefs.GetString(roomNamePlayerPrefKey);
                if (roomName != "default") roomNameField.text = roomName;
                if (string.IsNullOrEmpty(roomName)) roomName = "default";
            }
        }

        // Double check that the cursor is not locked or hidden
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Awake() {
        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log("PUN VERSION "+PhotonNetwork.PunVersion);
    }

    public override void OnConnectedToMaster() {
        Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
        // we don't want to do anything if we are not attempting to join a room.
        // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
        // we don't want to do anything.
        if (isConnecting) {
            // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
            RoomOptions roomOptions = new RoomOptions();
            PhotonNetwork.JoinOrCreateRoom(roomName.ToLower(), roomOptions, TypedLobby.Default);
            isConnecting = false;
        }
    }


    public override void OnDisconnected(DisconnectCause cause) {
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
        isConnecting = false;
        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
    }

    public override void OnJoinRandomFailed(short returnCode, string message) {
        Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom("FreePlay", new RoomOptions());
    }

    public override void OnJoinedRoom() {
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
        // #Critical: We only load if we are the first player, else we rely on `PhotonNetwork.AutomaticallySyncScene` to sync our instance scene.
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1) {
            // #Critical
            // Load the Room Level.
            string level = "BattlegroundsDefault";
            switch (arena) {
                case "FreePlay":
                    level = "Battlegrounds1";
                    break;
                case "Default Free for All":
                    level = "BattlegroundsDefault";
                    break;
                case "Multi-level":
                    level = "Deathmatch";
                    break;
                case "Open Field":
                    level = "DeathmatchOpenGround";
                    break;
                case "Default Deathmatch":
                    level = "DeathmatchOpenAdvanced";
                    break;
            }
            Debug.Log("Loading Scene: "+level+" Room name: "+roomName);
            PhotonNetwork.LoadLevel(level);
        }
    }

    /// <summary>
    /// Start the connection process.
    /// - If already connected, we attempt joining a random room
    /// - if not yet connected, Connect this application instance to Photon Cloud Network
    /// </summary>
    public void Connect() {
        if (PhotonNetwork.NickName == string.Empty) {
            return;
        }

        OnStart.Invoke();
        progressLabel.SetActive(true);
        controlPanel.SetActive(false);
        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (!PhotonNetwork.IsConnected) {
            // #Critical, we must first and foremost connect to Photon Online Server.
            isConnecting = PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        } else {
            RoomOptions roomOptions = new RoomOptions();
            PhotonNetwork.JoinOrCreateRoom(roomName.ToLower(), roomOptions, TypedLobby.Default);
        }
    }

    public void FreePlay() {
        roomName = "FreePlay";
        arena = "FreePlay";
        Connect();
    }

    public void SetRoomName(string name) {
        if (string.IsNullOrEmpty(name)) name = "default";
        roomName = name;
        PlayerPrefs.SetString(roomNamePlayerPrefKey, roomName);
    }

    public void SetArena(string arenaName) {
        arena = arenaName;
        ArenaText.gameObject.SetActive(true);
        ArenaText.text = "Arena: "+arenaName;
        Debug.Log("Arena set to: "+arena);
    }
}
