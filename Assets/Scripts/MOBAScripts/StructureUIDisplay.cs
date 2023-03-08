using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class StructureUIDisplay : MonoBehaviour
{
    [Tooltip("UI Text to display the structure's Name")]
    [SerializeField]
    private TMP_Text structureNameText;

    [Tooltip("UI Slider to display structures's Health")]
    [SerializeField]
    private Slider structureHealthSlider;

    [Tooltip("Image to indicate if the structure has immunity")]
    [SerializeField]
    private GameObject immunityIndicator;

    [Tooltip("Fill of the health bar, that we change if the structure is immune")]
    [SerializeField]
    private Image healthBarFill;
    
    [Tooltip("Base color for the structure's name")]
    [SerializeField]
    public Color baseNameTextColor = Color.white;

    [Tooltip("Base color for the structure's health bar when it is on the opposing team")]
    [SerializeField]
    public Color baseOpponentHealthFillColor = Color.red;

    [Tooltip("Base color for the structure's health bar when it is on the allied team")]
    [SerializeField]
    public Color baseAllyHealthFillColor = Color.green;

    [Tooltip("Color for the structure's name when it is Immune")]
    [SerializeField]
    private Color ImmuneColor = Color.cyan;

    private Structure structure = null;
    private bool hidden = false, setImmunity = false, immune = false, isAllied = false;
    private Color initialNameTextColor, baseColor;
    private Transform cam;

    // Start is called before the first frame update
    void Start() {
        cam = Camera.main.transform;
        initialNameTextColor = baseNameTextColor;
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (structure != null && !hidden) {
            // Set health bar value
            structureHealthSlider.value = structure.GetHealth();
        }
    }

    void LateUpdate() {
        transform.LookAt(transform.position + cam.forward);
    }

    public void SetStructure(Structure newStructure) {
        structure = newStructure;
        structureNameText.text = newStructure.GetName();
        structureHealthSlider.maxValue = newStructure.GetStartingHealth();
    }

    public void SetImmunity(bool immunity) {
        immune = immunity;
        immunityIndicator.SetActive(immune);
        structureNameText.color = immune ? ImmuneColor : baseNameTextColor;
        healthBarFill.color = immune ? ImmuneColor : isAllied ? baseAllyHealthFillColor : baseOpponentHealthFillColor;
    }

    public void SetColors(bool ally) {
        isAllied = ally;
        healthBarFill.color = structure.IsImmune() ? ImmuneColor : isAllied ? baseAllyHealthFillColor : baseOpponentHealthFillColor;
    }

    public void Hide() {
        if (hidden) return;
        structureNameText.gameObject.SetActive(false);
        immunityIndicator.gameObject.SetActive(false);
        structureHealthSlider.gameObject.SetActive(false);
        hidden = true;
    }

    public void Show() {
        if (!hidden) return;
        structureNameText.gameObject.SetActive(true);
        immunityIndicator.gameObject.SetActive(true);
        structureHealthSlider.gameObject.SetActive(true);
        hidden = false;
    }
}
