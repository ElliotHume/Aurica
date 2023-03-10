using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

public class Nexus : Structure, IPunObservable {

    [Tooltip("NexusField object")]
    [SerializeField]
    private NexusField Field;

    [Tooltip("Objects to disable when the nexus explodes")]
    [SerializeField]
    private List<GameObject> DisableObjectsOnExplode;

    [Tooltip("Objects to enable when the nexus explodes")]
    [SerializeField]
    private List<GameObject> EnableObjectsOnExplode;

    [Tooltip("Effects to play when the nexus is restored")]
    [SerializeField]
    private List<ParticleSystem> RestorationParticles;

    // Start is called before the first frame update
    void Start() {
        Health = StartingHealth;

        if (Field != null) Field.SetTeam(Team);

        if (StructureUIPrefab != null) {
            UIDisplayGO = Instantiate(StructureUIPrefab, UIDisplayAnchor.position, UIDisplayAnchor.rotation, transform);
            UIDisplay = UIDisplayGO.GetComponent<StructureUIDisplay>();
            UIDisplay.SetStructure(this);
        }
    }

    protected override void NetworkExplode() {
        if (!photonView.IsMine) return;
        Debug.Log("Nexus NetworkExplode");
        //TODO: Call MOBAMatchManager to do something when the nexus explodes
        MOBAMatchManager.Instance.NetworkMasterNexusBroken(Team);
    }

    protected override void LocalEffectExplode() {
        broken = true;
        Debug.Log("Nexus LocalEffectExplode");
        foreach(GameObject obj in DisableObjectsOnExplode) obj.SetActive(false);
        foreach(GameObject obj in EnableObjectsOnExplode) obj.SetActive(true);

        if (UIDisplay != null) UIDisplay.Hide();
        if (Field != null) Field.Disable();

        Collider[] colliders = GetComponents<Collider>();
        foreach(Collider collider in colliders) collider.enabled = false;
    }

    public override void Restore() {
        if (photonView.IsMine) Health = StartingHealth;
        broken = false;
        Debug.Log("Nexus Restore");
        foreach(GameObject obj in DisableObjectsOnExplode) obj.SetActive(true);
        foreach(GameObject obj in EnableObjectsOnExplode) obj.SetActive(false);
        foreach(ParticleSystem ps in RestorationParticles) ps.Play();

        if (UIDisplay != null) UIDisplay.Show();
        if (Field != null) Field.Enable();

        Collider[] colliders = GetComponents<Collider>();
        foreach(Collider collider in colliders) collider.enabled = true;
    }

    public override string GetName() {
        return Team.ToString()+" Nexus";
    }
}
