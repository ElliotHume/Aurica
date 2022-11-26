using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using PlayFab;
using PlayFab.ClientModels;

public class ExpertiseManager : MonoBehaviour {

    public static int MAXIMUM_EXPERTISE = 45;
    public static int MINIMUM_EXPERTISE = 0;

    public static int JOURNEYMAN_EXPERTISE = 15;
    public static int MASTER_EXPERTISE = 30;
    public static int ARCHMAGUS_EXPERTISE = 45;

    public static ExpertiseManager Instance;

    private int expertise = 0;
    private bool fetched = false, fetching = false;

    void Start() {
        ExpertiseManager.Instance = this;
    }

    void FixedUpdate() {
        if (!fetched && !fetching) FetchExpertise();
    }

    public int GetExpertise() {
        if (!fetched) return -1;
        return expertise;
    }

    private void FetchExpertise() {
        fetching = true;
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnError);
    }

    void OnDataRecieved (GetUserDataResult result) {
        fetching = false;
        if (result.Data != null && result.Data.ContainsKey("Expertise")) {
            expertise = int.Parse(result.Data["Expertise"].Value);
            fetched = true;
            Debug.Log("Fetched expertise from cloud: ["+expertise+"]");
        } else {
            SetExpertise(0);
            expertise = 0;
        }
        if (RewardsUIPanel.Instance != null) RewardsUIPanel.Instance.SetExpertiseValue();
        if (AuricaCaster.LocalCaster != null) AuricaCaster.LocalCaster.RecalculateExpertise(expertise);
        if (PlayerManager.LocalInstance != null) PlayerManager.LocalInstance.SendExpertise(expertise);
    }

    public void SetExpertise(int newExpertise) {
        var request = new UpdateUserDataRequest {
            Data = new Dictionary<string, string> {
                {"Expertise", newExpertise.ToString()}
            }
        };
        expertise = newExpertise;
        PlayFabClientAPI.UpdateUserData(request, OnExpertiseDataSend, OnError);
        fetched = false;
        FetchExpertise();
    }

    void OnExpertiseDataSend(UpdateUserDataResult result) {
        Debug.Log("Expertise Sent to Cloud");
        FetchExpertise();
    }

    void OnError(PlayFabError error) {
        Debug.LogError(error.GenerateErrorReport());
    }
}
