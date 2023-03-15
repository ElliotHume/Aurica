using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoUIDisplay : MonoBehaviour {
    
    [Tooltip("Text field to display the player's name")]
    [SerializeField]
    private Text PlayerName;


    public void SetPlayer(MOBAPlayer player) {
        PlayerName.text = player.GetName();
    }

    public void SetPlayer(PlayerManager player) {
        PlayerName.text = player.GetName();
    }
}
