using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[CreateAssetMenu(fileName = "AuricaPureSpell", menuName = "Aurica/AuricaPureSpell", order = 1)]
public class AuricaPureSpell : ScriptableObject {

    // Presets
    public string c_name;
    [TextArea(15, 3)]
    public string description;
    public AuricaSpellComponent.Classification SpellBasis;
    public AuricaSpellComponent.Classification SpellForm;
    public AuricaSpellComponent.Classification SpellFocus;
    public AuricaSpellComponent.Classification SpellAction;
    public float errorThreshold = 3.0f;

    public bool OrderCast;
    public ManaDistribution TargetOrderDistribution;
    public string OrderSpellResource;

    public bool ChaosCast;
    public ManaDistribution TargetChaosDistribution;
    public string ChaosSpellResource;
    
    public bool LifeCast;
    public ManaDistribution TargetLifeDistribution;
    public string LifeSpellResource;
    
    public bool DeathCast;
    public ManaDistribution TargetDeathDistribution;
    public string DeathSpellResource;
    
    public bool FireCast;
    public ManaDistribution TargetFireDistribution;
    public string FireSpellResource;
    
    public bool WaterCast;
    public ManaDistribution TargetWaterDistribution;
    public string WaterSpellResource;
    
    public bool EarthCast;
    public ManaDistribution TargetEarthDistribution;
    public string EarthSpellResource;
    
    public bool AirCast;
    public ManaDistribution TargetAirDistribution;
    public string AirSpellResource;
    
    public bool DivineCast;
    public ManaDistribution TargetDivineDistribution;
    public string DivineSpellResource;
    
    public bool DemonicCast;
    public ManaDistribution TargetDemonicDistribution;
    public string DemonicSpellResource;


    public bool CheckComponents(List<AuricaSpellComponent> components, ManaDistribution distribution) {
        bool hasCorrectBasis = false, hasCorrectForm = false, hasCorrectFocus = false, hasCorrectAction = false;
        foreach(AuricaSpellComponent component in components) {
            if (component.category == AuricaSpellComponent.Category.SpellBasis) {
                hasCorrectBasis = component.classification == SpellBasis || hasCorrectBasis;
            } else if (component.category == AuricaSpellComponent.Category.SpellForm) {
                hasCorrectForm = component.classification == SpellForm || hasCorrectForm;
            } else if (component.category == AuricaSpellComponent.Category.SpellFocus) {
                hasCorrectFocus = component.classification == SpellFocus || hasCorrectFocus;
            } else if (component.category == AuricaSpellComponent.Category.SpellAction) {
                hasCorrectAction = component.classification == SpellAction || hasCorrectAction;
            }
        }

        return hasCorrectBasis && hasCorrectForm && hasCorrectFocus && hasCorrectAction;
    }

    public float GetError(AuricaSpell.ManaType manaType, ManaDistribution checkDist) {
        switch(manaType) {
            case AuricaSpell.ManaType.Order:
                return TargetOrderDistribution.CheckDistError(checkDist);
            case AuricaSpell.ManaType.Chaos:
                return TargetChaosDistribution.CheckDistError(checkDist);
            case AuricaSpell.ManaType.Life:
                return TargetLifeDistribution.CheckDistError(checkDist);
            case AuricaSpell.ManaType.Death:
                return TargetDeathDistribution.CheckDistError(checkDist);
            case AuricaSpell.ManaType.Fire:
                return TargetFireDistribution.CheckDistError(checkDist);
            case AuricaSpell.ManaType.Water:
                return TargetWaterDistribution.CheckDistError(checkDist);
            case AuricaSpell.ManaType.Earth:
                return TargetEarthDistribution.CheckDistError(checkDist);
            case AuricaSpell.ManaType.Air:
                return TargetAirDistribution.CheckDistError(checkDist);
            case AuricaSpell.ManaType.Divine:
                return TargetDivineDistribution.CheckDistError(checkDist);
            case AuricaSpell.ManaType.Demonic:
                return TargetDemonicDistribution.CheckDistError(checkDist);
        }
        return 0f;
    }

    public AuricaSpell.ManaType GetManaType(ManaDistribution distribution) {
        float bestError = 999f;
        AuricaSpell.ManaType manaType = AuricaSpell.ManaType.Auric;

        if (OrderCast) {
            bestError = GetError(AuricaSpell.ManaType.Order, distribution);
            manaType = AuricaSpell.ManaType.Order;
        }
        if (ChaosCast && bestError > GetError(AuricaSpell.ManaType.Chaos, distribution)) {
            bestError = GetError(AuricaSpell.ManaType.Chaos, distribution);
            manaType = AuricaSpell.ManaType.Chaos;
        }
        if (LifeCast && bestError > GetError(AuricaSpell.ManaType.Life, distribution)) {
            bestError = GetError(AuricaSpell.ManaType.Life, distribution);
            manaType = AuricaSpell.ManaType.Life;
        }
        if (DeathCast && bestError > GetError(AuricaSpell.ManaType.Death, distribution)) {
            bestError = GetError(AuricaSpell.ManaType.Death, distribution);
            manaType = AuricaSpell.ManaType.Death;
        }
        if (FireCast && bestError > GetError(AuricaSpell.ManaType.Fire, distribution)) {
            bestError = GetError(AuricaSpell.ManaType.Fire, distribution);
            manaType = AuricaSpell.ManaType.Fire;
        }
        if (WaterCast && bestError > GetError(AuricaSpell.ManaType.Water, distribution)) {
            bestError = GetError(AuricaSpell.ManaType.Water, distribution);
            manaType = AuricaSpell.ManaType.Water;
        }
        if (EarthCast && bestError > GetError(AuricaSpell.ManaType.Earth, distribution)) {
            bestError = GetError(AuricaSpell.ManaType.Earth, distribution);
            manaType = AuricaSpell.ManaType.Earth;
        }
        if (AirCast && bestError > GetError(AuricaSpell.ManaType.Air, distribution)) {
            bestError = GetError(AuricaSpell.ManaType.Air, distribution);
            manaType = AuricaSpell.ManaType.Air;
        }
        if (DivineCast && bestError > GetError(AuricaSpell.ManaType.Divine, distribution)) {
            bestError = GetError(AuricaSpell.ManaType.Divine, distribution);
            manaType = AuricaSpell.ManaType.Divine;
        }
        if (DemonicCast && bestError > GetError(AuricaSpell.ManaType.Demonic, distribution)) {
            bestError = GetError(AuricaSpell.ManaType.Demonic, distribution);
            manaType = AuricaSpell.ManaType.Demonic;
        }

        return manaType;
    }

    public string GetSpellResourceName(AuricaSpell.ManaType manaType) {
        switch(manaType) {
            case (AuricaSpell.ManaType.Order):
                return OrderSpellResource;
            case (AuricaSpell.ManaType.Chaos):
                return ChaosSpellResource;
            case (AuricaSpell.ManaType.Life):
                return LifeSpellResource;
            case (AuricaSpell.ManaType.Death):
                return DeathSpellResource;
            case (AuricaSpell.ManaType.Fire):
                return FireSpellResource;
            case (AuricaSpell.ManaType.Water):
                return WaterSpellResource;
            case (AuricaSpell.ManaType.Earth):
                return EarthSpellResource;
            case (AuricaSpell.ManaType.Air):
                return AirSpellResource;
            case (AuricaSpell.ManaType.Divine):
                return DivineSpellResource;
            case (AuricaSpell.ManaType.Demonic):
                return DemonicSpellResource;
        }
        return "";
    }
    
}