using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ManaDistribution {
    public float structure, essence, fire, water, earth, air, nature;


    public ManaDistribution(string dist) {
        string[] distributionSeperator = new string[] { ", " };
        string[] splitDist = dist.Split(distributionSeperator, System.StringSplitOptions.None);
        structure = float.Parse(splitDist[0]);
        essence = float.Parse(splitDist[1]);
        fire = float.Parse(splitDist[2]);
        water = float.Parse(splitDist[3]);
        earth = float.Parse(splitDist[4]);
        air = float.Parse(splitDist[5]);
        nature = float.Parse(splitDist[6]);
    }

    public ManaDistribution(float _structure, float _essence, float _fire, float _water, float _earth, float _air, float _nature) {
        structure = _structure;
        essence = _essence;
        fire = _fire;
        water = _water;
        earth = _earth;
        air = _air;
        nature = _nature;
    }

    public string GetJson() {
        return JsonUtility.ToJson(this);
    }

    public override string ToString() {
        return "" + structure + ", " + essence + ", " + fire + ", " + water + ", " + earth + ", " + air + ", " + nature;
    }

    public List<float> ToList() {
        List<float> ls = new List<float>();
        ls.Add(structure);
        ls.Add(essence);
        ls.Add(fire);
        ls.Add(water);
        ls.Add(earth);
        ls.Add(air);
        ls.Add(nature);
        return ls;
    }

    public float GetAggregate() {
        return Mathf.Abs(structure) + Mathf.Abs(essence) + Mathf.Abs(fire) + Mathf.Abs(water) + Mathf.Abs(earth) + Mathf.Abs(air) + Mathf.Abs(nature);
    }

    public bool IsEmpty() {
        return structure == 0f && essence == 0f && fire == 0f && water == 0f && earth == 0f && air == 0f && nature == 0f;
    }

    public static ManaDistribution operator +(ManaDistribution a, ManaDistribution b)
        => new ManaDistribution(a.structure + b.structure, a.essence + b.essence, a.fire + b.fire, a.water + b.water, a.earth + b.earth, a.air + b.air, a.nature + b.nature);

    public static ManaDistribution operator -(ManaDistribution a, ManaDistribution b)
        => new ManaDistribution(a.structure - b.structure, a.essence - b.essence, a.fire - b.fire, a.water - b.water, a.earth - b.earth, a.air - b.air, a.nature - b.nature);

    public static ManaDistribution operator *(ManaDistribution a, ManaDistribution b)
        => new ManaDistribution(a.structure * b.structure, a.essence * b.essence, a.fire * b.fire, a.water * b.water, a.earth * b.earth, a.air * b.air, a.nature * b.nature);

    public static ManaDistribution operator *(ManaDistribution a, float b)
        => new ManaDistribution(a.structure * b, a.essence * b, a.fire * b, a.water * b, a.earth * b, a.air * b, a.nature * b);

    public List<float> GetAsPercentages() {
        List<float> percents = new List<float>();
        float aggregate = GetAggregate();
        if (aggregate == 0f) return percents;
        percents.Add(Mathf.Abs(structure) / aggregate);
        percents.Add(Mathf.Abs(essence) / aggregate);
        percents.Add(fire / aggregate);
        percents.Add(water / aggregate);
        percents.Add(earth / aggregate);
        percents.Add(air / aggregate);
        percents.Add(Mathf.Abs(nature) / aggregate);
        return percents;
    }

    public float CheckDistError(ManaDistribution dist) {
        float error = 0f;
        error += GetAlignedErrorFunc(structure, dist.structure);
        error += GetAlignedErrorFunc(essence, dist.essence);
        error += GetAlignedErrorFunc(nature, dist.nature);

        error += Mathf.Abs(fire - dist.fire);
        error += Mathf.Abs(water - dist.water);
        error += Mathf.Abs(earth - dist.earth);
        error += Mathf.Abs(air - dist.air);

        // Debug.Log("Structure Error" + GetAlignedErrorFunc(structure, dist.structure));
        // Debug.Log("Essence Error" + GetAlignedErrorFunc(essence, dist.essence));
        // Debug.Log("Nature Error" + GetAlignedErrorFunc(nature, dist.nature));
        return error;
    }

    public static float GetAlignedErrorFunc(float target, float actual) {
        if (target >= 0f && actual >= 0f) {
            return target > actual ? target - actual : actual - target;
        } else if (target <= 0f && actual <= 0f) {
            return target < actual ? Mathf.Abs(target - actual) : Mathf.Abs(actual - target);
        } else if (target >= 0f && actual <= 0f) {
            return Mathf.Abs(target) + Mathf.Abs(actual);
        } else if (target <= 0f && actual >= 0f) {
            return Mathf.Abs(actual) + Mathf.Abs(target);
        }
        return 0f;
    }

    public void AddBasicDist(ManaDistribution other) {
        structure += other.structure;
        essence += other.essence;
        fire += other.fire;
        water += other.water;
        earth += other.earth;
        air += other.air;
        nature += other.nature;
    }

    public void AddAuricDist(ManaDistribution auricDist, ManaDistribution aura) {
        structure += auricDist.structure * aura.structure;
        essence += auricDist.essence * aura.essence;
        fire += auricDist.fire * aura.fire;
        water += auricDist.water * aura.water;
        earth += auricDist.earth * aura.earth;
        air += auricDist.air * aura.air;
        nature += auricDist.nature * aura.nature;
    }

    public void ApplyFluxDist(ManaDistribution fluxDist) {
        // Aligned mana moves towards the midpoint of 0
        structure -= structure * fluxDist.structure;
        essence -= essence * fluxDist.essence;
        nature -= nature * fluxDist.nature;

        // Elemental mana moves towards the average of the elements
        float midpoint = GetElementalAverage();
        // Fire
        if (fire > midpoint) {
            fire -= (fire - midpoint) * fluxDist.fire;
        } else if (fire < midpoint) {
            fire += (midpoint - fire) * fluxDist.fire;
        }
        // Water
        if (water > midpoint) {
            water -= (water - midpoint) * fluxDist.water;
        } else if (water < midpoint) {
            water += (midpoint - water) * fluxDist.water;
        }
        // Earth
        if (earth > midpoint) {
            earth -= (earth - midpoint) * fluxDist.earth;
        } else if (earth < midpoint) {
            earth += (midpoint - earth) * fluxDist.earth;
        }
        // Air
        if (air > midpoint) {
            air -= (air - midpoint) * fluxDist.air;
        } else if (air < midpoint) {
            air += (midpoint - air) * fluxDist.air;
        }
    }

    public void ApplySiphon(ManaDistribution siphonDist) {
        // Find the minimum of the siphon and current distribution, so that you arent reducing mana by more than what exists
        ManaDistribution siphon = new ManaDistribution(
            siphonDist.structure >= 0f ? Mathf.Min(Mathf.Max(0f, structure), siphonDist.structure) : Mathf.Max(Mathf.Min(0f, structure), siphonDist.structure),
            siphonDist.essence >= 0f ? Mathf.Min(Mathf.Max(0f, essence), siphonDist.essence) : Mathf.Max(Mathf.Min(0f, essence), siphonDist.essence),
            Mathf.Min(fire, siphonDist.fire),
            Mathf.Min(water, siphonDist.water),
            Mathf.Min(earth, siphonDist.earth),
            Mathf.Min(air, siphonDist.air),
            siphonDist.nature >= 0f ? Mathf.Min(Mathf.Max(0f, nature), siphonDist.nature) : Mathf.Max(Mathf.Min(0f, nature), siphonDist.nature)
        );
        // Debug.Log("Siphon Distribution "+siphon.ToString());
        // Water, Earth and Divine add structure, Fire, Earth and Demonic reduce it
        structure += (0.3f * siphon.water) + (0.3f * siphon.earth) - (0.3f * siphon.fire) - (0.3f * siphon.air) + (0.5f * siphon.nature) - siphon.structure;
        // Water and Air add essence, Earth and Fire reduce it
        essence += (0.1f * siphon.water) - (0.1f * siphon.earth) - (0.1f * siphon.fire) + (0.1f * siphon.air) - siphon.essence;
        // Demonic increases fire
        fire += (0.5f * Mathf.Max(0f, -siphon.nature)) - siphon.fire;
        // Divine increases air
        air += (0.5f * Mathf.Max(0f, siphon.nature)) - siphon.air;

        // No siphon interactions
        water -= siphon.water;
        earth -= siphon.earth;
        nature -= siphon.nature;
        // Debug.Log("Post-Siphon Distribution "+this.ToString());
    }

    public void SubtractBasicDist(ManaDistribution other) {
        structure -= other.structure;
        essence -= other.essence;
        fire -= other.fire;
        water -= other.water;
        earth -= other.earth;
        air -= other.air;
        nature -= other.nature;
    }

    public void SubtractAuricDist(ManaDistribution auricDist, ManaDistribution aura) {
        structure -= auricDist.structure * aura.structure;
        essence -= auricDist.essence * aura.essence;
        fire -= auricDist.fire * aura.fire;
        water -= auricDist.water * aura.water;
        earth -= auricDist.earth * aura.earth;
        air -= auricDist.air * aura.air;
        nature -= auricDist.nature * aura.nature;
    }

    public void ClampElementalValues() {
        if (fire < 0f) fire = 0f;
        if (water < 0f) water = 0f;
        if (earth < 0f) earth = 0f;
        if (air < 0f) air = 0f;
    }

    public float GetElementalAverage() {
        return (fire + water + earth + air) / 4f;
    }

    public void PopulateFromList(List<float> ls) {
        structure = ls[0];
        essence = ls[1];
        fire = ls[2];
        water = ls[3];
        earth = ls[4];
        air = ls[5];
        nature = ls[6];
    }

    public float GetDamage(float damage, ManaDistribution damageDist) {
        List<float> percents = damageDist.GetAsPercentages();
        if (percents.Count == 0) return damage;
        float structureDiff = damageDist.structure - structure;
        float essenceDiff = damageDist.essence - essence;
        float natureDiff = damageDist.nature - nature;
        percents[0] = percents[0] * damage * (1f - (structureDiff < 0 ? structureDiff : Mathf.Abs(structure * 0.75f)));
        percents[1] = percents[1] * damage * (1f - (essenceDiff < 0 ? essenceDiff : Mathf.Abs(essence * 0.75f)));
        percents[2] = percents[2] * damage * (1f - (fire * 0.75f));
        percents[3] = percents[3] * damage * (1f - (water * 0.75f));
        percents[4] = percents[4] * damage * (1f - (earth * 0.75f));
        percents[5] = percents[5] * damage * (1f - (air * 0.75f));
        percents[6] = percents[6] * damage * (1f - (natureDiff < 0 ? natureDiff : Mathf.Abs(damageDist.nature * 0.75f)));

        float sum = 0;
        foreach (var element in percents) {
            sum += element;
        }

        return sum;
    }

    public ManaDistribution GetInverted() {
        return new ManaDistribution(1,1,1,1,1,1,1) - this;
    }
}