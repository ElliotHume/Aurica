using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasteryUIPanel : MonoBehaviour {

    // slider = Mathf.Min(mastery, 10f) + (22 * Mathf.Clamp(mastery-10f, 0f, 90f)/100f) + (34 * Mathf.Clamp(mastery-110f, 0f, 890f)/1000f) + (45 * Mathf.Clamp(mastery-1110f, 0f, 8890f)/10000f)

}
