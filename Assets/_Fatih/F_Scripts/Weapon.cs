using System.Collections;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;

public class Weapon : NetworkBehaviour
{
    public enum WeaponType { Knife, Pistol, Rifle, GravityGun };
    public WeaponType weaponType;

    Quaternion startRot;

    [SerializeField] LayerMask interactionLayer;
    [SerializeField] LayerMask playerHitLayer;
    [SerializeField] GameObject bulletImpactPrefab;
    [SerializeField] GameObject hitEffectPrefab;
    [SerializeField] GameObject magazineOnGun, leftHandMagazine;
    [SerializeField] Camera cam;
    [SerializeField] VisualEffect muzzleVFX;
    [SerializeField] Rigidbody headRb;

    [Header("General Specs")]
    [SerializeField] int Damage;
    [SerializeField] int spareBullet;
    [SerializeField] int magazineBullet;
    [SerializeField] int _bullet;
    [SerializeField] int currentRecoilIndex;
    [SerializeField] bool reloading;
    [SerializeField] bool playerShoots;
    [SerializeField] bool playerCanShoot = true;
    [SerializeField] float firingRate;
    [SerializeField] float fireDistance;

    [Header("Gravity Gun")]
    [SerializeField] float gravityGunForce;
    [SerializeField] GameObject[] laserBeam;

    public WeaponManager weaponManager;
    public WeaponMovements movementWeapon;

    private void OnEnable()
    {
        if (_bullet <= 0 && spareBullet > 0)
        {
            reloading = true;
            Invoke(nameof(Reload), 1f);
        }
    }

    private void Awake()
    {
        magazineBullet = _bullet;
        movementWeapon = GetComponent<WeaponMovements>();
        startRot = transform.localRotation;
    }

    private void Update()
    {
        if (IsOwner && playerCanShoot)
        {
            SwitchWeapon();
        }

        if (_bullet != magazineBullet && !reloading && spareBullet > 0 && (Input.GetKeyDown(KeyCode.R) || _bullet <= 0))
        {
            reloading = true;
            leftHandMagazine.SetActive(true);
            magazineOnGun.SetActive(false);
            Invoke(nameof(Reload), 1f);
        }

        if (currentRecoilIndex > 0 && !playerShoots)
        {
            Invoke(nameof(RecoilLower), firingRate * 2);
        }
    }

