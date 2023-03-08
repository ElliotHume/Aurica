using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

public class MOBAMatchManager : MonoBehaviourPun, IPunObservable {

    public static MOBAMatchManager Instance;

    [Tooltip("Novus MOBATeam object")]
    [SerializeField]
    private MOBATeam NovusTeam;

    [Tooltip("Elden MOBATeam object")]
    [SerializeField]
    private MOBATeam EldenTeam;

    [Tooltip("Player respawn anchor for Novus")]
    [SerializeField]
    private Transform NovusRespawnAnchor;

    [Tooltip("Player respawn anchor for Elden")]
    [SerializeField]
    private Transform EldenRespawnAnchor;

    [Tooltip("List of all Novus structures")]
    [SerializeField]
    private List<Structure> NovusStructures;

    [Tooltip("List of all Elden structures")]
    [SerializeField]
    private List<Structure> EldenStructures;

    private float timer = 0f;
    private bool matchStarted = false;

    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            // CRITICAL DATA
            stream.SendNext(timer);
        } else {
            // CRITICAL DATA
            this.timer = (float)stream.ReceiveNext();
        }
    }

    // Awake is called when the object is instantiated
    void Awake() {
        MOBAMatchManager.Instance = this;
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void FixedUpdate() {
        if (photonView.IsMine) {
            if (matchStarted) {
                timer += Time.deltaTime;
            }
        } else {

        }
    }

    public void NetworkStartMatch() {
        if (!photonView.IsMine || matchStarted) return;
        photonView.RPC("LocalStartMatch", RpcTarget.All);
    }

    public void NetworkStopMatch() {
        if (!photonView.IsMine || !matchStarted) return;
        timer = 0f;
        photonView.RPC("LocalStopMatch", RpcTarget.All);
    }

    [PunRPC]
    public void LocalStartMatch() {
        matchStarted = true;

    }

    [PunRPC]
    public void LocalStopMatch() {
        matchStarted = false;
    }

    IEnumerator RespawnPlayer(MOBAPlayer player) {
        float respawnTimer = 30f + (timer/15f);
        yield return new WaitForSeconds(respawnTimer);
        Transform spawnPoint = (player.Side == MOBATeam.Team.Novus) ? NovusRespawnAnchor : EldenRespawnAnchor;
        player.GetPlayerManager.Teleport(spawnPoint);
        player.GetPlayerManager.Respawn();
    }
}
