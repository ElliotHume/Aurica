using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMaterialManager : MonoBehaviour
{
    public CharacterUI characterUI;
    public Material baseMaterial, invisibleMaterial;
    public List<GameObject> toggleObjects;

    bool isInvisible = false;
    SkinnedMeshRenderer renderer;

    void Start() {
        renderer = GetComponent<SkinnedMeshRenderer>();
    }

    // DEBUG PURPOSES
    void Update() {
        if (Input.GetKeyDown("l")) {
            if (!isInvisible) {
                GoInvisible();
            } else {
                ResetMaterial();
            }
        }
    }

    public void SetUI(CharacterUI ui) {
        characterUI = ui;
    }

    public void GoInvisible() {
        Material[] mats = new Material[]{invisibleMaterial};
        renderer.materials = mats;
        if (characterUI != null) characterUI.Hide();
        isInvisible = true;
        foreach(var obj in toggleObjects) obj.SetActive(false);
    }

    public void ResetMaterial() {
        Material[] mats = new Material[]{baseMaterial};
        renderer.materials = mats;
        if (characterUI != null) characterUI.Show();
        isInvisible = false;
        foreach(var obj in toggleObjects) obj.SetActive(true);
    }
}
