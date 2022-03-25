using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using PlayFab;
using PlayFab.ClientModels;

public class RewardsManager : MonoBehaviour {

    // No player is allowed to have more mana than this threshold.
    public static float MAXIMUM_TOTAL_MANA_THRESHOLD = 500f;

    public static RewardsManager Instance;
    [HideInInspector]
    public float rewardPoints = 0f;
    [HideInInspector]
    public float cosmeticPoints = 0f;

    private bool fetched = false, fetching = false;
    private ManaDistribution modifiedAura;
    private float newExtraMana = 0f;

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
        fetching = false;
        if (result.Data != null && result.Data.ContainsKey("RewardPoints")) {
            rewardPoints = float.Parse(result.Data["RewardPoints"].Value);
            fetched = true;
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
                {"RewardPoints", (Mathf.Round((existingRewards+points) * 10000f) / 10000f).ToString()}
            }
        };
        rewardPoints = existingRewards+points;
        PlayFabClientAPI.UpdateUserData(request, OnRewardsDataSend, OnError);
        fetched = false;
        NotificationText.Instance.ShowCultivationPoints(points);
        GetRewards();
    }

    void OnRewardsDataSend(UpdateUserDataResult result) {
        Debug.Log("Reward points Sent to Cloud : "+rewardPoints);
        GetRewards();
    }

    public void SpendRewardsPoints(ManaDistribution newAuraAdditions, float addMana, float spentRewardPoints) {
        if (spentRewardPoints > rewardPoints+0.0001f) {
            Debug.LogError("Tried to use more reward points than are available! SPENT: ["+spentRewardPoints+"]  AVAILABLE: ["+rewardPoints+"]");
            return;
        }
        if (newAuraAdditions.GetAggregate() == 0f && addMana == 0f) {
            Debug.Log("Tried to spend no reward points, cancelling operation.");
            return;
        }
        ManaDistribution currentAura = new ManaDistribution(PlayerPrefs.GetString("Aura"));

        if (PlayerPrefs.HasKey("ExtraMana")) {
            float currentExtraMana = float.Parse(PlayerPrefs.GetString("ExtraMana"));
            if (currentExtraMana >= MAXIMUM_TOTAL_MANA_THRESHOLD) {
                Debug.LogError("Player is over the mana cap, no more extra mana can be added.  Tried to add ["+addMana+"] to an existing ["+currentExtraMana+"] Mana.");
                return;
            }
            newExtraMana = Mathf.Round((currentExtraMana + addMana) * 1000f) / 1000f;
        } else {
            newExtraMana = Mathf.Round(addMana * 1000f) / 1000f;
        }
        
        modifiedAura = currentAura + newAuraAdditions;
        
        float points = Mathf.Round((rewardPoints-spentRewardPoints) * 1000f) / 1000f;
        var request = new UpdateUserDataRequest {
            Data = new Dictionary<string, string> {
                {"ModifiedAura", modifiedAura.ToString()},
                {"RewardPoints", points.ToString()},
                {"ExtraMana", newExtraMana.ToString()}
            }
        };
        PlayFabClientAPI.UpdateUserData(request, OnNewAuraSend, OnError);
    }

    void OnNewAuraSend(UpdateUserDataResult result) {
        Debug.Log("Modified Aura Sent to Cloud : "+modifiedAura.ToString());
        PlayerPrefs.SetString("ExtraMana", newExtraMana.ToString());
        PlayerManager.LocalInstance.aura.SetAura(modifiedAura);
        PlayerManager.LocalInstance.PlayCultivationEffect();
        GetRewards();
    }

    void OnError(PlayFabError error) {
        Debug.LogError(error.GenerateErrorReport());
    }
}
