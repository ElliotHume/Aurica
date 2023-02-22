using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParticleManager : MonoBehaviour {

    public GameObject defaultParticles;
    private bool defaultParticlesPlaying = true;
    public GameObject r_auric, r_order, r_chaos, r_life, r_death, r_fire, r_water, r_earth, r_air, r_divine, r_demonic;
    public GameObject l_auric, l_order, l_chaos, l_life, l_death, l_fire, l_water, l_earth, l_air, l_divine, l_demonic;

    public List<GameObject> slowFX, hasteFX, rootFX, groundFX, stunFX, silenceFX, weakenFX, strengthenFX, fragileFX, toughFX, manaBuffFX, manaDebuffFX, healingFX, slowfallFX;
    public List<ParticleSystem> cleanseFX, cureFX, manaDrainFX, boostFX;
    private bool slowed, hastened, rooted, grounded, stunned, silenced, weakened, strengthened, fragile, tough, manaBuff, manaDebuff, healing, slowfall;

    PlayerManager playerManager;
    List<int> oneHandedAnimations = new List<int>();
    List<GameObject> activeParticles = new List<GameObject>();

    void Start() {
        oneHandedAnimations.Add(0);
        oneHandedAnimations.Add(1);
        oneHandedAnimations.Add(2);

        playerManager = GetComponent<PlayerManager>();
    }

    void FixedUpdate() {
        if (playerManager.stunned && !stunned && !playerManager.camouflaged) {
            ActivateEffectParticles(stunFX);
            stunned = true;
        } else if ((!playerManager.stunned && stunned) || (stunned && playerManager.camouflaged)) {
            DeactivateEffectParticles(stunFX);
            stunned = false;
        }

        if (playerManager.silenced && !silenced && !playerManager.camouflaged) {
            ActivateEffectParticles(silenceFX);
            silenced = true;
        } else if ((!playerManager.silenced && silenced) || (stunned && playerManager.camouflaged)) {
            DeactivateEffectParticles(silenceFX);
            silenced = false;
        }

        if (playerManager.rooted && !rooted && !playerManager.camouflaged) {
            ActivateEffectParticles(rootFX);
            rooted = true;
        } else if ((!playerManager.rooted && rooted) || (rooted && playerManager.camouflaged)) {
            DeactivateEffectParticles(rootFX);
            rooted = false;
        }

        if (playerManager.grounded && !grounded && !playerManager.camouflaged){
            ActivateEffectParticles(groundFX);
            grounded = true;
        } else if ((!playerManager.grounded && grounded) || (grounded && playerManager.camouflaged)) {
            DeactivateEffectParticles(groundFX);
            grounded = false;
        }

        if (playerManager.slowed && !slowed && !playerManager.camouflaged){
            ActivateEffectParticles(slowFX);
            slowed = true;
        } else if ((!playerManager.slowed && slowed) || (slowed && playerManager.camouflaged)) {
            DeactivateEffectParticles(slowFX);
            slowed = false;
        }

        if (playerManager.hastened && !hastened && !playerManager.camouflaged) {
            ActivateEffectParticles(hasteFX);
            hastened = true;
        } else if ((!playerManager.hastened && hastened) || (hastened && playerManager.camouflaged)) {
            DeactivateEffectParticles(hasteFX);
            hastened = false;
        }

        if (playerManager.fragile && !fragile && !playerManager.camouflaged) {
            ActivateEffectParticles(fragileFX);
            fragile = true;
        } else if ((!playerManager.fragile && fragile) || (fragile && playerManager.camouflaged)) {
            DeactivateEffectParticles(fragileFX);
            fragile = false;
        }

        if (playerManager.tough && !tough && !playerManager.camouflaged) {
            ActivateEffectParticles(toughFX);
            tough = true;
        } else if ((!playerManager.tough && tough) || (tough && playerManager.camouflaged)) {
            DeactivateEffectParticles(toughFX);
            tough = false;
        }

        if (playerManager.strengthened && !strengthened && !playerManager.camouflaged) {
            ActivateEffectParticles(strengthenFX);
            strengthened = true;
        } else if ((!playerManager.strengthened && strengthened) || (strengthened && playerManager.camouflaged)) {
            DeactivateEffectParticles(strengthenFX);
            strengthened = false;
        }

        if (playerManager.weakened && !weakened && !playerManager.camouflaged) {
            ActivateEffectParticles(weakenFX);
            weakened = true;
        } else if ((!playerManager.weakened && weakened) || (weakened && playerManager.camouflaged)) {
            DeactivateEffectParticles(weakenFX);
            weakened = false;
        }

        if (playerManager.slowFall && !slowfall && !playerManager.camouflaged) {
            ActivateEffectParticles(slowfallFX);
            slowfall = true;
        } else if ((!playerManager.slowFall && slowfall) || (slowfall && playerManager.camouflaged)) {
            DeactivateEffectParticles(slowfallFX);
            slowfall = false;
        }

        if (playerManager.IsHealing() && !healing && !playerManager.camouflaged) {
            ActivateEffectParticles(healingFX);
            healing = true;
        } else if ((!playerManager.IsHealing() && healing) || (healing && playerManager.camouflaged)) {
            DeactivateEffectParticles(healingFX);
            healing = false;
        }

        if (playerManager.manaRestorationChange && !playerManager.camouflaged) {
            if (playerManager.manaRestorationBuff) {
                if (!manaBuff) {
                    ActivateEffectParticles(manaBuffFX);
                    manaBuff = true;
                }
                
            } else {
                if (!manaDebuff) {
                    ActivateEffectParticles(manaDebuffFX);
                    manaDebuff = true;
                }
            }
        } else {
            if (manaBuff) {
                DeactivateEffectParticles(manaBuffFX);
                manaBuff = false;
            }
            if (manaDebuff) {
                DeactivateEffectParticles(manaDebuffFX);
                manaDebuff = false;
            }
        }
    }

    public void ActivateEffectParticles(List<GameObject> effects) {
        foreach(GameObject go in effects) {
            go.SetActive(true);
        }
    }

    public void DeactivateEffectParticles(List<GameObject> effects) {
        foreach(GameObject go in effects) {
            go.SetActive(false);
        }
    }

    public void PlayCureFX() {
        foreach(ParticleSystem go in cureFX) {
            go.Play();
        }
    }

    public void PlayCleanseFX() {
        foreach(ParticleSystem go in cleanseFX) {
            go.Play();
        }
    }

    public void PlayManaDrainFX() {
        foreach(ParticleSystem go in manaDrainFX) {
            go.Play();
        }
    }

    public void PlayHandParticle(int animationKey, AuricaSpell.ManaType manaType) {
        bool isOneHandedAnimation = animationKey == 0 || animationKey == 1 || animationKey == 2;
        // Debug.Log("ISONEHANDED: "+isOneHandedAnimation+ "   ANIMATION: "+animationKey);

        GameObject rHandParticles = r_auric, lHandParticles = l_auric;
        switch (manaType) {
            case AuricaSpell.ManaType.Auric:
                rHandParticles = r_auric;
                lHandParticles = l_auric;
                break;
            case AuricaSpell.ManaType.Order:
                rHandParticles = r_order;
                lHandParticles = l_order;
                break;
            case AuricaSpell.ManaType.Chaos:
                rHandParticles = r_chaos;
                lHandParticles = l_chaos;
                break;
            case AuricaSpell.ManaType.Life:
                rHandParticles = r_life;
                lHandParticles = l_life;
                break;
            case AuricaSpell.ManaType.Death:
                rHandParticles = r_death;
                lHandParticles = l_death;
                break;
            case AuricaSpell.ManaType.Fire:
                rHandParticles = r_fire;
                lHandParticles = l_fire;
                break;
            case AuricaSpell.ManaType.Water:
                rHandParticles = r_water;
                lHandParticles = l_water;
                break;
            case AuricaSpell.ManaType.Earth:
                rHandParticles = r_earth;
                lHandParticles = l_earth;
                break;
            case AuricaSpell.ManaType.Air:
                rHandParticles = r_air;
                lHandParticles = l_air;
                break;
            case AuricaSpell.ManaType.Divine:
                rHandParticles = r_divine;
                lHandParticles = l_divine;
                break;
            case AuricaSpell.ManaType.Demonic:
                rHandParticles = r_demonic;
                lHandParticles = l_demonic;
                break;
        }

        StopDefaultParticles();

        rHandParticles.SetActive(true);
        ParticleSystem[] particles = rHandParticles.GetComponentsInChildren<ParticleSystem>();
        foreach(var particle in particles) particle.Play();
        activeParticles.Add(rHandParticles);

        if (!isOneHandedAnimation) {
            lHandParticles.SetActive(true);
            particles = lHandParticles.GetComponentsInChildren<ParticleSystem>();
            foreach(var particle in particles) particle.Play();
            activeParticles.Add(lHandParticles);
        }
    }

    public void StopDefaultParticles() {
        if (!defaultParticlesPlaying) return;
        ParticleSystem[] particles = defaultParticles.GetComponentsInChildren<ParticleSystem>();
        foreach(var particle in particles) particle.Stop();
        defaultParticlesPlaying = false;
    }

    public void StartDefaultParticles() {
        if (defaultParticlesPlaying) return;
        ParticleSystem[] defaultParticleSystems = defaultParticles.GetComponentsInChildren<ParticleSystem>();
        foreach(var particle in defaultParticleSystems) particle.Play();
        defaultParticlesPlaying = true;
    }

    public void StopHandParticles(bool shouldStartDefaultParticles = true) {
        List<GameObject> disableParticles = activeParticles;
        ParticleSystem[] particles = new ParticleSystem[]{};
        foreach(var particleGO in disableParticles) {
            particles = particleGO.GetComponentsInChildren<ParticleSystem>();
            foreach(var particle in particles) particle.Stop();
        }

        if (shouldStartDefaultParticles) StartDefaultParticles();
    }

    public void PlayBoostParticles(){
        foreach(ParticleSystem go in boostFX) {
            go.Play();
        }
    }
}
