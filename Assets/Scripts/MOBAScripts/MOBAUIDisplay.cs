using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;

public class MOBAUIDisplay : MonoBehaviour {

    public static MOBAUIDisplay Instance;

    [Tooltip("Start button game object on the MOBA HUD")]
    [SerializeField]
    private GameObject MOBAMatchHUDStartButton;

    [Tooltip("Timer text")]
    [SerializeField]
    private Text TimerText;

    private MOBAMatchManager matchManager;

    void Awake() {
        MOBAUIDisplay.Instance = this;
    }

    void FixedUpdate() {
        if (matchManager == null) matchManager = MOBAMatchManager.Instance;
        if (matchManager != null) {
            if (matchManager.HasMatchStarted()) {
                var ts = TimeSpan.FromSeconds(matchManager.GetTimer());
                TimerText.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
            } else {
                TimerText.text = "0:00";
            }
        }
    }

    public void ToggleStartButton(bool flag) {
        MOBAMatchHUDStartButton.SetActive(flag);
    }
}
