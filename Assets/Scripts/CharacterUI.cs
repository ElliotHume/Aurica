using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class CharacterUI : MonoBehaviour {

    [Tooltip("UI Text to display Player's Name")]
    [SerializeField]
    private TMP_Text playerNameText;

    [Tooltip("UI Slider to display Player's Health")]
    [SerializeField]
    private Slider playerHealthSlider;

    public GameObject boostIndicator, boostIndicatorContainer;
    public Transform cam;

    public Color baseColor = Color.white, statusEffectColor = Color.yellow;


    private PlayerManager target;
    private bool hidden = false, showingBoost = true;
    private Color initialColor;
    

    // Start is called before the first frame update
    void Awake() {
        cam = Camera.main.transform;
        initialColor = baseColor;
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
        if (target.manaRestorationChange) statusEffects.Add("ALTERED MANA");

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
        if (playerNameText != null) {
            playerNameText.text = status;
            playerNameText.color = statusEffectColor;
        }
    }

    public void ResetStatusEffects() {
        if (playerNameText != null) {
            playerNameText.text = target.photonView.Owner.NickName;
            playerNameText.color = baseColor;
        }
    }

    public void Hide() {
        if (hidden) return;
        playerNameText.gameObject.SetActive(false);
        playerHealthSlider.gameObject.SetActive(false);
        boostIndicatorContainer.SetActive(false);
        hidden = true;
    }

    public void Show() {
        if (!hidden) return;
        playerNameText.gameObject.SetActive(true);
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
        newPopup.transform.SetParent(gameObject.transform);
        DamagePopup dmgPopup = newPopup.GetComponent<DamagePopup>();
        if (dmgPopup != null) {
            dmgPopup.AccumulatingDamagePopup(damage);
        }
        return dmgPopup;
    }

}
