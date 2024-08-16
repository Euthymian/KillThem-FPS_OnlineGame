using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;

public class GameModeManager : MonoBehaviourPunCallbacks
{
    [HideInInspector] public int maxTime;
    [HideInInspector] public float timeRemain;
    [HideInInspector] public UnityEvent OnGameEndEvent = new UnityEvent();
    [HideInInspector] public UnityEvent OffGameEndEvent = new UnityEvent();

    public void ResetGameTimer()
    {
        timeRemain = maxTime;
    }
}
