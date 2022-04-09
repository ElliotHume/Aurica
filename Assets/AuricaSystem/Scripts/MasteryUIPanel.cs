using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasteryUIPanel : MonoBehaviour {
    void OnEnable() {
        if (MasteryManager.Instance != null && !MasteryManager.Instance.synced) MasteryManager.Instance.SyncMasteries();
    }
}
