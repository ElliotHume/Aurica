using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class RewardsManager : MonoBehaviour {
    
    public TMP_Text MessageText;


    private float rewardPoints;
    private bool fetched = false;
    
    void Start() {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnError);
    }

    public float GetRewards() {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnError);
        return rewardPoints;
    }

    void OnDataRecieved (GetUserDataResult result) {
        Debug.Log("Recieved player data");

        if (result.Data != null && result.Data.ContainsKey("RewardPoints")) {
            PlayerPrefs.SetString("RewardPoints", result.Data["RewardPoints"].Value);
            rewardPoints = result.Data["RewardPoints"].Value;
            fetched = true;
        } else {
            AddRewards(0f);
            rewardPoints = 0f;
        }
    }

    public void AddRewards(float points) {
        float existingRewards = GetRewards(); 
        var request = new UpdateUserDataRequest {
            Data = new Dictionary<string, string> {
                {"RewardPoints", existingRewards+points}
            }
        };
        PlayFabClientAPI.UpdateUserData(request, OnDataSend, OnError);
    }

    void OnDataSend(UpdateUserDataResult result) {
        MessageText.text = "Reward points saved to cloud services: ["+AuraText+"]";
        Debug.Log("Reward points Sent to Cloud : "+AuraText);
        GetRewards();
    }

    void OnError(PlayFabError error) {
        MessageText.text = error.ErrorMessage;
        Debug.LogError(error.GenerateErrorReport());
    }
}
