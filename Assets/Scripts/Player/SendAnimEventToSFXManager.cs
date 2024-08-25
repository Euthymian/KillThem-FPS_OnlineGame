using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendAnimEventToSFXManager : MonoBehaviour
{
    public PlayerPhotonSoundManager playerPhotonSoundManager;

    public void TriggerFootstepSFX()
    {
        playerPhotonSoundManager.PlayFootstepSFX();
    }
}
