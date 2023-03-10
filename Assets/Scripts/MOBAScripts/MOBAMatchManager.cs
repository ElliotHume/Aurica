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

    [Tooltip("Player spawn anchor for players that join that are not part of a match")]
    [SerializeField]
    private Transform ObserverRoomAnchor;

    [Tooltip("Player spawn anchor for when the match is not started")]
    [SerializeField]
    private Transform GameStartSpawnAnchor;

    [Tooltip("List of all Novus structures")]
    [SerializeField]
    private List<Structure> NovusStructures;

    [Tooltip("List of all Elden structures")]
    [SerializeField]
    private List<Structure> EldenStructures;

    [Tooltip("How long to wait before ending the match after a nexus is destroyed")]
    [SerializeField]
    private float GameEndDelay;

    [Tooltip("Music to play during the match")]
    [SerializeField]
    private AudioSource MatchMusic;
    

    private float timer = 0f;
    private bool matchStarted = false, matchEnded = false;
    private MOBATeam.Team winningTeam = MOBATeam.Team.None;
    private List<Structure> AllStructures;

    
     /* HEADLINE: --------- PHOTON IPUNOBSERVABLE---------------------------------------------------------------------------------------------------------*/
    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            // CRITICAL DATA
            stream.SendNext(timer);
        } else {
            // CRITICAL DATA
            this.timer = (float)stream.ReceiveNext();
        }
    }

     /* HEADLINE: --------- MONOBEHAVIOUR METHODS---------------------------------------------------------------------------------------------------------*/
    // Awake is called when the object is instantiated
    void Awake() {
        MOBAMatchManager.Instance = this;
    }

    // Start is called before the first frame update
    void Start() {
        AllStructures = new List<Structure>();
        AllStructures.AddRange(NovusStructures);
        AllStructures.AddRange(EldenStructures);
    }

    void FixedUpdate() {
        if (photonView.IsMine) {
            if (matchStarted && !matchEnded) {
                timer += Time.deltaTime;
            } else {
                timer = 0f;
            }
        } else {

        }
    }

    void Update() {
        if (photonView.IsMine && Input.GetKeyDown("u")) {
            NetworkMasterStartMatch();
        }
        if (Input.GetKey("d") && Input.GetKey("e") && Input.GetKeyDown("b")) {
            Debug.LogError("DEBUG --> Master client: "+photonView.IsMine);
            Debug.LogError("timer: "+timer);
            Debug.LogError("matchStarted: "+matchStarted+"   matchEnded: "+matchEnded);
            Debug.LogError("winning team: "+winningTeam.ToString());
            string novusplayers = "";
            foreach(MOBAPlayer p in NovusTeam.GetPlayers()) { novusplayers += p.GetUniqueName()+" ";}
            Debug.LogError("Novus players: "+novusplayers);
            string eldenplayers = "";
            foreach(MOBAPlayer p in EldenTeam.GetPlayers()) { eldenplayers += p.GetUniqueName()+" ";}
            Debug.LogError("Elden players: "+eldenplayers);
        }
    }







    /* HEADLINE: --------- MATCH START & STOP HANDLING---------------------------------------------------------------------------------------------------------*/

    // CLIENT call this method to join a team, the request is then sent to the master client to apply the changes
    public void NetworkClientJoinTeam(MOBATeam.Team Side) {
        string playerId = MOBAPlayer.LocalPlayer.GetUniqueName();
        photonView.RPC("MasterAddPlayerToTeam", RpcTarget.All, Side.ToString(), playerId);
    }

    // MASTER client recieves request to move player to a team
    [PunRPC]
    public void MasterAddPlayerToTeam(string teamName, string playerId) {
        if (photonView.IsMine || matchStarted) return;
        MOBAPlayer player = MOBAPlayer.GetMOBAPlayerFromID(playerId);
        if (teamName == MOBATeam.Team.Novus.ToString()) {
            NovusTeam.AddPlayer(player);
        } else {
            EldenTeam.AddPlayer(player);
        }
    }

    // MASTER client call to start the match for everyone
    public void NetworkMasterStartMatch() {
        if (!photonView.IsMine || matchStarted) return;
        // Restore all structures and reset immunity
        foreach( Structure structure in AllStructures) {
            structure.NetworkRestoreStructure();
            structure.NetworkResetImmunity();
        }

        // Get All MOBA players in the lobby, if they are not on a team, assign them to one
        MOBAPlayer[] allPlayers = FindObjectsOfType<MOBAPlayer>();
        foreach(MOBAPlayer player in allPlayers) {
            if (player.Side != MOBATeam.Team.None && (NovusTeam.GetPlayers().Contains(player) || EldenTeam.GetPlayers().Contains(player))) {
                // Player has already been assigned to a team, do nothing
                continue;
            } else {
                if (NovusTeam.GetPlayerCount() <= EldenTeam.GetPlayerCount()) {
                    NovusTeam.AddPlayer(player);
                } else {
                    EldenTeam.AddPlayer(player);
                }
            }
        }

        // Reset all null spheres
        NullSphere[] nullSpheres = FindObjectsOfType<NullSphere>();
        foreach(NullSphere sphere in nullSpheres) { sphere.NetworkMasterReset(); }

        // Format teams into a list of players to send to clients
        string novusTeamPlayerNames = "";
        string eldenTeamPlayerNames = "";
        foreach(MOBAPlayer player in NovusTeam.GetPlayers()) { if (player != null) novusTeamPlayerNames += player.GetUniqueName()+"|"; }
        foreach(MOBAPlayer player in EldenTeam.GetPlayers()) { if (player != null) eldenTeamPlayerNames += player.GetUniqueName()+"|"; }

        photonView.RPC("ClientStartMatch", RpcTarget.All, novusTeamPlayerNames, eldenTeamPlayerNames);
    }

    // CLIENT response to NetworkStartMatch
    // Even the master client will respond to this in the same way, to ensure a clarity of state
    [PunRPC]
    public void ClientStartMatch(string novusTeamPlayerNames, string eldenTeamPlayerNames) {
        Debug.LogError("START MATCH\nNOVUS TEAM: "+novusTeamPlayerNames+"\nELDEN TEAM: "+eldenTeamPlayerNames);
        MOBAPlayer[] allPlayers = FindObjectsOfType<MOBAPlayer>();
        NovusTeam.ClearPlayers();
        EldenTeam.ClearPlayers();
        foreach(MOBAPlayer player in allPlayers) {
            if (novusTeamPlayerNames.Contains(player.GetUniqueName())) {
                // Add the player to the Novus team
                NovusTeam.AddPlayer(player);
                // These two methods will only be run by the owner of the player
                player.Reset();
                player.FuzzyTeleport(NovusRespawnAnchor);
            } else if (eldenTeamPlayerNames.Contains(player.GetUniqueName())) {
                // Add the player to the Elden team
                EldenTeam.AddPlayer(player);
                // These two methods will only be run by the owner of the player
                player.Reset();
                player.FuzzyTeleport(EldenRespawnAnchor);
            } else {
                Debug.LogError("Player not placed in a team: "+player.GetUniqueName());
                player.Reset();
                player.FuzzyTeleport(ObserverRoomAnchor);
            }
        }

        NovusTeam.LockedTeamList = novusTeamPlayerNames;
        EldenTeam.LockedTeamList = eldenTeamPlayerNames;

        // Set the structure team colors
        foreach( Structure structure in AllStructures) {
            structure.SetColors();
        }

        // Destroy all spells in the scene, they must be destroyed by their owner
        Spell[] foundSpells = FindObjectsOfType<Spell>();
        foreach(Spell spell in foundSpells) {
            if (spell.photonView.IsMine) PhotonNetwork.Destroy(spell.photonView);
        }
        
        // Set the team outline colors for all players
        foreach(MOBAPlayer player in allPlayers) player.SetSideColor();
        if (MatchMusic != null) MatchMusic.Play();
        matchStarted = true;
        matchEnded = false;
    }

    // MASTER client call to stop the match for everyone
    public void NetworkMasterStopMatch() {
        if (!photonView.IsMine || !matchStarted || matchEnded) return;
        photonView.RPC("ClientStopMatch", RpcTarget.All);
    }

    // CLIENT response to NetworkStopMatch
    [PunRPC]
    public void ClientStopMatch() {
        matchStarted = false;
        matchEnded = true;

        MOBAPlayer[] allPlayers = FindObjectsOfType<MOBAPlayer>();
        foreach(MOBAPlayer player in allPlayers) {
            player.Reset();
            player.FuzzyTeleport(GameStartSpawnAnchor);
        }

        NovusTeam.LockedTeamList = null;
        EldenTeam.LockedTeamList = null;

        // Destroy all spells in the scene, they must be destroyed by their owner
        Spell[] foundSpells = FindObjectsOfType<Spell>();
        foreach(Spell spell in foundSpells) {
            if (spell.photonView.IsMine) PhotonNetwork.Destroy(spell.photonView);
        }
        if (MatchMusic != null) MatchMusic.Stop();
    }


    /* HEADLINE: --------- PLAYER CONNECTION HANDLING ---------------------------------------------------------------------------------------------------------*/

    // CLIENT calls this when they join the room
    public void NetworkClientPlayerJoined(string playerId) {
        // Debug.LogError("Client sending player join");
        photonView.RPC("MasterPlayerJoined", RpcTarget.All, playerId);
    }



    /* MAJORTODO: Make players able to rejoin matches that they have disconnected from
    *    PREREQUISITE: Make Null Spheres network synced
    *   - Master checks current locked team lists, if the rejoining players belongs in one of them the master adds them to their MOBATeam
    *   - Master removes any null players from the MOBATeams
    *   - Master generates new team list strings, if the updated team strings are not the same as the previous locked team lists, an error has occured.
    *   - Master sends out an update request to all clients if the lists are the same.
    *   - Clients recieve the update request and refresh the players in their MOBATeams to match their locked team lists
    *   - Clients refresh their structure colors and player outline colors
    *
    *   [NOT SURE IF NEEDED SECTION]
    *   - Clients check if they are the rejoining player, if they are, they send a request for the state of all structures and null spheres
    *   - Master recieves request, sends the state of structures and null spheres
    */
    
    // MASTER determine what to do with the joined player
    [PunRPC]
    void MasterPlayerJoined(string playerId) {
        if (!photonView.IsMine) return;
        // Debug.LogError("Master receieved player join");
        if (matchStarted) {
            photonView.RPC("MovePlayerToObserverZone", RpcTarget.All, playerId);
        } else {
            // Reset all null spheres
            NullSphere[] nullSpheres = FindObjectsOfType<NullSphere>();
            foreach(NullSphere sphere in nullSpheres) { sphere.NetworkMasterReset(); }
        }
    }

    // CLIENT if the player is under this clients control, move player to the observer zone and update their state
    [PunRPC]
    void MovePlayerToObserverZone(string playerId) {
        MOBAPlayer player = MOBAPlayer.GetMOBAPlayerFromID(playerId);
        if (player == MOBAPlayer.LocalPlayer) {
            player.FuzzyTeleport(ObserverRoomAnchor);
            matchStarted = true;
        }
    }








    /* HEADLINE: --------- MATCH OBJECTIVE HANDLING ---------------------------------------------------------------------------------------------------------*/

    // MASTER
    public void NetworkMasterNexusBroken(MOBATeam.Team Side) {
        if (!photonView.IsMine) return;
        photonView.RPC("ClientNexusBroken", RpcTarget.All, Side.ToString());
    
        // Grant immunity to all the structures of the winning team, remove it from the losing team's structures
        if (Side == MOBATeam.Team.Novus) {
            foreach(Structure structure in EldenStructures) { structure.NetworkSetImmunity(true); }
            foreach(Structure structure in NovusStructures) { structure.NetworkSetImmunity(false); }
        } else {
            foreach(Structure structure in NovusStructures) { structure.NetworkSetImmunity(true); }
            foreach(Structure structure in EldenStructures) { structure.NetworkSetImmunity(false); }
        }

        Invoke("NetworkMasterStopMatch", GameEndDelay);
    }

    // MASTER
    public void NetworkMasterTowerBroken(MOBATeam.Team Side) {
        if (!photonView.IsMine) return;
        photonView.RPC("ClientTowerBroken", RpcTarget.All, Side.ToString());
    }

    //CLIENT
    [PunRPC]
    public void ClientNexusBroken(string side) {
        winningTeam = (MOBATeam.Team)System.Enum.Parse( typeof(MOBATeam.Team), side );
        Debug.Log("["+side+"] team Nexus destroyed!");
        // TODO: Play announcement of nexus destruction
    }

    //CLIENT
    [PunRPC]
    public void ClientTowerBroken(string side) {
        MOBATeam.Team towerTeam = (MOBATeam.Team)System.Enum.Parse( typeof(MOBATeam.Team), side );
        Debug.Log("["+side+"] team Tower destroyed!");
        // TODO: Play announcement of tower destruction
    }













    /* HEADLINE: --------- PLAYER DEATH HANDLING ---------------------------------------------------------------------------------------------------------*/

    // CLIENT
    public void PlayerDeath(PlayerManager player) {
        if (player == PlayerManager.LocalInstance) {
            // TODO: Increase client death count
        }
        StartCoroutine(RespawnPlayer(MOBAPlayer.GetMOBAPlayerFromPlayerManager(player)));
    }

    // CLIENT
    public void NetworkPlayerDeath(string killerID) {
        photonView.RPC("SendKillEvent", RpcTarget.All, killerID);
    }

    // CLIENT
    [PunRPC]
    public void SendKillEvent(string killerID) {
        if (matchStarted) {
            Debug.Log("Player ["+killerID+"] got a kill!");
            if (killerID == MOBAPlayer.LocalPlayer.GetUniqueName()){
                // TODO: Increase client kill count
            }
        }
    }

    // CLIENT
    IEnumerator RespawnPlayer(MOBAPlayer player) {
        float respawnTimer = matchStarted ? 2f + (timer/15f) : 1f;
        yield return new WaitForSeconds(respawnTimer);
        Transform spawnPoint = (player.Side == MOBATeam.Team.Novus) ? NovusRespawnAnchor : EldenRespawnAnchor;
        player.Teleport(spawnPoint);
        player.GetPlayerManager.Respawn();
    }
}
