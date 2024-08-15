using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameModeManager : MonoBehaviourPunCallbacks
{
    [HideInInspector] public int maxTime;
    [HideInInspector] public float timeRemain;
    [HideInInspector] public bool isGameEnd;

    public void ResetGameTimer()
    {
        timeRemain = maxTime;
    }
}
