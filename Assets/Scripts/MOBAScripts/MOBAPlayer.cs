using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MOBAPlayer : MonoBehaviour {

    public static MOBAPlayer LocalPlayer;

    public Color AllyOutlineColor = Color.green;
    public Color EnemyOutlineColor = Color.red;

    private MOBATeam team = null;
    public MOBATeam Team {
        get { return team; }
        set { team = value; }
    }

    private MOBATeam.Team side;
    public MOBATeam.Team Side {
        get { return side; }
        set { side = value; }
    }
    
    private PlayerManager playerManager;
    public PlayerManager GetPlayerManager{
        get { return playerManager; }
    }

    private CharacterMaterialManager materialManager;

    private bool isLocalPlayer;
    public bool IsLocalPlayer {
        get { return isLocalPlayer; }
    }

    public static MOBAPlayer GetMOBAPlayerFromID(string PlayerID) {
        MOBAPlayer[] players = FindObjectsOfType<MOBAPlayer>();
        return Array.Find(players, player => player.GetUniqueName() == PlayerID);
    }

    public static MOBAPlayer GetMOBAPlayerFromPlayerManager(PlayerManager pm) {
        MOBAPlayer[] players = FindObjectsOfType<MOBAPlayer>();
        return Array.Find(players, player => player.GetPlayerManager == pm);
    }
    
    // Start is called before the first frame update
    void Start() {
        side = MOBATeam.Team.None;
        playerManager = GetComponent<PlayerManager>();
        isLocalPlayer = playerManager == PlayerManager.LocalInstance;
        if (isLocalPlayer) MOBAPlayer.LocalPlayer = this;
    }

    public void SetSideColor(bool clear = false) {
        if (!isLocalPlayer) {
            if (clear) {
                playerManager.ResetPlayerOutline();
            } else if (side == MOBAPlayer.LocalPlayer.Side) {
                playerManager.SetPlayerOutline(AllyOutlineColor);
            } else {
                playerManager.SetPlayerOutline(EnemyOutlineColor);
            }
        }
    }

    public void Reset() {
        playerManager.HardReset();
    }

    public void Teleport(Transform anchor) {
        playerManager.Teleport(anchor);
    }

    public void FuzzyTeleport(Transform anchor) {
        playerManager.FuzzyTeleport(anchor);
    }

    public string GetUniqueName() {
        return playerManager.GetUniqueName();
    }
}
