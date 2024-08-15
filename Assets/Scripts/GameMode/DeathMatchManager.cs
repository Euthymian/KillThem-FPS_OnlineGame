using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;
using Photon.Realtime;

public class DeathMatchManager : GameModeManager
{
    int timeForDeathMatch = 4;

    [SerializeField] TMP_Text minuteText;
    [SerializeField] TMP_Text secondText;

    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Canvas leaveRoomCanvas;

    [SerializeField] GameObject startButton;
    [SerializeField] GameObject readyButton;

    bool readyToLoad = true;

    private void Start()
    {
        maxTime = timeForDeathMatch;
        timeRemain = maxTime;
        PhotonNetwork.AutomaticallySyncScene = false;
        InvokeRepeating(nameof(GetMinutes), 0, 1f);
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
                startButton.SetActive(PhotonNetwork.IsMasterClient);
                readyButton.SetActive(!PhotonNetwork.IsMasterClient);
            }

            readyToLoad = false;
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startButton.SetActive(PhotonNetwork.IsMasterClient);
        readyButton.SetActive(!PhotonNetwork.IsMasterClient);
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
