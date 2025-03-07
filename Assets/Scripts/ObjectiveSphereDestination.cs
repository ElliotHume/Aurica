using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ObjectiveSphereDestination : MonoBehaviourPun {
    void OnTriggerEnter(Collider collider) {
        ObjectiveSphere os = collider.gameObject.GetComponent<ObjectiveSphere>();
        if (os != null) {
            os.ObjectiveComplete();
        }
    }
}
