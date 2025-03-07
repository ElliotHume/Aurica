using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    public Sprite AuricIcon, OrderIcon, ChaosIcon, LifeIcon, DeathIcon, FireIcon, WaterIcon, EarthIcon, AirIcon, DivineIcon, DemonicIcon, PureIcon;
    public Color AuricColor, OrderColor, ChaosColor, LifeColor, DeathColor, FireColor, WaterColor, EarthColor, AirColor, DivineColor, DemonicColor, PureColor;
    public Sprite AuricMasteryIcon, OrderMasteryIcon, ChaosMasteryIcon, LifeMasteryIcon, DeathMasteryIcon, FireMasteryIcon, WaterMasteryIcon, EarthMasteryIcon, AirMasteryIcon, DivineMasteryIcon, DemonicMasteryIcon;


    // Start is called before the first frame update
    void Start() {
        Instance = this;
    }

    public Sprite GetIcon(AuricaSpell.ManaType manaType) {
        switch (manaType) {
            case AuricaSpell.ManaType.Auric:
                return AuricIcon;
            case AuricaSpell.ManaType.Order:
                return OrderIcon;
            case AuricaSpell.ManaType.Chaos:
                return ChaosIcon;
            case AuricaSpell.ManaType.Life:
                return LifeIcon;
            case AuricaSpell.ManaType.Death:
                return DeathIcon;
            case AuricaSpell.ManaType.Fire:
                return FireIcon;
            case AuricaSpell.ManaType.Water:
                return WaterIcon;
            case AuricaSpell.ManaType.Earth:
                return EarthIcon;
            case AuricaSpell.ManaType.Air:
                return AirIcon;
            case AuricaSpell.ManaType.Divine:
                return DivineIcon;
            case AuricaSpell.ManaType.Demonic:
                return DemonicIcon;
        }
        return AuricIcon;
    }

    public Sprite GetMasteryIcon(AuricaSpell.ManaType manaType) {
        switch (manaType) {
            case AuricaSpell.ManaType.Auric:
                return AuricMasteryIcon;
            case AuricaSpell.ManaType.Order:
                return OrderMasteryIcon;
            case AuricaSpell.ManaType.Chaos:
                return ChaosMasteryIcon;
            case AuricaSpell.ManaType.Life:
                return LifeMasteryIcon;
            case AuricaSpell.ManaType.Death:
                return DeathMasteryIcon;
            case AuricaSpell.ManaType.Fire:
                return FireMasteryIcon;
            case AuricaSpell.ManaType.Water:
                return WaterMasteryIcon;
            case AuricaSpell.ManaType.Earth:
                return EarthMasteryIcon;
            case AuricaSpell.ManaType.Air:
                return AirMasteryIcon;
            case AuricaSpell.ManaType.Divine:
                return DivineMasteryIcon;
            case AuricaSpell.ManaType.Demonic:
                return DemonicMasteryIcon;
        }
        return AuricMasteryIcon;
    }

    public Color GetColor(AuricaSpell.ManaType manaType) {
        switch (manaType) {
            case AuricaSpell.ManaType.Auric:
                return AuricColor;
            case AuricaSpell.ManaType.Order:
                return OrderColor;
            case AuricaSpell.ManaType.Chaos:
                return ChaosColor;
            case AuricaSpell.ManaType.Life:
                return LifeColor;
            case AuricaSpell.ManaType.Death:
                return DeathColor;
            case AuricaSpell.ManaType.Fire:
                return FireColor;
            case AuricaSpell.ManaType.Water:
                return WaterColor;
            case AuricaSpell.ManaType.Earth:
                return EarthColor;
            case AuricaSpell.ManaType.Air:
                return AirColor;
            case AuricaSpell.ManaType.Divine:
                return DivineColor;
            case AuricaSpell.ManaType.Demonic:
                return DemonicColor;
        }
        return Color.white;
    }

    public Sprite GetPureIcon() {
        return PureIcon;
    }
    
    public Color GetPureColor() {
        return PureColor;
    }
}
