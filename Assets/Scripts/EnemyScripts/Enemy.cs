using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Enemy : MonoBehaviourPunCallbacks {
    public string c_name = "";
    public float Health = 100f;
    protected float maxHealth;

    // Patrolling
    public bool doesPatrol = false;
    public float walkPointRange = 10f, idleTime = 6f;
    protected Vector3 walkPoint, startPoint;
    protected bool walkPointSet;
}
