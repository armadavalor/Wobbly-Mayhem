using System.Collections;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;

public class Weapon : NetworkBehaviour
{
    // Silah t�rlerini belirten bir enum
    public enum WeaponType { Knife, Pistol, Rifle, GravityGun };
    public WeaponType weaponType;

    Quaternion startRot;

    [SerializeField] LayerMask interactionLayer; // Etkile�im katman�
    [SerializeField] GameObject bulletImpact; // Kur�un �arpt���nda kullan�lacak efekt
    [SerializeField] GameObject hitEffect; // Kur�un �arpt���nda kullan�lacak efekt
    [SerializeField] GameObject magazineOnGun, leftHandMagazine;
    [SerializeField] Camera cam; // Silah�n kullanaca�� kamera
    [SerializeField] VisualEffect muzzleVFX;
    [SerializeField] Rigidbody headRb;

    [Header("General Specs")]
    [SerializeField] int Damage; // Silah�n verdi�i hasar
    [SerializeField] int spareBullet; // Yedek mermi say�s�
    [SerializeField] int magazineBullet; // Silah�n �arj�r� doluyken alabilece�i maksimum mermi say�s�
    [SerializeField] int _bullet; // Anl�k mermi say�s�
    [SerializeField] int currentRecoilIndex;
    [SerializeField] bool reloading; // Yeniden y�kleme yap�l�yor mu?
    [SerializeField] bool playerShoots; // Oyuncu �u anda ate� ediyor mu?
    [SerializeField] bool playerCanShoot = true; // Oyuncu ate� edebilir mi?
    [SerializeField] float firingRate; // Ate� etme h�z�
    [SerializeField] float fireDistance; // Ate� etme mesafesi


    [Header("Gravity Gun")]
    [SerializeField] float gravityGunForce; // Gravity Gun kuvveti
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

    private void Start()
    {
        // Maksimum mermi say�s�n� anl�k mermi say�s�yla e�itle
        magazineBullet = _bullet;
        movementWeapon = GetComponent<WeaponMovements>();
        startRot = transform.localRotation;
    }

    private void Update()
    {
        playerCanShoot = weaponManager.canSwitchWeapon;

        // Oyuncu ate� edebilir mi kontrol et ve ate�le
        if (IsOwner && playerCanShoot)
        {
            SwitchWeapon();
            weaponManager.TextBullentCount(_bullet, spareBullet);
        }

        // R tu�una bas�ld���nda ve mermiler bitti�inde yeniden y�kle
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
                // Silah�n rotasyonunu yay�lma a��s�na g�re ayarla
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
        // Ate� etmiyorsa ve yeniden y�klenmiyorsa ate�le
        if (!playerShoots && !reloading)
        {
            switch (weaponType)
            {
                case WeaponType.GravityGun:
                    // Gravity Gun: Sa� fare tu�una basarak kuvveti art�r, b�rak�ld���nda ate�le
                    FireServerRpc();
                    break;

                case WeaponType.Rifle:
                    // Makinal� T�fek: S�rekli ate�le
                    Fire();
                    // if (Input.GetMouseButtonDown(1)) GetComponent<RifleScope>().Scope();
                    break;

                case WeaponType.Pistol:
                    // Tabanca: Sol fare tu�una bas�ld���nda ate�le
                    if (Input.GetMouseButtonDown(0)) Fire();
                    break;

                case WeaponType.Knife:
                    // B��ak
                    FireServerRpc();
                    break;
            }
        }
    }

    void Fire()
    {
        // Ate� ediliyor mu ve yeniden y�kleniyor mu kontrol et
        if (Input.GetMouseButton(0) && !playerShoots && !reloading)
        {
            if (_bullet > 0)
            {
                // Mermiyi azalt
                _bullet--;
                weaponManager.TextBullentCount(_bullet, spareBullet);
                muzzleVFX.Play();

                FireServerRpc();
            }

            // Ate� etmeyi ba�lat ve bir s�re sonra durdur
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
        if (IsOwner)
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
    }

    void ShootRay()
    {
        Vector3 forward = cam.transform.forward;
        Vector3 randomSpread = new(Random.Range(-0.03f, 0.03f), 0f);
        Vector3 spreadBullet = cam.transform.TransformDirection(weaponManager.recoilVectorList[currentRecoilIndex]);

        Vector3 spreadRay = forward + spreadBullet;
        if (currentRecoilIndex != 0) spreadRay.x += randomSpread.x;

        // Bir objeye isabet etti mi?
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, spreadRay, out hit, fireDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Instantiate(hitEffect, hit.point + spreadBullet + randomSpread, Quaternion.LookRotation(hit.normal));
            }
            else
            {
                Instantiate(bulletImpact, hit.point + spreadBullet + randomSpread, Quaternion.LookRotation(hit.normal));
            }
        }
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
                    Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                }
                else
                {
                    Instantiate(bulletImpact, hit.point, Quaternion.LookRotation(hit.normal));
                }
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

            // Oyuncunun bak�� y�n�n� al
            Transform playerTransform = playerCont.gameObject.transform;
            Vector3 lookDirection = playerTransform.forward;

            // Kuvvet y�n�n� belirle ve tersine �evir
            Vector3 direction = lookDirection * -1f;


            if (Physics.Raycast(cam.transform.position, forward, out hit, fireDistance))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                }
                else
                {
                    Instantiate(bulletImpact, hit.point, Quaternion.LookRotation(hit.normal));

                    headRb.AddForce(Vector3.up * gravityGunForce * 10000f * Time.deltaTime, ForceMode.Impulse);
                    headRb.AddForce(direction * gravityGunForce * 5000f * Time.deltaTime, ForceMode.Impulse);
                }
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
            Debug.Log("cal�st�");
        }
    }

    public void Reload()
    {
        // Eksik mermileri hesapla ve yeniden y�kle
        int missingBullets = magazineBullet - _bullet;
        int bulletsToLoad = Mathf.Min(spareBullet, missingBullets);

        _bullet += bulletsToLoad;
        spareBullet -= bulletsToLoad;

        // Mermi say�lar�n� s�n�rla
        _bullet = Mathf.Clamp(_bullet, 0, magazineBullet);
        spareBullet = Mathf.Clamp(spareBullet, 0, 100);

        reloading = false;
        playerCanShoot = true;
        weaponManager.TextBullentCount(_bullet, spareBullet);

        leftHandMagazine.SetActive(false);
        magazineOnGun.SetActive(true);
    }

    IEnumerator BasicTimer()
    {
        // Belirtilen s�re boyunca bekle ve ilgili de�i�keni s�f�rla
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
