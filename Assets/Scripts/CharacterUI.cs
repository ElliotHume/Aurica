using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CharacterUI : MonoBehaviour {

    [Tooltip("UI Text to display Player's Name")]
    [SerializeField]
    private Text playerNameText;

    [Tooltip("UI Slider to display Player's Health")]
    [SerializeField]
    private Slider playerHealthSlider;
    public Transform cam;

    public Color baseColor = Color.black, statusEffectColor = Color.white;


    private PlayerManager target;
    private bool hidden = false;
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

        if (target.stunned) {
            SetStatusEffect("STUNNED...");
        } else if (target.silenced) {
            SetStatusEffect("SILENCED...");
        } else if (target.rooted) {
            SetStatusEffect("ROOTED...");
        } else if (target.slowed) {
            SetStatusEffect("SLOWED...");
        } else if (target.hastened) {
            SetStatusEffect("HASTE...");
        } else if (target.fragile) {
            SetStatusEffect("FRAGILE...");
        } else if (target.tough) {
            SetStatusEffect("TOUGH...");
        } else if (target.strengthened) {
            SetStatusEffect("STRONG...");
        } else if (target.weakened) {
            SetStatusEffect("WEAK...");
        }  else {
            ResetStatusEffects();
        }
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
        hidden = true;
    }

    public void Show() {
        if (!hidden) return;
        playerNameText.gameObject.SetActive(true);
        playerHealthSlider.gameObject.SetActive(true);
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
}
