using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardsUIPanel : MonoBehaviour {
    public static RewardsUIPanel Instance;

    public float changePerButtonPress = 0.001f;
    public float changePerExpertiseButtonPress = 0.005f;
    public Color modifiedValueColor, baseValueColor;
    public Text rewardPointsText;
    public DistributionUIDisplay distributionDisplay;
    public DistributionUIDisplayValues distributionDisplayValues;
    public Text structureText, essenceText, fireText, waterText, earthText, airText, natureText, expertiseText;
    public Button addExpertiseButton, decreaseExpertiseButton;
    public Slider expertiseSlider;
    public GameObject advicePanel;
    public List<Button> toggleButtonsWhenPointsAvailable, toggleButtonsWhenPointsSpent;

    private ManaDistribution currentAura, addedDistribution;
    private float rewardPoints, usedRewardPoints, usedRewardPointsForExpertise;
    private int expertise, addedExpertise;

    // Start is called before the first frame update
    void Start() {
        RewardsUIPanel.Instance = this;
        currentAura = new ManaDistribution(PlayerPrefs.GetString("Aura"));
        if (RewardsManager.Instance != null) RewardsManager.Instance.GetRewards();
        Reset();
    }

    void OnEnable() {
        Rerender();
    }

    public void Rerender() {
        currentAura = new ManaDistribution(PlayerPrefs.GetString("Aura"));
        float points = Mathf.Round((rewardPoints-usedRewardPoints-usedRewardPointsForExpertise) * 1000f) / 1000f;
        rewardPointsText.text = "Available Cultivation Points: "+(Mathf.Round(points * 1000f)).ToString();
        distributionDisplay.SetDistribution(currentAura+addedDistribution);
        distributionDisplayValues.SetDistribution(currentAura+addedDistribution);

        foreach (var obj in toggleButtonsWhenPointsAvailable) obj.interactable = (rewardPoints > 0f);
        foreach (var obj in toggleButtonsWhenPointsSpent) obj.interactable = (usedRewardPoints > 0f || usedRewardPointsForExpertise > 0f);

        if (ExpertiseManager.Instance != null) {
            expertise = ExpertiseManager.Instance.GetExpertise();
            if (expertise >= 0) {
                expertiseText.text = (expertise + addedExpertise).ToString();
                expertiseSlider.value = Mathf.Clamp(expertise + addedExpertise, ExpertiseManager.MINIMUM_EXPERTISE, ExpertiseManager.MAXIMUM_EXPERTISE);
            } else {
                expertiseText.text = "0";
                expertiseSlider.value = 0;
            }
            
        }

        if (addedExpertise != 0) {
            expertiseText.color = modifiedValueColor;
        } else {
            expertiseText.color = baseValueColor;
        }

        if (addedDistribution.structure == 0f) {
            structureText.color = baseValueColor;
        } else {
            structureText.color = modifiedValueColor;
        }
        if (addedDistribution.essence == 0f) {
            essenceText.color = baseValueColor;
        } else {
            essenceText.color = modifiedValueColor;
        }
        if (addedDistribution.fire == 0f) {
            fireText.color = baseValueColor;
        } else {
            fireText.color = modifiedValueColor;
        }
        if (addedDistribution.water == 0f) {
            waterText.color = baseValueColor;
        } else {
            waterText.color = modifiedValueColor;
        }
        if (addedDistribution.earth == 0f) {
            earthText.color = baseValueColor;
        } else {
            earthText.color = modifiedValueColor;
        }
        if (addedDistribution.air == 0f) {
            airText.color = baseValueColor;
        } else {
            airText.color = modifiedValueColor;
        }
        if (addedDistribution.nature == 0f) {
            natureText.color = baseValueColor;
        } else {
            natureText.color = modifiedValueColor;
        }
        
    }

    public void ModifyDistribution(string key) {
        if ((usedRewardPoints + usedRewardPointsForExpertise) >= rewardPoints) return;
        switch (key) {
            case "order":
                addedDistribution.structure += changePerButtonPress;
                break;
            case "chaos":
                addedDistribution.structure -= changePerButtonPress;
                break;
            case "life":
                addedDistribution.essence += changePerButtonPress;
                break;
            case "death":
                addedDistribution.essence -= changePerButtonPress;
                break;
            case "fireUp":
                addedDistribution.fire += changePerButtonPress;
                break;
            case "fireDown":
                addedDistribution.fire -= changePerButtonPress;
                break;
            case "waterUp":
                addedDistribution.water += changePerButtonPress;
                break;
            case "waterDown":
                addedDistribution.water -= changePerButtonPress;
                break;
            case "earthUp":
                addedDistribution.earth += changePerButtonPress;
                break;
            case "earthDown":
                addedDistribution.earth -= changePerButtonPress;
                break;
            case "airUp":
                addedDistribution.air += changePerButtonPress;
                break;
            case "airDown":
                addedDistribution.air -= changePerButtonPress;
                break;
            case "divine":
                addedDistribution.nature += changePerButtonPress;
                break;
            case "demonic":
                addedDistribution.nature -= changePerButtonPress;
                break;
        }
        usedRewardPoints = Mathf.Round(addedDistribution.GetAggregate() * 1000f) / 1000f;
        // Debug.Log("Used reward points: "+usedRewardPoints+"      dist: "+addedDistribution.ToString());
        Rerender();
    }

    public void SetExpertiseValue() {
        Rerender();
    }

    public void AddExpertise() {
        if ((expertise + addedExpertise) == ExpertiseManager.MAXIMUM_EXPERTISE || (usedRewardPoints + usedRewardPointsForExpertise) >= rewardPoints) return;
        addedExpertise += 1;
        usedRewardPointsForExpertise = Mathf.Abs(addedExpertise) * changePerExpertiseButtonPress;
        Rerender();
    }

    public void RemoveExpertise() {
        if ((expertise + addedExpertise) == ExpertiseManager.MINIMUM_EXPERTISE) return;
        addedExpertise -= 1;
        usedRewardPointsForExpertise = Mathf.Abs(addedExpertise) * changePerExpertiseButtonPress;
        Rerender();
    }

    public void ClosePanel() {
        Reset();
        advicePanel.SetActive(false);
        gameObject.SetActive(false);
    }

    public void Reset() {
        addedDistribution = new ManaDistribution();
        rewardPoints = RewardsManager.Instance.rewardPoints;
        usedRewardPoints = 0f;
        usedRewardPointsForExpertise = 0f;
        addedExpertise = 0;
        Rerender();
    }

    public void SubmitAuraChanges() {
        RewardsManager.Instance.SpendRewardsPoints(addedDistribution, Mathf.Round((usedRewardPoints) * 1000f) / 1000f);
        ExpertiseManager.Instance.SetExpertise(expertise + addedExpertise);
        Reset();
        Rerender();
    }

    public void SetRewardPoints(float points) {
        rewardPoints = points;
        Rerender();
    }
}
