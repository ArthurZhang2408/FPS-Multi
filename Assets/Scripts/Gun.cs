using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Gun : Item
{
    public GameObject bulletImpactPrefab;
    public int ammoInStock;
    public int currentLoadedAmmo;
    public float coolingof;
    public float reloadCD;
    public AudioSource source;
    private GunInfo gunInfo;
    [SerializeField] Camera cam;
    PhotonView PV;
    private float reloadRotation;
    private int recoilRotateSmooth;


    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        gunInfo = (GunInfo)itemInfo;
        ammoInStock = gunInfo.maxAmmo;
        currentLoadedAmmo = 0;
        recoilRotateSmooth = 0;
        reloadRotation = 0f;
    }

    public override bool Use(int id)
    {
        if (recoilRotateSmooth > 0)
        {
            recoilRotateSmooth--;
            PlayerController.verticalLookRotation += gunInfo.recoilRotation / 10;
        }
        if (preCheck())
        {
            if (gunInfo.auto)
            {
                if (Input.GetMouseButton(0))
                {
                    return Shoot(id, cam, PV);
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                return Shoot(id, cam, PV);
            }
        }
        return false;
    }

    public bool Shoot(int id, Camera cam, PhotonView PV)
    {
        recoilRotateSmooth = 10;
        source.PlayOneShot(gunInfo.fire);
        currentLoadedAmmo--;
        coolingof = gunInfo.threshold;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);
            IDamageable i = hit.collider.gameObject.GetComponent<IDamageable>();
            if (i != null)
            {
                i.TakeDamage(gunInfo.damage, id);
                return true;
            }
        }
        return false;
    }

    public bool preCheck()
    {
        if (ammoInStock <= 0 && currentLoadedAmmo <= 0) return false;
        if (coolingof > 0)
        {
            coolingof -= Time.deltaTime;
            return false;
        }
        if (ammoInStock > 0 && (currentLoadedAmmo == 0 || Input.GetKeyDown(KeyCode.R)) && reloadCD <= 0)
        {
            source.PlayOneShot(gunInfo.reload);
            reloadCD = gunInfo.reloadThreshold;
            return false;
        }
        if (reloadCD > 0)
        {
            reloadCD -= Time.deltaTime;
            if (reloadCD >= gunInfo.reloadThreshold / 2)
            {
                reloadRotation += 1f;
            }else
            {
                reloadRotation -= 1f;
            }
            itemGameObject.transform.localEulerAngles = Vector3.down * reloadRotation;
            //trans.localEulerAngles = Vector3.left * Mathf.Lerp(trans.rotation.y, -30, gunInfo.reloadThreshold * Time.deltaTime / 2);
            if (reloadCD <= 0)
            {
                itemGameObject.transform.localEulerAngles = Vector3.zero;
                if (ammoInStock > gunInfo.maxLoadedAmmo - currentLoadedAmmo)
                {
                    ammoInStock -= gunInfo.maxLoadedAmmo - currentLoadedAmmo;
                    currentLoadedAmmo = gunInfo.maxLoadedAmmo;
                }
                else if (ammoInStock > 0)
                {
                    currentLoadedAmmo += ammoInStock;
                    ammoInStock = 0;
                }
            }
            return false;
        }
        return true;
    }

    [PunRPC]
    void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        source.PlayOneShot(gunInfo.fire);
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        if (colliders.Length != 0)
        {
            GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * bulletImpactPrefab.transform.rotation);
            Destroy(bulletImpactObj, 10f);
            bulletImpactObj.transform.SetParent(colliders[0].transform);
        }
    }
}