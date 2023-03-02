using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using PlayFab;
using PlayFab.ClientModels;

public class MasteryManager : MonoBehaviour {
    public enum MasteryLevel {
        Novice, Adept, Master, Legend
    };
    public static Dictionary<MasteryLevel, int> MasteryThresholds = new Dictionary<MasteryLevel, int>{
        {MasteryLevel.Novice, 10},
        {MasteryLevel.Adept, 100},
        {MasteryLevel.Master, 1000},
        {MasteryLevel.Legend, 10000},
    };

    public enum MasteryCategories {
        Auric, Order, Chaos, Life, Death, Fire, Water, Earth, Air, Divine, Demonic,
        Movement, Battlemage, Support, Defender, Magewright
    };

    public static MasteryManager Instance;

    [HideInInspector]
    public int AuricMastery, OrderMastery, ChaosMastery, LifeMastery, DeathMastery, FireMastery, WaterMastery, EarthMastery, AirMastery, DivineMastery, DemonicMastery;
    [HideInInspector]
    public int MovementMastery, BattlemageMastery, SupportMastery, DefenderMastery, MagewrightMastery;
    [HideInInspector]
    public Dictionary<MasteryCategories, int> Masteries = new Dictionary<MasteryCategories, int>();
    [HideInInspector]
    public bool synced = true;

    public AudioSource MasteryProgressionSound;

    private bool fetched = false, fetching = false;
    private Dictionary<string, MasteryCategories> cloudKeys;
    private Dictionary<AuricaSpell.DifficultyRank, int> MasteryPerRank;
    

    void Start() {
        MasteryManager.Instance = this;
        cloudKeys = new Dictionary<string, MasteryCategories>();
        cloudKeys.Add("AuricMastery", MasteryCategories.Auric);
        cloudKeys.Add("OrderMastery", MasteryCategories.Order);
        cloudKeys.Add("ChaosMastery", MasteryCategories.Chaos);
        cloudKeys.Add("LifeMastery", MasteryCategories.Life);
        cloudKeys.Add("DeathMastery", MasteryCategories.Death);
        cloudKeys.Add("FireMastery", MasteryCategories.Fire);
        cloudKeys.Add("WaterMastery", MasteryCategories.Water);
        cloudKeys.Add("EarthMastery", MasteryCategories.Earth);
        cloudKeys.Add("AirMastery", MasteryCategories.Air);
        cloudKeys.Add("DivineMastery", MasteryCategories.Divine);
        cloudKeys.Add("DemonicMastery", MasteryCategories.Demonic);
        cloudKeys.Add("MovementMastery", MasteryCategories.Movement);
        cloudKeys.Add("BattlemageMastery", MasteryCategories.Battlemage);
        cloudKeys.Add("SupportMastery", MasteryCategories.Support);
        cloudKeys.Add("DefenderMastery", MasteryCategories.Defender);
        cloudKeys.Add("MagewrightMastery", MasteryCategories.Magewright);

        Masteries = new Dictionary<MasteryCategories, int>();
        Masteries.Add(MasteryCategories.Auric, AuricMastery);
        Masteries.Add(MasteryCategories.Order, OrderMastery);
        Masteries.Add(MasteryCategories.Chaos, ChaosMastery);
        Masteries.Add(MasteryCategories.Life, LifeMastery);
        Masteries.Add(MasteryCategories.Death, DeathMastery);
        Masteries.Add(MasteryCategories.Fire, FireMastery);
        Masteries.Add(MasteryCategories.Water, WaterMastery);
        Masteries.Add(MasteryCategories.Earth, EarthMastery);
        Masteries.Add(MasteryCategories.Air, AirMastery);
        Masteries.Add(MasteryCategories.Divine, DivineMastery);
        Masteries.Add(MasteryCategories.Demonic, DemonicMastery);
        Masteries.Add(MasteryCategories.Movement, MovementMastery);
        Masteries.Add(MasteryCategories.Battlemage, BattlemageMastery);
        Masteries.Add(MasteryCategories.Support, SupportMastery);
        Masteries.Add(MasteryCategories.Defender, DefenderMastery);
        Masteries.Add(MasteryCategories.Magewright, MagewrightMastery);
        StartCoroutine(SyncTimer());

        MasteryPerRank = new Dictionary<AuricaSpell.DifficultyRank, int>();
        MasteryPerRank.Add(AuricaSpell.DifficultyRank.Rank1, 0);
        MasteryPerRank.Add(AuricaSpell.DifficultyRank.Rank2, 1);
        MasteryPerRank.Add(AuricaSpell.DifficultyRank.Rank3, 2);
        MasteryPerRank.Add(AuricaSpell.DifficultyRank.Rank4, 4);
    }

    void FixedUpdate() {
        if (!fetched && !fetching) FetchMasteries();
    }

    IEnumerator SyncTimer() {
        while(true) {
            yield return new WaitForSeconds(60f);
            if (!synced) SyncMasteries();
        }
    }

    public Dictionary<MasteryCategories, int> GetMasteries() {
        return Masteries;
    }

    public int GetMastery(MasteryCategories category) {
        return Masteries[category];
    }

    public bool HasMasteryForSpell(AuricaSpell spell) {
        // Debug.Log("Check mastery for ["+spell.c_name+"]  isMasterySpell: "+spell.isMasterySpell+" category: "+spell.masteryCategory+"   required mastery: "+MasteryThresholds[spell.masteryLevel]+" player mastery: "+Masteries[spell.masteryCategory]+"     FINAL VERDICT = "+(!spell.isMasterySpell || (Masteries[spell.masteryCategory] >= MasteryThresholds[spell.masteryLevel])));
        return !spell.isMasterySpell || (Masteries[spell.masteryCategory] >= MasteryThresholds[spell.masteryLevel]);
    }

