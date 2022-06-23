using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour {

    public static SettingsManager Instance;

    void Start() {
        Instance = this;
    }

    public void ChangeGraphicsTier(int level) {
        switch (level) {
            case 1:
                QualitySettings.SetQualityLevel(5, true);
                break;
            case 2:
                QualitySettings.SetQualityLevel(3, true);
                break;
            case 3:
                QualitySettings.SetQualityLevel(1, true);
                break;
        }
    }
}
