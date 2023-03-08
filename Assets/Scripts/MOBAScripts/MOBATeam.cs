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

    void Start() {
        Players = new List<MOBAPlayer>();
    }
    
    public void SetPlayers(List<MOBAPlayer> newPlayers) {
        Players = new List<MOBAPlayer>(newPlayers);
        foreach(MOBAPlayer player in Players) {
            player.Team = this;
            player.Side = Side;
        }
    }

    public List<MOBAPlayer> GetPlayers() {
        return Players;
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
}
