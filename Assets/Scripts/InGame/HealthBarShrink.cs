using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarShrink : MonoBehaviour
{
    [SerializeField] Image healthBarImage, damagedBarImage;
    float timerForStartShrink = 0.75f;
    float shrinkSpeed = 0.9f;
    bool startShrink;

    void Update()
    {
        if(startShrink)
            Shrink();
    }

    public void ResetTimer()
    {
        timerForStartShrink = 1;
    }

    public void Shrink()
    {
        timerForStartShrink -= Time.deltaTime;
        if(timerForStartShrink <= 0)
        {
            if (damagedBarImage.fillAmount > healthBarImage.fillAmount)
            {
                damagedBarImage.fillAmount -= shrinkSpeed * Time.deltaTime;
            }
            else startShrink = false;
        }
    }

    public void StartShrink()
    {
        startShrink = true;
        ResetTimer();
    }
}
