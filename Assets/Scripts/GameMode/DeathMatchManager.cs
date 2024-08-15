using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;
using Photon.Realtime;

public class DeathMatchManager : GameModeManager
{
    PhotonView pv;

    int timeForDeathMatch = 30;

    [SerializeField] TMP_Text minuteText;
    [SerializeField] TMP_Text secondText;

    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] GameObject endGameOptionCanvas;
    [SerializeField] Scoreboard scoreboard;

    [SerializeField] GameObject startButton;
    [SerializeField] GameObject readyButton;

    bool ableToActivateEndGameOption = true;

    private void Start()
    {
        maxTime = timeForDeathMatch;
        timeRemain = maxTime;
        PhotonNetwork.AutomaticallySyncScene = false;
        InvokeRepeating(nameof(GetMinutes), 0, 1f);
        InvokeRepeating(nameof(GetSeconds), 0, 0.8f);
        pv = GetComponent<PhotonView>();
    }

    void GetMinutes()
    {
        minuteText.text = ((int)(timeRemain / 60)).ToString();
    }

    void GetSeconds()
    {
        secondText.text = Mathf.Ceil((int)timeRemain - (int)(timeRemain / 60) * 60).ToString();
    }

    private void Update()
    {
        if (timeRemain > 0)
            timeRemain -= Time.deltaTime;

        if (timeRemain <= 0)
        {
            if (ableToActivateEndGameOption)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                foreach (var item in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
                {
                    Destroy(item.gameObject);
                }

                canvasGroup.alpha = 1.0f;
                endGameOptionCanvas.SetActive(true);
                startButton.SetActive(PhotonNetwork.IsMasterClient);
                readyButton.SetActive(!PhotonNetwork.IsMasterClient);

                ableToActivateEndGameOption = false;
                isGameEnd = true;
            }
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startButton.SetActive(PhotonNetwork.IsMasterClient);
        readyButton.SetActive(!PhotonNetwork.IsMasterClient);
    }

    public void OnClickRestart()
    {
        pv.RPC(nameof(RestartGame), RpcTarget.All);
    }

    [PunRPC]
    void RestartGame()
    {
        ResetGameTimer();
        foreach (var item in FindObjectsByType<PlayerManager>(FindObjectsSortMode.None))
        {
            if (item.GetComponent<PhotonView>().IsMine)
            {
                item.Spawn();
                item.ResetStats();
                break;
            }
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        canvasGroup.alpha = 0.0f;
        startButton.SetActive(false);
        readyButton.SetActive(false);
        endGameOptionCanvas.SetActive(false);
        scoreboard.ResetScoreBoard();
        ableToActivateEndGameOption = true;
        isGameEnd = false;
    }

    public void OnClickLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel(0);
    }
}
