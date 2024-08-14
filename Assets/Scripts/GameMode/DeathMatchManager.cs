using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;

public class DeathMatchManager : GameModeManager
{
    [SerializeField] int timeForDeathMatch = 4;

    [SerializeField] TMP_Text minuteText;
    [SerializeField] TMP_Text secondText;

    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Canvas leaveRoomCanvas;

    bool readyToLoad = true;

    private void Start()
    {
        maxTime = timeForDeathMatch;
        timeRemain = maxTime;
        PhotonNetwork.AutomaticallySyncScene = false;
        InvokeRepeating(nameof(GetMinutes), 0.1f, 40f);
        InvokeRepeating(nameof (GetSeconds), 0, 0.8f);
    }

    private void Update()
    {
        if(timeRemain > 0)
            timeRemain -= Time.deltaTime;

        if(timeRemain <= 0)
        {
            if(readyToLoad)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                foreach (var item in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
                {
                    Destroy(item.gameObject);
                }

                canvasGroup.alpha = 1.0f;
                leaveRoomCanvas.gameObject.SetActive(true);
            }

            readyToLoad = false;
        }
    }

    void GetMinutes()
    {
        minuteText.text = ((int)(timeRemain / 60)).ToString();
    }

    void GetSeconds()
    {
        secondText.text = Mathf.Ceil((int)timeRemain - (int)(timeRemain/60)*60).ToString();
    }
}
