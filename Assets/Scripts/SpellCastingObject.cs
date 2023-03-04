using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpellCastingObject : MonoBehaviour {

    public string spellName;
    public float spellStrength = 1f;
    public Vector3 offset;

    public bool active = false, oneTimeCast = false;
    public float timeBetweenCasts = 0f;

    float cooldown = 0f;
    bool hasShot;

    // Update is called once per frame
    void FixedUpdate() {
        if (active) {
            if (cooldown <= 0.1f) {
                Shoot();
                cooldown = timeBetweenCasts;
            }
            cooldown -= Time.deltaTime;
        }
    }

    public void StartShooting() {
        active = true;
    }

    public void StopShooting() {
        active = false;
    }

    public void Shoot() {
        if (oneTimeCast && hasShot) {
            active = false;
            return;
        }
        GameObject closestPlayer = FindClosestPlayer();

        GameObject newSpell = PhotonNetwork.Instantiate(spellName, transform.position + (transform.right * offset.x) +(transform.forward * offset.z) + (transform.up * offset.y), transform.rotation);
        Spell spell = newSpell.GetComponent<Spell>();
        if (spell != null) {
            spell.SetSpellStrength(spellStrength);
            spell.SetOwner(gameObject, false);
        } else {
            Debug.Log("Could not grab <Spell> Object from newly instantiated spell");
        }

        if (spell.IsSelfTargeted) {
            TargetedSpell targetedSpell = newSpell.GetComponent<TargetedSpell>();
            if (targetedSpell != null) targetedSpell.SetTarget(gameObject);
            AoESpell aoeSpell = newSpell.GetComponent<AoESpell>();
            if (aoeSpell != null) aoeSpell.SetTarget(gameObject);
        } else if (spell.IsOpponentTargeted) {
            TargetedSpell ts = newSpell.GetComponent<TargetedSpell>();
            if (ts != null) ts.SetTarget(closestPlayer);
            AoESpell aoeSpell = newSpell.GetComponent<AoESpell>();
            if (aoeSpell != null) aoeSpell.SetTarget(closestPlayer);
        }

        BasicProjectileSpell bps = newSpell.GetComponent<BasicProjectileSpell>();
        if (bps != null) {
            bps.CanHitSelf = true;
            bps.SetEnemyAttack();
            bps.SetAimAssistTarget(closestPlayer);
            bps.SetHomingTarget(closestPlayer);
        }
        StatusEffect se = newSpell.GetComponent<StatusEffect>();
        if (se != null) {
            se.SetOwner(gameObject);
        }
    }

    public GameObject FindClosestPlayer() {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Player");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos) {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance && go.GetComponents<TargetDummy>().Length == 0) {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }
}