    private void LateUpdate()
    {
        if (IsOwner && Input.GetKey(KeyCode.Mouse0) && playerShoots && !reloading)
        {
            if (_bullet > 0)
            {
                Vector3 quaRot = new(0f, weaponManager.recoilVectorList[currentRecoilIndex].x * 1f,
                    weaponManager.recoilVectorList[currentRecoilIndex].y * -1f);
                Quaternion spreadRotation = Quaternion.Euler(quaRot);

                transform.localRotation = Quaternion.Slerp(transform.localRotation, transform.localRotation * spreadRotation, firingRate * 2);
            }
        }
        else
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, startRot, firingRate);
        }
    }

    void RecoilLower()
    {
        currentRecoilIndex--;
        if (currentRecoilIndex < 0) currentRecoilIndex = 0;
    }

    void SwitchWeapon()
    {
        if (!playerShoots && !reloading)
        {
            switch (weaponType)
            {
                case WeaponType.GravityGun:
                    FireServerRpc();
                    break;

                case WeaponType.Rifle:
                    Fire();
                    break;

                case WeaponType.Pistol:
                    if (Input.GetMouseButtonDown(0)) Fire();
                    break;

                case WeaponType.Knife:
                    FireServerRpc();
                    break;
            }
        }
    }

    void Fire()
    {
        if (Input.GetMouseButton(0) && !playerShoots && !reloading)
        {
            if (_bullet > 0)
            {
                _bullet--;
                muzzleVFX.Play();

                FireServerRpc();
            }

            playerShoots = true;
            currentRecoilIndex++;
            StartCoroutine(BasicTimer());
        }
    }

    [ServerRpc]
    void FireServerRpc()
    {
        FireClientRpc();
    }

    [ClientRpc]
    void FireClientRpc()
    {
        if (weaponType == WeaponType.Rifle || weaponType == WeaponType.Pistol)
        {
            muzzleVFX.Play();
            ShootRay();
            movementWeapon.SlideMovement();
        }
        if (weaponType == WeaponType.GravityGun)
        {
            GravityGun();
        }
        if (weaponType == WeaponType.Knife)
        {
            KnifeAttacks();
        }
    }

    void ShootRay()
    {
        Vector3 forward = cam.transform.forward;
        Vector3 randomSpread = new(Random.Range(-0.03f, 0.03f), 0f);
        Vector3 spreadBullet = cam.transform.TransformDirection(weaponManager.recoilVectorList[currentRecoilIndex]);

        Vector3 spreadRay = forward + spreadBullet;
        if (currentRecoilIndex != 0) spreadRay.x += randomSpread.x;

        RaycastHit hit;

        if (Physics.Raycast(cam.transform.position, spreadRay, out hit, fireDistance, interactionLayer))
        { 
            InstantiateBulletImpactServerRpc(hit.point + spreadBullet + randomSpread, Quaternion.LookRotation(hit.normal)); 
        }

        if (Physics.Raycast(cam.transform.position, spreadRay, out hit, fireDistance, playerHitLayer))
        {
            InstantiateHitEffectServerRpc(hit.point + spreadBullet + randomSpread, Quaternion.LookRotation(hit.normal));
            hit.collider.GetComponent<PlayerController2>().ApplyDamage(Damage);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void InstantiateHitEffectServerRpc(Vector3 position, Quaternion rotation)
    {
        InstantiateHitEffectClientRpc(position, rotation);
    }

    [ClientRpc]
    void InstantiateHitEffectClientRpc(Vector3 position, Quaternion rotation)
    {
        Instantiate(hitEffectPrefab, position, rotation);
    }

    [ServerRpc(RequireOwnership = false)]
    void InstantiateBulletImpactServerRpc(Vector3 position, Quaternion rotation)
    {
        InstantiateBulletImpactClientRpc(position, rotation);
    }

    [ClientRpc]
    void InstantiateBulletImpactClientRpc(Vector3 position, Quaternion rotation)
    {
        Instantiate(bulletImpactPrefab, position, rotation);
    }

    void GravityGun()
    {
        Vector3 forward = cam.transform.forward;
        RaycastHit hit;

        if (Input.GetMouseButtonDown(0) && !playerShoots)
        {
            playerShoots = true;
            StartCoroutine(BasicTimer());

            if (Physics.Raycast(cam.transform.position, forward, out hit, fireDistance, interactionLayer))
            {
                InstantiateBulletImpactServerRpc(hit.point, Quaternion.LookRotation(hit.normal));
            }

            if (Physics.Raycast(cam.transform.position, forward, out hit, fireDistance, playerHitLayer))
            {
                InstantiateHitEffectServerRpc(hit.point, Quaternion.LookRotation(hit.normal));
                hit.collider.GetComponent<PlayerController2>().ApplyDamage(Damage);
            }

            movementWeapon.SlideMovement();
        }

        if (Input.GetMouseButton(1))
        {
            gravityGunForce += 2.5f * Time.deltaTime;
            gravityGunForce = Mathf.Clamp(gravityGunForce, 0f, 5f);
            LaserBeam(true);
            movementWeapon.RotatePortal();
        }
        if (Input.GetMouseButtonUp(1) && gravityGunForce >= 2f)
        {
            PlayerController2 playerCont = GetComponentInParent<PlayerController2>();

            Transform playerTransform = playerCont.gameObject.transform;
            Vector3 lookDirection = playerTransform.forward;
            Vector3 direction = lookDirection * -1f;

            if (Physics.Raycast(cam.transform.position, forward, out hit, fireDistance, interactionLayer))
            {
                InstantiateBulletImpactServerRpc(hit.point, Quaternion.LookRotation(hit.normal));

                headRb.AddForce(Vector3.up * gravityGunForce * 10000f * Time.deltaTime, ForceMode.Impulse);
                headRb.AddForce(direction * gravityGunForce * 5000f * Time.deltaTime, ForceMode.Impulse);
            }
            if (Physics.Raycast(cam.transform.position, forward, out hit, fireDistance, playerHitLayer))
            {
                InstantiateHitEffectServerRpc(hit.point, Quaternion.LookRotation(hit.normal));
                hit.collider.GetComponent<PlayerController2>().ApplyDamage(Damage);
            }

            LaserBeam(false);
            gravityGunForce = 0f;
        }
    }

    void LaserBeam(bool isActive)
    {
        if (isActive)
        {
            laserBeam[0].SetActive(true);
            Vector3 maxScale = new Vector3(0.075f, 0.075f, 0.075f);
            Vector3 minScale = new Vector3(0.01f, 0.01f, 0.01f);

            foreach (var efx in laserBeam)
            {
                Vector3 currentScale = efx.transform.localScale;

                currentScale += new Vector3(0.03f, 0.03f, 0.03f) * Time.deltaTime;

                currentScale.x = Mathf.Clamp(currentScale.x, minScale.x, maxScale.x);
                currentScale.y = Mathf.Clamp(currentScale.y, minScale.y, maxScale.y);
                currentScale.z = Mathf.Clamp(currentScale.z, minScale.z, maxScale.z);

                efx.transform.localScale = currentScale;
            }
        }
        else
        {
            laserBeam[0].SetActive(false);
            foreach (var efx in laserBeam) { efx.transform.localScale = new(0.01f, 0.01f, 0.01f); }
        }
    }

    void KnifeAttacks()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Rigidbody knifeRb = gameObject.AddComponent<Rigidbody>();

            knifeRb.AddForce(Vector3.forward * 10f * Time.deltaTime, ForceMode.Impulse);
            Debug.Log("Knife attack executed");
        }
    }

    public void Reload()
    {
        int missingBullets = magazineBullet - _bullet;
        int bulletsToLoad = Mathf.Min(spareBullet, missingBullets);

        _bullet += bulletsToLoad;
        spareBullet -= bulletsToLoad;

        _bullet = Mathf.Clamp(_bullet, 0, magazineBullet);
        spareBullet = Mathf.Clamp(spareBullet, 0, 100);

        reloading = false;
        playerCanShoot = true;

        leftHandMagazine.SetActive(false);
        magazineOnGun.SetActive(true);
    }

    IEnumerator BasicTimer()
    {
        yield return new WaitForSeconds(firingRate);
        playerShoots = false;
    }

    private void OnDisable()
    {
        reloading = false;
        playerShoots = false;
        playerCanShoot = true;
    }
}
