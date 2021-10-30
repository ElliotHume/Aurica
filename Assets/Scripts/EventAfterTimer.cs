using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventAfterTimer : MonoBehaviour
{
    public float timer;
    public UnityEvent OnTimerFinish;
    
    void OnEnable() {
        Invoke("TimerFinish", timer);
    }

    private void TimerFinish() {
        OnTimerFinish.Invoke();
    }

}
