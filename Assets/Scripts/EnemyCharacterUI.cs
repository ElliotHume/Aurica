using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class EnemyCharacterUI : MonoBehaviour {

    public Enemy target;

    [Tooltip("UI Text to display Player's Name")]
    [SerializeField]
    private TMP_Text enemyNameText;

    [Tooltip("UI Slider to display Player's Health")]
    [SerializeField]
    private Slider enemyHealthSlider;

    public Transform cam;

    public Color baseColor = Color.white, statusEffectColor = Color.yellow;

    private bool hidden = false;
    private Color initialColor;
    

    void Awake() {
        cam = Camera.main.transform;
        initialColor = baseColor;
    }

    void Start() {
        if (target != null && target.c_name != "") enemyNameText.text = target.c_name;
    }

    void FixedUpdate() {
        // Destroy itself if the target is null, It's a fail safe when Photon is destroying Instances of a Enemy over the network
        if (target == null) {
            Destroy(this.gameObject);
            return;
        }

        // Reflect the Enemy Health
        if (enemyHealthSlider != null) {
            enemyHealthSlider.value = target.Health;
        }
    }

    void LateUpdate() {
        transform.LookAt(transform.position + cam.forward);
    }

    public void SetTarget(Enemy _target) {
        if (_target == null) {
            Debug.LogError("<Color=Red><a>Missing</a></Color> PlayMakerManager target for EnemyUI.SetTarget.", this);
            return;
        }

        // Cache references for efficiency
        target = _target;
        if (enemyNameText != null) enemyNameText.text = target.c_name;
    }

    public void Hide() {
        if (hidden) return;
        enemyNameText.gameObject.SetActive(false);
        enemyHealthSlider.gameObject.SetActive(false);
        hidden = true;
    }

    public void Show() {
        if (!hidden) return;
        enemyNameText.gameObject.SetActive(true);
        enemyHealthSlider.gameObject.SetActive(true);
        hidden = false;
    }

    public void SetNameColor(Color newColor) {
        baseColor = newColor;
        enemyNameText.color = baseColor;
    }

    public void ResetNameColor() {
        baseColor = initialColor;
        enemyNameText.color = baseColor;
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
