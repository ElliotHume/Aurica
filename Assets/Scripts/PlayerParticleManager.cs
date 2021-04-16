using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParticleManager : MonoBehaviour
{
    public GameObject r_auric, r_order, r_chaos, r_life, r_death, r_fire, r_water, r_earth, r_air, r_divine, r_demonic;
    public GameObject l_auric, l_order, l_chaos, l_life, l_death, l_fire, l_water, l_earth, l_air, l_divine, l_demonic;

    List<int> oneHandedAnimations = new List<int>();
    List<GameObject> activeParticles = new List<GameObject>();

    void Start() {
        oneHandedAnimations.Add(0);
        oneHandedAnimations.Add(1);
        oneHandedAnimations.Add(2);
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

    public void StopHandParticles() {
        StopAllCoroutines();
        StartCoroutine(DisableParticles());
    }

    IEnumerator DisableParticles() {
        List<GameObject> disableParticles = activeParticles;
        ParticleSystem[] particles = new ParticleSystem[]{};
        foreach(var particleGO in disableParticles) {
            particles = particleGO.GetComponentsInChildren<ParticleSystem>();
            foreach(var particle in particles) particle.Stop();
        }
        yield return new WaitForSeconds(3f);
        foreach(var particleGO in disableParticles) {
            particleGO.SetActive(false);
        }
    }
}
