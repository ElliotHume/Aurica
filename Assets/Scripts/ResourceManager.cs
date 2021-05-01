using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    public Sprite AuricIcon, OrderIcon, ChaosIcon, LifeIcon, DeathIcon, FireIcon, WaterIcon, EarthIcon, AirIcon, DivineIcon, DemonicIcon;
    public Color AuricColor, OrderColor, ChaosColor, LifeColor, DeathColor, FireColor, WaterColor, EarthColor, AirColor, DivineColor, DemonicColor;


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
    
}
