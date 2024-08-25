using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerPhotonSoundManager : MonoBehaviour
{
    public AudioSource footstepSource;
    public AudioClip footstepSFX;

    public AudioSource gunShootSource;
    public AudioClip[] gunShootSFXs;

    public void PlayFootstepSFX()
    {
        GetComponent<PhotonView>().RPC(nameof(RPC_PlayFootstepSFX), RpcTarget.All);
    }

    [PunRPC]
    public void RPC_PlayFootstepSFX()
    {
        footstepSource.clip = footstepSFX;

        // Pitch and volume
        footstepSource.pitch = Random.Range(0.7f, 1.2f);
        footstepSource.volume = Random.Range(0.2f, 0.35f);

        footstepSource.Play();
    }

    public void PlayShootSFX(int gunIndex)
    {
        GetComponent<PhotonView>().RPC(nameof(RPC_PlayShootSFX), RpcTarget.All, gunIndex);
    }

    [PunRPC]
    public void RPC_PlayShootSFX(int gunIndex)
    {
        gunShootSource.clip = gunShootSFXs[gunIndex];
        gunShootSource.Play();
    }
}
