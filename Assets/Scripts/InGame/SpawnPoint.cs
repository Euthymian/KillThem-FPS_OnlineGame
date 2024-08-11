using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] GameObject graphic;

    private void Awake()
    {
        graphic.SetActive(false);
    }
}
