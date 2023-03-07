using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using PlayFab;
using PlayFab.ClientModels;

public class RankedPlayerManager : MonoBehaviour {

    public static RankedPlayerManager Instance;
    private bool fetched = false, fetching = false;

    private float RankedElo = -1f, MOBAElo = -1f;

    void Start() {
        RankedPlayerManager.Instance = this;
    }

    void FixedUpdate() {
        if (!fetched && !fetching) GetRankedData();
    }

    public void GetRankedData() {
        fetching = true;
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnError);
    }

    void OnDataRecieved (GetUserDataResult result) {
        fetching = false;
        if (result.Data != null && result.Data.ContainsKey("RankedElo")) {
            RankedElo = float.Parse(result.Data["RankedElo"].Value);
            fetched = true;
            Debug.Log("Fetched ranked elo from cloud: ["+RankedElo+"]");
        } else {
            SetElo(EloSystem.InitialElo);
        }
    }

    public void SetElo(float newElo) {
        var request = new UpdateUserDataRequest {
            Data = new Dictionary<string, string> {
                {"RankedElo", (Mathf.Round(newElo * 10000f) / 10000f).ToString()}
            }
        };
        RankedElo = newElo;
        PlayFabClientAPI.UpdateUserData(request, OnEloSet, OnError);
        fetched = false;
        GetRankedData();
    }

    void OnEloSet(UpdateUserDataResult result) {
        Debug.Log("[OVERWRITE] Updated Ranked Elo Sent to Cloud : "+RankedElo);
        GetRankedData();
    }

    public void UpdateElo(float opponentElo, bool didIWin) {
        float newElo = EloSystem.EloRating(RankedElo, opponentElo, didIWin);
        var request = new UpdateUserDataRequest {
            Data = new Dictionary<string, string> {
                {"RankedElo", newElo.ToString()},
            }
        };
        PlayFabClientAPI.UpdateUserData(request, OnEloUpdated, OnError);
    }

    void OnEloUpdated(UpdateUserDataResult result) {
        Debug.Log("Elo updated on cloud: "+RankedElo);
        GetRankedData();
    }

    void OnError(PlayFabError error) {
        Debug.LogError(error.GenerateErrorReport());
    }
}
