using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine.UI;

public class ScoreboardItem : MonoBehaviourPunCallbacks
{
    public TMP_Text usernameText, killsText, deathsText;
    public GameObject leaveButton;
    public GameObject masterClientIndicator;
    public GameObject readyIndicator;
    public Player player;

    GameModeManager gameModeManager;

    public void Initialize(Player player)
    {
        usernameText.text = player.NickName;
        this.player = player;
    }

    private void Start()
    {
        gameModeManager = FindAnyObjectByType<GameModeManager>();
        gameModeManager.OnGameEndEvent.AddListener(OnMasterClientIndicator);
        gameModeManager.OffGameEndEvent.AddListener(OffIndicators);
    }

    void OnMasterClientIndicator()
    {
        if (player == PhotonNetwork.MasterClient)
        {
            masterClientIndicator.SetActive(true);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (player == newMasterClient)
        {
            masterClientIndicator.SetActive(true);
            readyIndicator.SetActive(false);
        }
    }

    void OffIndicators()
    {
        masterClientIndicator.SetActive(false);
        readyIndicator.SetActive(false);
    }

    void ChangeReadyIndicator(bool input)
    {
        readyIndicator.SetActive(input);
    }

    void UpdateStats()
    {
        if (player.CustomProperties.TryGetValue(Utilities.killKey, out object kills))
        {
            killsText.text = kills.ToString();
        }

        if (player.CustomProperties.TryGetValue(Utilities.deathKey, out object deaths))
        {
            deathsText.text = deaths.ToString();
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer == player)
        {
            if (changedProps.ContainsKey(Utilities.killKey) || changedProps.ContainsKey(Utilities.deathKey))
                UpdateStats();

            if (changedProps.ContainsKey(Utilities.readyKey))
                ChangeReadyIndicator((bool)targetPlayer.CustomProperties[Utilities.readyKey]);
        }
    }
}
