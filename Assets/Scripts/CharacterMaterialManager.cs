using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CharacterMaterialManager : MonoBehaviourPun
{
    public CharacterUI characterUI;
    public Material defaultMaterial, invisibleMaterial;
    public List<GameObject> toggleObjects, adminToggleObjects;
    public Mesh adminMesh;
    public Material adminMaterial;

    Material baseMaterial;
    bool isInvisible = false;
    new SkinnedMeshRenderer renderer;

    void Start() {
        renderer = GetComponent<SkinnedMeshRenderer>();
        baseMaterial = defaultMaterial;

        if (photonView.Owner.NickName == "Xelerox") {
            baseMaterial = adminMaterial;
            Material[] mats = new Material[]{baseMaterial};
            renderer.materials = mats;
            renderer.sharedMesh = adminMesh;
            foreach(var obj in toggleObjects) obj.SetActive(false);
            toggleObjects.Clear();
            toggleObjects = adminToggleObjects;
            foreach(var obj in toggleObjects) obj.SetActive(true);
        }
    }

    public void SetUI(CharacterUI ui) {
        characterUI = ui;
    }

    public void HideCharacterUI() {
        if (characterUI != null) characterUI.Hide();
    }

    public void ShowCharacterUI() {
        if (characterUI != null) characterUI.Show();
    }

    public void GoInvisible() {
        if (isInvisible) return;
        Material[] mats = new Material[]{invisibleMaterial};
        renderer.materials = mats;
        HideCharacterUI();
        isInvisible = true;
        foreach(var obj in toggleObjects) obj.SetActive(false);
    }

    public void ResetMaterial() {
        Material[] mats = new Material[]{baseMaterial};
        renderer.materials = mats;
        ShowCharacterUI();
        isInvisible = false;
        foreach(var obj in toggleObjects) obj.SetActive(true);
    }
}
