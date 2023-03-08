using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CharacterMaterialManager : MonoBehaviourPun
{
    public CharacterUI characterUI;
    public Material defaultMaterial, invisibleMaterial;
    public List<GameObject> toggleObjects, adminToggleObjects, journeymanObjects, masterObjects, archmagusObjects;
    public Mesh adminMesh;
    public Material adminMaterial, masterMaterial, archmagusMaterial;
    public GameObject basicHelm;

    Material baseMaterial;
    bool isInvisible = false, isAdmin = false;
    new SkinnedMeshRenderer renderer;
    Outline outline;
    bool outlineSet = false, readyForOutline = false;

    void Start() {
        renderer = GetComponent<SkinnedMeshRenderer>();
        baseMaterial = defaultMaterial;

        StartCoroutine(CreateOutline());
    }

    IEnumerator CreateOutline() {
        while (!readyForOutline) {
            yield return new WaitForFixedUpdate();
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
            outline.OutlineWidth = 1.5f;
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
            outline.OutlineWidth = (!outlineSet && photonView.IsMine) ? 0f : outlineSet ? 1.5f : 1f;
        }
    }

    public void CheckAdmin(string name) {
        if (renderer == null) {
            renderer = GetComponent<SkinnedMeshRenderer>();
        }
        if (name == "Xelerox #E3A180454FDFC70E") {
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
        readyForOutline = true;
    }

    public void SetExpertiseMaterials(int expertise) {
        if (isAdmin) return;
        if (expertise >= ExpertiseManager.ARCHMAGUS_EXPERTISE) {
            SetPlayerMaterial(archmagusMaterial);
            basicHelm.SetActive(false);
            foreach(var obj in toggleObjects) obj.SetActive(false);
            toggleObjects = archmagusObjects;
            foreach(var obj in toggleObjects) obj.SetActive(true);
        } else if (expertise >= ExpertiseManager.MASTER_EXPERTISE) {
            SetPlayerMaterial(masterMaterial);
            basicHelm.SetActive(false);
            foreach(var obj in toggleObjects) obj.SetActive(false);
            toggleObjects = masterObjects;
            foreach(var obj in toggleObjects) obj.SetActive(true);
        } else if (expertise >= ExpertiseManager.JOURNEYMAN_EXPERTISE) {
            SetPlayerMaterial(defaultMaterial);
            foreach(var obj in toggleObjects) obj.SetActive(false);
            basicHelm.SetActive(true);
            toggleObjects = journeymanObjects;
            foreach(var obj in toggleObjects) obj.SetActive(true);
        } else {
            SetPlayerMaterial(defaultMaterial);
            foreach(var obj in toggleObjects) obj.SetActive(false);
            basicHelm.SetActive(true);
            toggleObjects.Clear();
            toggleObjects.Add(basicHelm);
        }
    }

    public void SetPlayerMaterial(Material mat) {
        if (isAdmin) return;
        baseMaterial = mat;
        Material[] mats = new Material[]{baseMaterial};
        renderer.materials = mats;
        // Refresh the outline on material change
        if (outline != null) {
            outline.enabled = false;
            outline.enabled = true;
        }
    }

    public void ResetPlayerMaterial() {
        if (isAdmin) return;
        baseMaterial = defaultMaterial;
        Material[] mats = new Material[]{baseMaterial};
        renderer.materials = mats;
        // Refresh the outline on material change
        if (outline != null) {
            outline.enabled = false;
            outline.enabled = true;
        }
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
