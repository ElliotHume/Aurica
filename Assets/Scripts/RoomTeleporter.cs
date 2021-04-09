using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomTeleporter : MonoBehaviourPunCallbacks
{

    public string NetworkRoomName = "Default_GATE", SceneName = "Deathmatch";
    public bool onCollide = true;

    void OnTriggerEnter(Collider other) {
        if (!onCollide) return;

        if (other.gameObject.tag == "Player" && other.gameObject == PlayerManager.LocalPlayerInstance) {
            PlayerManager pm = other.gameObject.GetComponent<PlayerManager>();
            PhotonNetwork.LoadLevel(SceneName);
            //RoomOptions roomOptions = new RoomOptions();
            //PhotonNetwork.JoinOrCreateRoom(NetworkRoomName, roomOptions, TypedLobby.Default);
        }
    }
}
