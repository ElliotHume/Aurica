using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioController : MonoBehaviour {
    public Slider volumeSlider;
    float Volume;

    // Start is called before the first frame update
    void Start() {
        if (PlayerPrefs.HasKey("Volume")){
            Volume = PlayerPrefs.GetFloat("Volume");
            AudioListener.volume = Volume;
            if (volumeSlider != null) volumeSlider.value = Volume;
        }
    }

    public void ChangeVolume(float newVolume) {
        Volume = newVolume;
        AudioListener.volume = Volume;
        PlayerPrefs.SetFloat("Volume", Volume);
    }
}
