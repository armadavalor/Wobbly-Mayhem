using System.Collections;
using System.Globalization;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;

public class Weapon : NetworkBehaviour
{
    public enum WeaponType { Knife, Pistol, Rifle, GravityGun };
    public WeaponType weaponType;
    public PlayerController2 playerController;

    Vector3 startPosition;

    [SerializeField] LayerMask interactionLayer;
    [SerializeField] LayerMask playerHitLayer;
    [SerializeField] GameObject bulletImpactPrefab;
    [SerializeField] GameObject hitEffectPrefab;
    [SerializeField] GameObject magazineOnGun, leftHandMagazine;
    [SerializeField] AudioSource weaponSound;
    [SerializeField] AudioSource chargingSound;
    [SerializeField] Camera cam;
    [SerializeField] VisualEffect muzzleVFX;
    [SerializeField] Rigidbody headRb;

    [Header("General Specs")]
    [SerializeField] int Damage;
    public int spareBullet;
    public int magazineBullet;
    public int _bullet;
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

    bool chargingGG;
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
        startPosition = transform.localPosition;
        movementWeapon = GetComponent<WeaponMovements>();
    }

    private void Update()
    {
        if (IsOwner)
        {
            playerCanShoot = weaponManager.canSwitchWeapon.Value;

            if (playerCanShoot)
            {
                SwitchWeapon();
                weaponManager.TextBulletCount(_bullet, spareBullet);
            }

            if (_bullet != magazineBullet && !reloading && spareBullet > 0 && (Input.GetKeyDown(KeyCode.R) || _bullet <= 0))
            {
                reloading = true;
              //  leftHandMagazine.SetActive(true);
                magazineOnGun.SetActive(false);
                Invoke(nameof(Reload), 1f);
            }

            if (currentRecoilIndex > 0 && !playerShoots)
            {
                Invoke(nameof(RecoilLower), firingRate * 2);
            }
        }
    }

    void RecoilLower()
    {
        currentRecoilIndex--;
        if (currentRecoilIndex < 0) currentRecoilIndex = 0;
    }

    void SwitchWeapon()
    {
        if (!IsOwner)
            return;

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
                    if (Input.GetMouseButtonDown(0))
                        KnifeAttacks();
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
                FireServerRpc();
            }

            playerShoots = true;
            currentRecoilIndex++;
            StartCoroutine(BasicTimer());
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
        if (Physics.Raycast(cam.transform.position, spreadRay, out hit, fireDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                InstantiateServerRpc(false, hit.point + spreadBullet + randomSpread, Quaternion.LookRotation(hit.normal));
                DamageReceiver damageReceiver = hit.collider.GetComponent<DamageReceiver>();

                if (damageReceiver != null)
                {
                    Debug.Log($"Hit player: {hit.collider.name}, applying damage: {Damage}");
                    damageReceiver.ApplyDamage(Damage, attackerId: OwnerClientId,victimId:OwnerClientId );
                }
                else
                {
                    Debug.LogError("DamageReceiver component not found on the hit object.");
                }
            }
            else
            {
                InstantiateServerRpc(true, hit.point + spreadBullet + randomSpread, Quaternion.LookRotation(hit.normal));
            }
        }

    }


    [ServerRpc]
    void FireServerRpc()
    {
        if (IsServer)
        {
            FireClientRpc();
        }
    }

    [ClientRpc]
    void FireClientRpc()
    {
        if (IsOwner)
        {
            if (weaponType == WeaponType.Rifle || weaponType == WeaponType.Pistol)
            {
                muzzleVFX.Play(); weaponSound.Play();
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
    }

    [ServerRpc(RequireOwnership = false)]
    public void InstantiateServerRpc(bool isBullet, Vector3 position, Quaternion rotation)
    {
        if (isBullet)
        {
            InstantiateBulletClientRpc(position, rotation);
        }
        else
        {
            InstantiateHitClientRpc(position, rotation);
        }
    }

    [ClientRpc]
    void InstantiateHitClientRpc(Vector3 position, Quaternion rotation)
    {
        Instantiate(hitEffectPrefab, position, rotation);
    }

    [ClientRpc]
    void InstantiateBulletClientRpc(Vector3 position, Quaternion rotation)
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

            if (Physics.Raycast(cam.transform.position, forward, out hit, fireDistance))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    InstantiateServerRpc(false, hit.point, Quaternion.LookRotation(hit.normal));
                    DamageReceiver damageReceiver = hit.collider.GetComponent<DamageReceiver>();

                    if (damageReceiver != null)
                    {
                        Debug.Log($"Hit player: {hit.collider.name}, applying damage: {Damage}");
                        damageReceiver.ApplyDamage(Damage, attackerId: OwnerClientId,OwnerClientId);
                    }
                    else
                    {
                        Debug.LogError("DamageReceiver component not found on the hit object.");
                    }

                    // hit.collider.GetComponent<PlayerController2>().ApplyDamage(Damage,killerId:OwnerClientId);

                }
                else
                {
                    InstantiateServerRpc(true, hit.point, Quaternion.LookRotation(hit.normal));
                }
            }

            weaponSound.Play();
            movementWeapon.SlideMovement();
        }

        if (Input.GetMouseButton(1))
        {
            
            gravityGunForce += 2.5f * Time.deltaTime;
            gravityGunForce = Mathf.Clamp(gravityGunForce, 0f, 5f);
            LaserBeam(true);
            
            if (!chargingGG)
            {
                chargingSound.Play();
                chargingGG = true;
            }

            movementWeapon.RotatePortal();
        }
        if (Input.GetMouseButtonUp(1) && gravityGunForce >= 2f)
        {
            PlayerController2 playerCont = GetComponentInParent<PlayerController2>();

            Transform playerTransform = playerCont.gameObject.transform;
            Vector3 lookDirection = playerTransform.forward;
            Vector3 direction = lookDirection * -1f;

            if (Physics.Raycast(cam.transform.position, forward, out hit, fireDistance))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    InstantiateServerRpc(false, hit.point, Quaternion.LookRotation(hit.normal));
                    DamageReceiver damageReceiver = hit.collider.GetComponent<DamageReceiver>();

                    if (damageReceiver != null)
                    {
                        Debug.Log($"Hit player: {hit.collider.name}, applying damage: {Damage}");
                        damageReceiver.ApplyDamage(Damage, attackerId: OwnerClientId,OwnerClientId);
                    }
                    else
                    {
                        Debug.LogError("DamageReceiver component not found on the hit object.");
                    }

                    // hit.collider.GetComponent<PlayerController2>().ApplyDamage(Damage,killerId:OwnerClientId);
                }
                else
                {
                    InstantiateServerRpc(true, hit.point, Quaternion.LookRotation(hit.normal));

                    headRb.AddForce(Vector3.up * gravityGunForce * 10000f * Time.deltaTime, ForceMode.Impulse);
                    headRb.AddForce(direction * gravityGunForce * 5000f * Time.deltaTime, ForceMode.Impulse);
                }
            }

            chargingGG = false;
            chargingSound.Stop();
            weaponSound.Play();
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
        StartCoroutine(KnifeAttack());
    }

    IEnumerator KnifeAttack()
    {
        Vector3 targetPosition = new(0f, 5f, 0f);
        while (Vector3.Distance(transform.localPosition, targetPosition) > 0.01f)
        {
            // Hedef konuma doðru hareket
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, 1f * Time.deltaTime);
            yield return null;
        }
    }

    public void ResetKnifePosition()
    {
        transform.localPosition = startPosition;
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

        weaponManager.TextBulletCount(_bullet, spareBullet);
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

    private void OnTriggerEnter(Collider other)
    {
        if (weaponType == WeaponType.Knife)
        {
            if (other.CompareTag("Player") && !IsOwner)
            {
                InstantiateServerRpc(false, other.transform.position, Quaternion.identity);
                DamageReceiver damageReceiver = other.GetComponent<DamageReceiver>();

                if (damageReceiver != null)
                {
                    Debug.Log($"Hit player: {other.name}, applying damage: {Damage}");
                    damageReceiver.ApplyDamage(Damage, attackerId: OwnerClientId);
                }
                else
                {
                    Debug.LogError("DamageReceiver component not found on the hit object.");
                }

                gameObject.SetActive(false);
                // hit.collider.GetComponent<PlayerController2>().ApplyDamage(Damage,killerId:OwnerClientId);
            }
            else
            {
                InstantiateServerRpc(true, other.transform.position, Quaternion.identity);
                gameObject.SetActive(false);
            }
        }
    }
}
