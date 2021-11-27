using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using PlayFab;
using PlayFab.ClientModels;

public class RewardsManager : MonoBehaviour {

    public static RewardsManager Instance;
    [HideInInspector]
    public float rewardPoints = 0f;

    private bool fetched = false, fetching = false;
    private ManaDistribution modifiedAura;

    void Start() {
        RewardsManager.Instance = this;
    }

    void FixedUpdate() {
        if (!fetched && !fetching) GetRewards();
    }

    public void GetRewards() {
        fetching = true;
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnError);
    }

    void OnDataRecieved (GetUserDataResult result) {
        if (result.Data != null && result.Data.ContainsKey("RewardPoints")) {
            rewardPoints = float.Parse(result.Data["RewardPoints"].Value);
            fetched = true;
            fetching = false;
            Debug.Log("Fetched reward points from cloud: ["+rewardPoints+"]");
        } else {
            AddRewards(0f);
            rewardPoints = 0f;
        }
        if (RewardsUIPanel.Instance != null) RewardsUIPanel.Instance.SetRewardPoints(rewardPoints);
    }

    public void AddRewards(float points) {
        float existingRewards = rewardPoints; 
        var request = new UpdateUserDataRequest {
            Data = new Dictionary<string, string> {
                {"RewardPoints", (existingRewards+points).ToString()}
            }
        };
        rewardPoints = existingRewards+points;
        PlayFabClientAPI.UpdateUserData(request, OnDataSend, OnError);
        fetched = false;
        GetRewards();
    }

    void OnDataSend(UpdateUserDataResult result) {
        Debug.Log("Reward points Sent to Cloud : "+rewardPoints);
        PlayerManager.LocalInstance.PlayCultivationEffect();
        GetRewards();
    }

    public void SpendRewardsPoints(ManaDistribution newAuraAdditions, float spentRewardPoints) {
        if (spentRewardPoints > rewardPoints+0.0001f) {
            Debug.LogError("Tried to use more reward points than are available! SPENT: ["+spentRewardPoints+"]  AVAILABLE: ["+rewardPoints+"]");
            return;
        }
        ManaDistribution currentAura = new ManaDistribution(PlayerPrefs.GetString("Aura"));
        modifiedAura = currentAura + newAuraAdditions;
        float points = Mathf.Round((rewardPoints-spentRewardPoints) * 1000f) / 1000f;
        var request = new UpdateUserDataRequest {
            Data = new Dictionary<string, string> {
                {"ModifiedAura", modifiedAura.ToString()},
                {"RewardPoints", points.ToString()}
            }
        };
        PlayFabClientAPI.UpdateUserData(request, OnNewAuraSend, OnError);
    }

    void OnNewAuraSend(UpdateUserDataResult result) {
        Debug.Log("Modified Aura Sent to Cloud : "+modifiedAura.ToString());
        PlayerManager.LocalInstance.aura.SetAura(modifiedAura);
        PlayerManager.LocalInstance.PlayCultivationEffect();
        GetRewards();
    }

    void OnError(PlayFabError error) {
        Debug.LogError(error.GenerateErrorReport());
    }
}
