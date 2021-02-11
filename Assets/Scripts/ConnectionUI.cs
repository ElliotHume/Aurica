using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class ConnectionUI : MonoBehaviourPunCallbacks {

    public InputField inputField;
    public Text error;

    // Start is called before the first frame update
    void Start() {

    }

    public void JoinOrCreateRoom() {
        GameManager.Instance.LeaveRoom();
        string roomName = inputField.text == "" ? "default" : inputField.text;
        RoomOptions roomOptions = new RoomOptions();
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, null);
    }

    public override void OnJoinRoomFailed(short returnCode, string message) {
        error.text = "Room creation failed with error code " + returnCode + " and error message " + message;
        Debug.LogErrorFormat("Room creation failed with error code {0} and error message {1}", returnCode, message);
    }

    public override void OnJoinedRoom() {
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
        // #Critical: We only load if we are the first player, else we rely on `PhotonNetwork.AutomaticallySyncScene` to sync our instance scene.
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1) {
            // #Critical
            // Load the Room Level.
            PhotonNetwork.LoadLevel("Deathmatch");
        }
    }
}
