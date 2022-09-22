using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;

public class PlayerUIInfo : MonoBehaviour {

    public static PlayerUIInfo Instance;

    public Text playerNameText, playerTitleText;
    public TMP_Text statusEffectText;

    public string playerTitle, playerTitleColour;

    PlayerManager target;

    void Start() {
        PlayerUIInfo.Instance = this;
    }

    void FixedUpdate() {
        if (target == null) {
            if (PlayerManager.LocalInstance == null) return;
            target = PlayerManager.LocalInstance;
            playerNameText.text = target.photonView.Owner.NickName;
            FetchPlayerTitle();
        }

        List<string> statusEffects = new List<string>();
        if (target.stunned) statusEffects.Add("STUNNED");
        if (target.silenced) statusEffects.Add("SILENCED");
        if (target.rooted) statusEffects.Add("ROOTED");
        if (target.grounded) statusEffects.Add("GROUNDED");
        if (target.slowed) statusEffects.Add("SLOWED");
        if (target.hastened) statusEffects.Add("HASTENED");
        if (target.fragile) statusEffects.Add("FRAGILE");
        if (target.tough) statusEffects.Add("TOUGH");
        if (target.strengthened) statusEffects.Add("STRONG");
        if (target.weakened) statusEffects.Add("WEAK");
        if (target.slowFall) statusEffects.Add("SLOW FALL");
        if (target.camouflaged) statusEffects.Add("CAMOUFLAGED");
        if (target.manaRestorationChange) {
            if (target.manaRestorationBuff) {
                statusEffects.Add("MANA RESTORATION");
            } else {
                statusEffects.Add("MANA DAMPENING");
            }
        }

        if (statusEffects.Count == 0) {
            ResetStatusEffects();
            return;
        }

        string combinedStatusEffects = "";
        foreach(string status in statusEffects) {
            combinedStatusEffects += " & "+status;
        }
        combinedStatusEffects = combinedStatusEffects.Substring(2) + "...";
        SetStatusEffect(combinedStatusEffects);
    }

    public void SetStatusEffect(string status) {
        statusEffectText.text = status;
    }

    public void ResetStatusEffects() {
        statusEffectText.text = "";
    }

    // Fetch player title from Playfab
    public void FetchPlayerTitle() {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnError);
    }

    void OnDataRecieved (GetUserDataResult result) {
        if (result.Data == null || !result.Data.ContainsKey("Title")) {
            playerTitleText.text = "";
            return;
        }
        playerTitle = result.Data["Title"].Value;
        playerTitleText.text = playerTitle;

        if (result.Data.ContainsKey("TitleColour")) {
            playerTitleColour = result.Data["TitleColour"].Value;

            string[] colourSeperator = new string[] { ", " };
            string[] splitColour = playerTitleColour.Split(colourSeperator, System.StringSplitOptions.None);
            Color newColor = new Color(float.Parse(splitColour[0])/255f, float.Parse(splitColour[1])/255f, float.Parse(splitColour[2])/255f);
            playerTitleText.color = newColor;
        }

        PlayerManager.LocalInstance.SendTitle(playerTitle, playerTitleColour);
    }

    void OnError(PlayFabError error) {
        Debug.LogError(error.GenerateErrorReport());
    }
}
