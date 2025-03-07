using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MOBATeam : MonoBehaviour {

    public enum Team {
       Novus, Elden, None
    };

    [Tooltip("Which team this object is the manager for.")]
    [SerializeField]
    private Team Side;

    private List<MOBAPlayer> Players;
    private float PlayerCount;
    private string lockedTeamList;
    public string LockedTeamList {
        get { return lockedTeamList; }
        set { lockedTeamList = value; }
    }

    void Start() {
        Players = new List<MOBAPlayer>();
    }

    public void AddPlayer(MOBAPlayer newPlayer) {
        Players.Add(newPlayer);
        newPlayer.Team = this;
        newPlayer.Side = Side;
        PlayerCount = Players.Count;
    }

    public void RemovePlayer(MOBAPlayer newPlayer) {
        Players.Remove(newPlayer);
        newPlayer.Team = null;
        newPlayer.Side = Team.None;
        PlayerCount = Players.Count;
    }
    
    public void SetPlayers(List<MOBAPlayer> newPlayers) {
        Players = new List<MOBAPlayer>(newPlayers);
        foreach(MOBAPlayer player in Players) {
            player.Team = this;
            player.Side = Side;
        }
        PlayerCount = Players.Count;
    }

    public List<MOBAPlayer> GetPlayers() {
        return Players;
    }

    public bool IsADisconnectedPlayer() {
        foreach(MOBAPlayer player in Players) {
            if (player == null) return true;
        }
        return false;
    }

    public void ClearDisconnectedPlayers() {
        while (IsADisconnectedPlayer()) {
            foreach(MOBAPlayer player in Players) {
                if (player == null) Players.Remove(player);
                break;
            }
        }
    }

    public void ClearPlayers() {
        foreach(MOBAPlayer player in Players) {
            player.Team = null;
            player.Side = Team.None;
        }
        Players.Clear();
    }

    public Team GetSide() {
        return Side;
    }

    public Team GetOpposingSide() {
        if (Side == Team.Novus) {
            return Team.Elden;
        }
        return Team.Novus;
    }

    public float GetPlayerCount() {
        return PlayerCount;
    }
}
