using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RecastSpell : MonoBehaviourPun {

    public string spellOnRecastPrefabName;
    public string networkEffectOnRecast;
    public Vector3 positionOffset;
    public bool createRecastAtOwner = false;
    public Vector3 rotationOffset;
    public bool useRotationOffset = false;
    public bool turnRecastSpellTowardsAimPoint = false;

    protected float spellStrength = 1f, expertise = -1f;
    protected ManaDistribution damageModifier;
    protected GameObject owner;
    protected PlayerManager ownerPM;

    public virtual void SetSpellStrength(float newStrength) {
        spellStrength = newStrength;
    }

    public virtual void SetSpellDamageModifier(ManaDistribution newMod) {
        damageModifier = newMod;
    }

    public float GetSpellStrength() {
        return spellStrength;
    }

    public ManaDistribution GetSpellDamageModifier() {
        return damageModifier;
    }

    public virtual void SetOwner(GameObject ownerGO) {
        owner = ownerGO;
        ownerPM = ownerGO.GetComponent<PlayerManager>();
    }

    public GameObject GetOwner() {
        return owner;
    }

    public PlayerManager GetOwnerPM() {
        return ownerPM;
    }

    public virtual void SetExpertiseParameters(int exp) {
        expertise = exp;
    }

    Vector3 GetSpawnPosition() {
        if (createRecastAtOwner) {
            return owner.transform.position;
        }

        return transform.position+transform.InverseTransformDirection(positionOffset);
    }

    Quaternion GetSpawnRotation() {
        if (useRotationOffset) {
            return Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(rotationOffset), 1000);
        }
        if (turnRecastSpellTowardsAimPoint) {
            Vector3 aimPoint = Crosshair.Instance.GetWorldPoint();
            return Quaternion.LookRotation(aimPoint-transform.position);
        }

        return transform.rotation;
    }


    public void InitiateRecast(){
        if (!photonView.IsMine) return;
        Vector3 spawnPosition = GetSpawnPosition();
        Quaternion spawnRotation = GetSpawnRotation();
        GameObject instance = PhotonNetwork.Instantiate(spellOnRecastPrefabName, spawnPosition, spawnRotation);
        Spell instanceSpell = instance.GetComponent<Spell>();
        if (instanceSpell != null) {
            instanceSpell.SetSpellStrength(GetSpellStrength());
            instanceSpell.SetSpellDamageModifier(GetSpellDamageModifier());
            instanceSpell.SetOwner(GetOwner());
            instanceSpell.SetExpertiseParameters(ExpertiseManager.Instance.GetExpertise());
        } else {
            Enemy instancedEnemy = instance.GetComponent<Enemy>();
            if (instancedEnemy != null) {
                instance.transform.rotation = Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y, 0));
                instancedEnemy.SetPlayerOwner(GetOwner());
                instancedEnemy.SetStrength(GetSpellStrength());
            }
        }

        GameObject networkEffect = PhotonNetwork.Instantiate(networkEffectOnRecast, spawnPosition, spawnRotation);

        PhotonNetwork.Destroy(gameObject);
    }
}
