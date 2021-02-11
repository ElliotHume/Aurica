using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkDestroyAfterTime : MonoBehaviour {

    public float Lifetime;
    // Start is called before the first frame update
    void Start() {
        Invoke("XDestroy", Lifetime);
    }

    void XDestroy() {
        PhotonNetwork.Destroy(gameObject);
    }
}
