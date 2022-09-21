using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class CharacterUI : MonoBehaviourPun {

    [Tooltip("UI Text to display Player's Name")]
    [SerializeField]
    private TMP_Text playerNameText;

    [Tooltip("UI Text to display Player's title")]
    [SerializeField]
    private TMP_Text playerTitleText;

    [Tooltip("UI Text to display status effects")]
    [SerializeField]
    private TMP_Text statusEffectText;

    [Tooltip("UI Slider to display Player's Health")]
    [SerializeField]
    private Slider playerHealthSlider;

    public GameObject boostIndicator, boostIndicatorContainer;
    public Transform cam;

    public Color baseColor = Color.white, statusEffectColor = Color.yellow;

    private string playerTitle, playerTitleColour;
    private PlayerManager target;
    private bool hidden = false, showingBoost = true;
    private Color initialColor;
    

    // Start is called before the first frame update
    void Awake() {
        cam = Camera.main.transform;
        initialColor = baseColor;
    }

    public void SetTitle(string title, string titleColour) {
        playerTitle = title;
        playerTitleColour = titleColour;
        playerTitleText.text = playerTitle;

        if (playerTitleColour == "" || playerTitleColour == null) return;
        string[] colourSeperator = new string[] { ", " };
        string[] splitColour = playerTitleColour.Split(colourSeperator, System.StringSplitOptions.None);
        Color newColor = new Color(float.Parse(splitColour[0])/255f, float.Parse(splitColour[1])/255f, float.Parse(splitColour[2])/255f);
        playerTitleText.color = newColor;
    }

    void FixedUpdate() {
        // Destroy itself if the target is null, It's a fail safe when Photon is destroying Instances of a Player over the network
        if (target == null) {
            Destroy(this.gameObject);
            return;
        }

        // Reflect the Player Health
        if (playerHealthSlider != null) {
            playerHealthSlider.value = target.Health;
        }

        List<string> statusEffects = new List<string>();
        if (target.stunned) statusEffects.Add("STUNNED");
        if (target.silenced) statusEffects.Add("SILENCED");
        if (target.rooted) statusEffects.Add("ROOTED");
        if (target.grounded) statusEffects.Add("GROUNDED");
        if (target.slowed) statusEffects.Add("SLOWED");
        if (target.hastened) statusEffects.Add("HASTE");
        if (target.fragile) statusEffects.Add("FRAGILE");
        if (target.tough) statusEffects.Add("TOUGH");
        if (target.strengthened) statusEffects.Add("STRONG");
        if (target.weakened) statusEffects.Add("WEAK");
        if (target.slowFall) statusEffects.Add("SLOW FALL");
        if (target.manaRestorationChange) {
            if (target.manaRestorationBuff) {
                statusEffects.Add("MANA RESTORATION");
            } else {
                statusEffects.Add("MANA DAMPENING");
            }
        }

        if (target.hasBoost && !showingBoost) {
            boostIndicator.SetActive(true);
            showingBoost = true;
        } else if (!target.hasBoost && showingBoost) {
            boostIndicator.SetActive(false);
            showingBoost = false;
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

    void LateUpdate() {
        transform.LookAt(transform.position + cam.forward);
    }

    public void SetTarget(PlayerManager _target) {
        if (_target == null) {
            Debug.LogError("<Color=Red><a>Missing</a></Color> PlayMakerManager target for PlayerUI.SetTarget.", this);
            return;
        }

        // Cache references for efficiency
        target = _target;
        if (playerNameText != null) playerNameText.text = target.photonView.Owner.NickName;
    }

    public void SetStatusEffect(string status) {
        if (statusEffectText != null) {
            statusEffectText.text = status;
        }
    }

    public void ResetStatusEffects() {
        if (statusEffectText != null) {
            statusEffectText.text = "";
        }
    }

    public void Hide() {
        if (hidden) return;
        playerNameText.gameObject.SetActive(false);
        playerTitleText.gameObject.SetActive(false);
        statusEffectText.gameObject.SetActive(false);
        playerHealthSlider.gameObject.SetActive(false);
        boostIndicatorContainer.SetActive(false);
        hidden = true;
    }

    public void Show() {
        if (!hidden) return;
        playerNameText.gameObject.SetActive(true);
        playerTitleText.gameObject.SetActive(true);
        statusEffectText.gameObject.SetActive(true);
        playerHealthSlider.gameObject.SetActive(true);
        boostIndicatorContainer.SetActive(true);
        hidden = false;
    }

    public void SetNameColor(Color newColor) {
        baseColor = newColor;
        playerNameText.color = baseColor;
    }

    public void ResetNameColor() {
        baseColor = initialColor;
        playerNameText.color = baseColor;
    }

    public void CreateDamagePopup(float damage) {
        GameObject newPopup = PhotonNetwork.Instantiate("ZZZ Damage Popup Canvas", transform.position+ (Vector3.up * 0.5f), transform.rotation, 0);
        DamagePopup dmgPopup = newPopup.GetComponent<DamagePopup>();
        if (dmgPopup != null) {
            dmgPopup.ShowDamage(damage);
        }
    }

    public DamagePopup CreateAccumulatingDamagePopup(float damage) {
        GameObject newPopup = PhotonNetwork.Instantiate("ZZZ Damage Popup Canvas", transform.position+ (Vector3.up * 0.5f), transform.rotation, 0);
        DamagePopup dmgPopup = newPopup.GetComponent<DamagePopup>();
        if (dmgPopup != null) {
            dmgPopup.AccumulatingDamagePopup(damage);
            dmgPopup.AttachTo(gameObject);
        }
        return dmgPopup;
    }

}
