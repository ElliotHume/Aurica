using UnityEngine;
using UnityEngine.UI;

public class SampleListener : MonoBehaviour {

    public Text result;

    public void GlyphCastResult(int index, AdVd.GlyphRecognition.GlyphMatch match)
    {
        if (match == null || match.Cost > match.Threshold) result.text = "Match not found";
        else result.text = match.target.name;
    }

}
