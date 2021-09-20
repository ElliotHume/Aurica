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
    bool isInvisible = false, isAdmin = false;
    new SkinnedMeshRenderer renderer;
    Outline outline;
    bool outlineSet = false;

    void Start() {
        renderer = GetComponent<SkinnedMeshRenderer>();
        baseMaterial = defaultMaterial;

        if (photonView.Owner.NickName == "Xelerox") {
            isAdmin = true;
            baseMaterial = adminMaterial;
            Material[] mats = new Material[]{baseMaterial};
            renderer.materials = mats;
            renderer.sharedMesh = adminMesh;
            foreach(var obj in toggleObjects) obj.SetActive(false);
            toggleObjects.Clear();
            toggleObjects = adminToggleObjects;
            foreach(var obj in toggleObjects) obj.SetActive(true);
        }

        outline = gameObject.AddComponent<Outline>();
        outline.OutlineMode = Outline.Mode.OutlineVisible;
        outline.OutlineColor = Color.white;
        outline.OutlineWidth = photonView.IsMine ? 0f : 1f;
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

    public void SetNameColor(Color color) {
        if (characterUI != null) characterUI.SetNameColor(color);
    }

    public void ResetNameColor() {
        if (characterUI != null) characterUI.ResetNameColor();
    }

    public void SetOutline(Color color) {
        if (outline != null) {
            outline.OutlineColor = color;
            outline.OutlineWidth = 1f;
            outlineSet = true;
        }
    }

    public void ResetOutline() {
        if (outline != null) {
            outline.OutlineColor = Color.white;
            outline.OutlineWidth = photonView.IsMine ? 0f : 1f;
            outlineSet = false;
        }
    }

    public void HideOutline() {
        if (outline != null) {
            outline.OutlineWidth = 0f;
        }
    }

    public void ShowOutline() {
        if (outline != null) {
            outline.OutlineWidth = (!outlineSet && photonView.IsMine) ? 0f : 1f;
        }
    }

    public void SetPlayerMaterial(Material mat) {
        if (isAdmin) return;
        baseMaterial = mat;
        Material[] mats = new Material[]{baseMaterial};
        renderer.materials = mats;
        // Refresh the outline on material change
        outline.enabled = false;
        outline.enabled = true;
    }

    public void ResetPlayerMaterial() {
        if (isAdmin) return;
        baseMaterial = defaultMaterial;
        Material[] mats = new Material[]{baseMaterial};
        renderer.materials = mats;
        // Refresh the outline on material change
        outline.enabled = false;
        outline.enabled = true;
    }

    public void GoInvisible() {
        if (isInvisible) return;
        Material[] mats = new Material[]{invisibleMaterial};
        renderer.materials = mats;
        HideCharacterUI();
        isInvisible = true;
        foreach(var obj in toggleObjects) obj.SetActive(false);
        if (outline != null) outline.enabled = false;
    }

    public void ResetMaterial() {
        Material[] mats = new Material[]{baseMaterial};
        renderer.materials = mats;
        ShowCharacterUI();
        isInvisible = false;
        foreach(var obj in toggleObjects) obj.SetActive(true);
        if (outline != null) outline.enabled = true;
    }
}
