using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gun : Item
{
    public abstract override void Use();
    public abstract void Reload();

    protected bool isReloading;
    [HideInInspector] public bool WasReloading;
    [HideInInspector] public float currentReloadTime;
    public bool IsReloading { get => isReloading; }

    protected GunInfo gunInfo;

    public GunInfo GunInfor { get => gunInfo; }

    protected int currentAmmos;
    public int CurrentAmmos { get => currentAmmos; }
}
