using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;

public class DeathMatchManager : MonoBehaviour
{
    int maxTime = 10;
    float timeRemain;

    [SerializeField] TMP_Text minuteText;
    [SerializeField] TMP_Text secondText;

    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Canvas leaveRoomCanvas;

    bool readyToLoad = true;

    private void Start()
    {
        timeRemain = maxTime;
        PhotonNetwork.AutomaticallySyncScene = false;
        InvokeRepeating(nameof(GetMinutes), 0, 40f);
        InvokeRepeating(nameof (GetSeconds), 0, 0.5f);
    }

    private void Update()
    {
        if(timeRemain > 0)
            timeRemain -= Time.deltaTime;

        if(timeRemain <= 0)
        {
            //if (readyToLoad && PhotonNetwork.IsMasterClient)
            //{
            //    PhotonNetwork.LoadLevel(0);
            //}

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
        secondText.text = ((int)timeRemain - (int)(timeRemain / 60)*60).ToString();
    }
}
