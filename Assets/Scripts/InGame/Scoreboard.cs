using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scoreboard : MonoBehaviourPunCallbacks
{
    [SerializeField] Transform container;
    [SerializeField] GameObject scoreboardItemPrefab;
    [SerializeField] CanvasGroup canvasGroup;
    GameModeManager gameModeManager;

    private void Start()
    {
        gameModeManager = FindAnyObjectByType<GameModeManager>();
        foreach (Player player in PhotonNetwork.PlayerList) 
        {
            AddScoreBoardItem(player);
        }   
    }

    private void AddScoreBoardItem(Player player)
    {
        ScoreboardItem item = Instantiate(scoreboardItemPrefab, container).GetComponent<ScoreboardItem>();
        item.Initialize(player);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        foreach (var child in container.GetComponentsInChildren<ScoreboardItem>())
        {
            if (child.player == otherPlayer)
            {
                Destroy(child.gameObject);
                break;
            }
        }
    }

    private void Update()
    {
        if (gameModeManager.timeRemain > 0)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                canvasGroup.alpha = 1;
            }
            else if (Input.GetKeyUp(KeyCode.Tab))
            {
                canvasGroup.alpha = 0;
            }
        }
    }

    public void ResetScoreBoard()
    {
        foreach (var child in container.GetComponentsInChildren<ScoreboardItem>())
        {
            Destroy(child.gameObject);
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            AddScoreBoardItem(player);
        }
    }
}
