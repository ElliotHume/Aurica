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
    
    // Start is called before the first frame update
    void Start() {
        side = MOBATeam.Team.None;
        playerManager = GetComponent<PlayerManager>();
        isLocalPlayer = playerManager == PlayerManager.LocalInstance;
        if (isLocalPlayer) MOBAPlayer.LocalPlayer = this;
    }

    public void SetSideColor() {
        if (!isLocalPlayer) {
            if (side == MOBAPlayer.LocalPlayer.Side) {
                playerManager.SetPlayerOutline(AllyOutlineColor);
            } else {
                playerManager.SetPlayerOutline(EnemyOutlineColor);
            }
        }
    }
}