    public bool HasMasteryForCompoundComponent(AuricaCompoundComponent component) {
        Debug.Log("Check mastery for compound component ["+component.c_name+"]  isMasterySpell: "+component.needsMastery+"   required mastery: "+MasteryThresholds[component.masteryLevel]+" player mastery: "+Masteries[MasteryCategories.Magewright]+"     FINAL VERDICT = "+(!component.needsMastery || (Masteries[MasteryCategories.Magewright] >= MasteryThresholds[component.masteryLevel])));
        return !component.needsMastery || (Masteries[MasteryCategories.Magewright] >= MasteryThresholds[component.masteryLevel]);
    }

    public void FetchMasteries() {
        fetching = true;
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnError);
    }

    void OnDataRecieved (GetUserDataResult result) {
        if (result.Data == null) return;

        if (result.Data.ContainsKey("Masteries")) {
            string rawMasteries = result.Data["Masteries"].Value;
            SetMasteriesFromString(rawMasteries);
            // Debug.Log("Fetched Masteries from cloud: "+rawMasteries);
        } else {
            Debug.Log("No Masteries found in cloud");
        }

        fetched = true;
        fetching = false;
    }

    public void AddMasteries(List<MasteryCategories> categories, AuricaSpell.DifficultyRank rank) {
        if (categories.Count == 0 || rank == AuricaSpell.DifficultyRank.Rank1) return;
        foreach(MasteryCategories category in categories){
            Masteries[category] += MasteryPerRank[rank];
            // Debug.Log("Added "+MasteryPerRank[rank]+" mastery to "+category.ToString()+"  total mastery: "+Masteries[category]);
            if (Masteries[category] == 10) {
                RewardsManager.Instance.AddRewards(0.01f);
                TipWindow.Instance.ShowTip(""+category.ToString()+" Novice Mastery", "You have achieved Novice mastery in "+category.ToString()+" magic. Achieving this milestone progresses your cultivation. Press \"M\" to view your mastery.", 10f);
                SyncMasteries();
                MasteryProgressionSound.Play();
            } else if (Masteries[category] == 100) {
                RewardsManager.Instance.AddRewards(0.0210f);
                TipWindow.Instance.ShowTip(""+category.ToString()+" Adept Mastery", "You have achieved Adept mastery in "+category.ToString()+" magic. Achieving this milestone progresses your cultivation. Press \"M\" to view your mastery.", 10f);
                SyncMasteries();
                MasteryProgressionSound.Play();
            } else if (Masteries[category] == 1000) {
                RewardsManager.Instance.AddRewards(0.1f);
                TipWindow.Instance.ShowTip(""+category.ToString()+" Master Mastery", "You have achieved Master mastery in "+category.ToString()+" magic. Achieving this milestone progresses your cultivation. Press \"M\" to view your mastery.", 10f);
                SyncMasteries();
                MasteryProgressionSound.Play();
            } else if (Masteries[category] == 10000) {
                RewardsManager.Instance.AddRewards(1f);
                TipWindow.Instance.ShowTip(""+category.ToString()+" LEGEND MASTERY", "You have achieved Legendary mastery in "+category.ToString()+" magic. Achieving this milestone progresses your cultivation. Press \"M\" to view your mastery.", 10f);
                SyncMasteries();
                MasteryProgressionSound.Play();
                PlayerUIInfo.Instance.SetPlayerTitle("Legend");
            }
        }
        synced = false;
    }

    public void AddMastery(MasteryCategories category, int amount) {
        if (amount == 0) return;
        Masteries[category] += amount;
        synced = false;
        // Debug.Log("Added "+amount+" mastery to "+category.ToString());
    }

    public void UpdateMasteries(Dictionary<MasteryCategories, int> addedMasteries) {
        foreach(KeyValuePair<MasteryCategories,int> mastery in addedMasteries) {
            if (mastery.Value > 0) {
                Masteries[mastery.Key] = mastery.Value;
            }
        }
        synced = false;
    }

    public void SyncMasteries() {
        // Debug.Log("Syncing Masteries");

        var request = new UpdateUserDataRequest {
            Data = new Dictionary<string, string> {
                {"Masteries", MasteriesToString()}
            }
        };

        PlayFabClientAPI.UpdateUserData(request, OnDiscoveriesDataSend, OnError);
    }

    void OnDiscoveriesDataSend(UpdateUserDataResult result) {
        // Debug.Log("Masteries Synced with Cloud");
        synced = true;
        fetched = false;
    }

    void OnError(PlayFabError error) {
        Debug.LogError(error.GenerateErrorReport());
    }

    string MasteriesToString() {
        string text = "";
        foreach(KeyValuePair<MasteryCategories,int> mastery in Masteries) {
            text += ""+mastery.Key.ToString()+":"+mastery.Value+"|";
        }
        return text;
    }

    void SetMasteriesFromString(string rawText) {
        List<string> splitText = rawText.Split('|').ToList();
        foreach(string mastery in splitText){
            if (mastery == "") continue;
            List<string> splitMastery = mastery.Split(':').ToList();
            MasteryCategories category = (MasteryCategories)System.Enum.Parse( typeof(MasteryCategories), splitMastery[0] );
            Masteries[category] = int.Parse(splitMastery[1]);
        }
    }
}
