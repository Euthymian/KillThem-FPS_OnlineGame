using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Kill Them/New Gun")]
public class GunInfo : ItemInfo
{
    public float damage;
    public bool alwaysOriginalDamage;
    public float originalDamageRange;
    public float noDamageRange;
    public float fireRate;
    public int bulletPerShoot;
    public int ammoCapacity;
    public Vector2 spreadRange;
    public float reloadTime;
    public bool hasScope;
}
