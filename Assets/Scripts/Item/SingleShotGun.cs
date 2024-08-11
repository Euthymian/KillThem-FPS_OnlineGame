using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;

public class SingleShotGun : Gun
{
    [SerializeField] Camera cam;
    PhotonView pv;
    

    [SerializeField] ParticleSystem muzzleFlask;
    bool readyToShoot = true;
    UnityEvent ReloadEvent = new UnityEvent();

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    private void Start()
    {
        gunInfo = (GunInfo)itemInfo;
        currentAmmos = gunInfo.ammoCapacity;
        ReloadEvent.AddListener(StartReload);
    }

    public override void Use()
    {
        if (currentAmmos > 0 && !isReloading)
            Shoot();
    }

    IEnumerator ReloadProcedure()
    {
        isReloading = true;
        yield return new WaitForSeconds(gunInfo.reloadTime);
        currentAmmos = gunInfo.ammoCapacity;
        isReloading = false;
        GetComponentInParent<PlayerController>().UpdateAmmoUI();
    }

    void StartReload()
    {
        if (currentAmmos == gunInfo.ammoCapacity)
            return;
        StartCoroutine(ReloadProcedure());
    }

    public override void Reload()
    {
        ReloadEvent.Invoke();
    }

    private void Shoot()
    {
        if (readyToShoot)
        {
            readyToShoot = false;

            muzzleFlask.Play();

            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            ray.origin = cam.transform.position + cam.transform.forward * 1;

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                //Debug.Log("Hit "+ hit.collider.gameObject.name);
                if (hit.collider.gameObject.TryGetComponent<IDamageable>(out IDamageable component))
                {
                    float realDamage = gunInfo.damage;
                    if (!gunInfo.alwaysOriginalDamage)
                    {
                        float distance = Vector3.Distance(gameObject.transform.position, hit.collider.gameObject.transform.position);
                        if (distance > gunInfo.noDamageRange) realDamage = 0;
                        else if (distance > gunInfo.originalDamageRange)
                        {
                            realDamage = realDamage * (1 - distance / gunInfo.noDamageRange);
                        }
                    }

                    if(hit.collider.GetType() == typeof(SphereCollider))
                    {
                        //print("head shot");
                        realDamage *= 2;
                    }

                    component.TakeDamage(realDamage);
                }

                pv.RPC(nameof(RPC_Shoot), RpcTarget.All, hit.point, hit.normal);
            }

            currentAmmos--;
            if (currentAmmos == 0)
            {
                Reload();
            }

            StartCoroutine(DelayBetweenFires());
        }
    }

    IEnumerator DelayBetweenFires()
    {
        yield return new WaitForSeconds(((GunInfo)itemInfo).fireRate);
        readyToShoot = true;
    }

    [PunRPC]
    void RPC_Shoot(Vector3 hitPos, Vector3 hitNormal)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPos, 0.3f);
        if (colliders.Length != 0)
        {
            if (colliders[0].gameObject.TryGetComponent<BulletImpact>(out BulletImpact component))
            {
                GameObject bulletImpactPrefab = component.bulletImpactPrefab;
                GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hitPos + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * bulletImpactPrefab.transform.rotation);
                Destroy(bulletImpactObj, component.destroyTime);
                bulletImpactObj.transform.SetParent(colliders[0].transform);
            }
        }
    }
}
