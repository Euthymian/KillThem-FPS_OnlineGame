using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon.StructWrapping;

public class ScoreboardItem : MonoBehaviourPunCallbacks
{
    public TMP_Text usernameText, killsText, deathsText;
    Player player;

    public void Initialize(Player player)
    {
        usernameText.text = player.NickName;
        this.player = player;
    }

    void UpdateStats()
    {
        if(player.CustomProperties.TryGetValue("Kills", out object kills))
        {
            killsText.text = kills.ToString();
        }

        if (player.CustomProperties.TryGetValue("Deaths", out object deaths))
        {
            deathsText.text = deaths.ToString();
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer == player)
        {
            if (changedProps.ContainsKey("Kills") || changedProps.ContainsKey("Deaths"))
            {
                UpdateStats();
            }
        }
    }
}
