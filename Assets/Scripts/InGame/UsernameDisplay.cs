using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UsernameDisplay : MonoBehaviour
{
    [SerializeField] PhotonView playerPV;
    [SerializeField] TMP_Text username;

    void Start()
    {
        if(playerPV.IsMine)
            gameObject.SetActive(false);

        username.text = playerPV.Owner.NickName; //Nickname will be sync across network automatically
    }
}
