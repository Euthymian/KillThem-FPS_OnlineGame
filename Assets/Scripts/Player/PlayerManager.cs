using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using Photon.Realtime;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerManager : MonoBehaviour
{
    PhotonView pv;
    GameObject controller;
    GameModeManager gameModeManager;

    int kills;
    int deaths;

    float respawnTime = 2f;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    void Start()
    {
        gameModeManager = FindAnyObjectByType<GameModeManager>();
        if (pv.IsMine)
        {
            Spawn();
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Spawn()
    {
        Transform spawnPoint = SpawnManager.Instance.GetSpawnPoint();
        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), spawnPoint.position, spawnPoint.rotation, 0, new object[] { pv.ViewID });
    }

    public void Die()
    {
        PhotonNetwork.Destroy(controller);
 
        StartCoroutine(Respawn());

        deaths++;

        Hashtable hashtable = new Hashtable();
        hashtable.Add("Deaths", deaths);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
    }

    public void GetKill()
    {
        pv.RPC(nameof(RPC_GetKill), pv.Owner);
    }

    [PunRPC]
    void RPC_GetKill()
    {
        kills++;

        Hashtable hashtable = new Hashtable();
        hashtable.Add("Kills", kills);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
    }

    public static PlayerManager Find(Player player)
    {
        return FindObjectsOfType<PlayerManager>().SingleOrDefault(x => x.pv.Owner == player);
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);
        if (gameModeManager.timeRemain > 0)
            Spawn();
    }
}
