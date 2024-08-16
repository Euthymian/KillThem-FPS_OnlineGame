using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;

public class DeathMatchManager : GameModeManager
{
    PhotonView pv;

    int timeForDeathMatch = 30;

    [SerializeField] TMP_Text minuteText;
    [SerializeField] TMP_Text secondText;

    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] GameObject endGameOptionCanvas;
    [SerializeField] Scoreboard scoreboard;

    [SerializeField] GameObject leaveButton;
    [SerializeField] GameObject startButton;
    [SerializeField] GameObject readyButton;
    [SerializeField] GameObject unreadyButton;

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
                OnGameEndEvent.Invoke();
            }

            int maxNumberOfReadyPlayers = PhotonNetwork.PlayerList.Length - 1;
            int currentNumberOfReadyPlayers = 0;
            foreach (var item in PhotonNetwork.PlayerList)
            {
                if (item.CustomProperties.ContainsKey(Utilities.readyKey)) 
                    if((bool)item.CustomProperties[Utilities.readyKey])
                        currentNumberOfReadyPlayers++;
            }

            if (currentNumberOfReadyPlayers == maxNumberOfReadyPlayers)
            {
                EnableButton(startButton);
            }
            else
            {
                DisableButton(startButton);
            }
        }
    }

    public void ChangeLeaveButtonStatus(bool input)
    {
        if (input)
        {
            DisableButton(leaveButton);
        }
        else
        {
            EnableButton(leaveButton);
        }
    }

    void DisableButton(GameObject button)
    {
        button.GetComponent<Image>().color = new Color(button.GetComponent<Image>().color.r, button.GetComponent<Image>().color.g, button.GetComponent<Image>().color.b, 0.5f);
        button.GetComponent<Button>().enabled = false;
    }

    void EnableButton(GameObject button)
    {
        button.GetComponent<Image>().color = new Color(button.GetComponent<Image>().color.r, button.GetComponent<Image>().color.g, button.GetComponent<Image>().color.b, 1);
        button.GetComponent<Button>().enabled = true;
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if(targetPlayer == PhotonNetwork.LocalPlayer)
        {
            if (changedProps.ContainsKey(Utilities.readyKey))
                ChangeLeaveButtonStatus((bool)targetPlayer.CustomProperties[Utilities.readyKey]);
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
        unreadyButton.SetActive(false);
        EnableButton(leaveButton);
        endGameOptionCanvas.SetActive(false);
        scoreboard.ResetScoreBoard();
        ableToActivateEndGameOption = true;
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            player.CustomProperties.Remove(Utilities.readyKey);
        }
    }

    public void OnClickLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel(0);
    }

    public void OnClickReady()
    {
        Hashtable hashtable = new Hashtable();
        hashtable.Add(Utilities.readyKey, true);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
        readyButton.SetActive(false);
        unreadyButton.SetActive(true);
    }

    public void OnClickUnready()
    {
        Hashtable hashtable = new Hashtable();
        hashtable.Add(Utilities.readyKey, false);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
        readyButton.SetActive(true);
        unreadyButton.SetActive(false);
    }
}
