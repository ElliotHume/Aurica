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
    public float addedManaCost = 0f, errorThreshold = 3.0f;

    public AuricaSpell OrderSpell = null;
    public AuricaSpell ChaosSpell = null;
    public AuricaSpell LifeSpell = null;
    public AuricaSpell DeathSpell = null;
    public AuricaSpell FireSpell = null;
    public AuricaSpell WaterSpell = null;
    public AuricaSpell EarthSpell = null;
    public AuricaSpell AirSpell = null;
    public AuricaSpell DivineSpell = null;
    public AuricaSpell DemonicSpell = null;



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

        // Debug.Log("PURE CHECKS:  "+hasCorrectBasis+"  "+hasCorrectForm+"  "+hasCorrectFocus+"  "+hasCorrectAction);

        return hasCorrectBasis && hasCorrectForm && hasCorrectFocus && hasCorrectAction;
    }

    public float GetError(AuricaSpell.ManaType manaType, ManaDistribution checkDist) {
        switch(manaType) {
            case AuricaSpell.ManaType.Order:
                return OrderSpell.targetDistribution.CheckDistError(checkDist);
            case AuricaSpell.ManaType.Chaos:
                return ChaosSpell.targetDistribution.CheckDistError(checkDist);
            case AuricaSpell.ManaType.Life:
                return LifeSpell.targetDistribution.CheckDistError(checkDist);
            case AuricaSpell.ManaType.Death:
                return DeathSpell.targetDistribution.CheckDistError(checkDist);
            case AuricaSpell.ManaType.Fire:
                return FireSpell.targetDistribution.CheckDistError(checkDist);
            case AuricaSpell.ManaType.Water:
                return WaterSpell.targetDistribution.CheckDistError(checkDist);
            case AuricaSpell.ManaType.Earth:
                return EarthSpell.targetDistribution.CheckDistError(checkDist);
            case AuricaSpell.ManaType.Air:
                return AirSpell.targetDistribution.CheckDistError(checkDist);
            case AuricaSpell.ManaType.Divine:
                return DivineSpell.targetDistribution.CheckDistError(checkDist);
            case AuricaSpell.ManaType.Demonic:
                return DemonicSpell.targetDistribution.CheckDistError(checkDist);
        }
        return 0f;
    }

    public AuricaSpell.ManaType GetManaType(ManaDistribution distribution) {
        float bestError = 999f;
        AuricaSpell.ManaType manaType = AuricaSpell.ManaType.Auric;

        if (OrderSpell != null) {
            bestError = GetError(AuricaSpell.ManaType.Order, distribution);
            manaType = AuricaSpell.ManaType.Order;
        }
        if (ChaosSpell != null && bestError > GetError(AuricaSpell.ManaType.Chaos, distribution)) {
            bestError = GetError(AuricaSpell.ManaType.Chaos, distribution);
            manaType = AuricaSpell.ManaType.Chaos;
        }
        if (LifeSpell != null && bestError > GetError(AuricaSpell.ManaType.Life, distribution)) {
            bestError = GetError(AuricaSpell.ManaType.Life, distribution);
            manaType = AuricaSpell.ManaType.Life;
        }
        if (DeathSpell != null && bestError > GetError(AuricaSpell.ManaType.Death, distribution)) {
            bestError = GetError(AuricaSpell.ManaType.Death, distribution);
            manaType = AuricaSpell.ManaType.Death;
        }
        if (FireSpell != null && bestError > GetError(AuricaSpell.ManaType.Fire, distribution)) {
            bestError = GetError(AuricaSpell.ManaType.Fire, distribution);
            manaType = AuricaSpell.ManaType.Fire;
        }
        if (WaterSpell != null && bestError > GetError(AuricaSpell.ManaType.Water, distribution)) {
            bestError = GetError(AuricaSpell.ManaType.Water, distribution);
            manaType = AuricaSpell.ManaType.Water;
        }
        if (EarthSpell != null && bestError > GetError(AuricaSpell.ManaType.Earth, distribution)) {
            bestError = GetError(AuricaSpell.ManaType.Earth, distribution);
            manaType = AuricaSpell.ManaType.Earth;
        }
        if (AirSpell != null && bestError > GetError(AuricaSpell.ManaType.Air, distribution)) {
            bestError = GetError(AuricaSpell.ManaType.Air, distribution);
            manaType = AuricaSpell.ManaType.Air;
        }
        if (DivineSpell != null && bestError > GetError(AuricaSpell.ManaType.Divine, distribution)) {
            bestError = GetError(AuricaSpell.ManaType.Divine, distribution);
            manaType = AuricaSpell.ManaType.Divine;
        }
        if (DemonicSpell != null && bestError > GetError(AuricaSpell.ManaType.Demonic, distribution)) {
            bestError = GetError(AuricaSpell.ManaType.Demonic, distribution);
            manaType = AuricaSpell.ManaType.Demonic;
        }

        return manaType;
    }

    public string GetSpellResourceName(AuricaSpell.ManaType manaType) {
        switch(manaType) {
            case (AuricaSpell.ManaType.Order):
                return OrderSpell.linkedSpellResource;
            case (AuricaSpell.ManaType.Chaos):
                return ChaosSpell.linkedSpellResource;
            case (AuricaSpell.ManaType.Life):
                return LifeSpell.linkedSpellResource;
            case (AuricaSpell.ManaType.Death):
                return DeathSpell.linkedSpellResource;
            case (AuricaSpell.ManaType.Fire ):
                return FireSpell.linkedSpellResource;
            case (AuricaSpell.ManaType.Water):
                return WaterSpell.linkedSpellResource;
            case (AuricaSpell.ManaType.Earth):
                return EarthSpell.linkedSpellResource;
            case (AuricaSpell.ManaType.Air):
                return AirSpell.linkedSpellResource;
            case (AuricaSpell.ManaType.Divine):
                return DivineSpell.linkedSpellResource;
            case (AuricaSpell.ManaType.Demonic):
                return DemonicSpell.linkedSpellResource;
        }
        return "";
    }

    public AuricaSpell GetAuricaSpell(AuricaSpell.ManaType manaType) {
        switch(manaType) {
            case (AuricaSpell.ManaType.Order):
                return OrderSpell;
            case (AuricaSpell.ManaType.Chaos):
                return ChaosSpell;
            case (AuricaSpell.ManaType.Life):
                return LifeSpell;
            case (AuricaSpell.ManaType.Death):
                return DeathSpell;
            case (AuricaSpell.ManaType.Fire ):
                return FireSpell;
            case (AuricaSpell.ManaType.Water):
                return WaterSpell;
            case (AuricaSpell.ManaType.Earth):
                return EarthSpell;
            case (AuricaSpell.ManaType.Air):
                return AirSpell;
            case (AuricaSpell.ManaType.Divine):
                return DivineSpell;
            case (AuricaSpell.ManaType.Demonic):
                return DemonicSpell;
        }
        return null;
    }
    
}