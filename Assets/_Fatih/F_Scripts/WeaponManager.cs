using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    [SerializeField] private List<GameObject> weapons;
    public List<Vector3> recoilVectorList;

    public NetworkVariable<bool> haveRifle = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> haveGravityGun = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> canSwitchWeapon = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> weaponIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Dictionary<KeyCode, int> keyToWeapon;

    [SerializeField] private TextMeshProUGUI bulletText;

    private void Start()
    {
        keyToWeapon = new Dictionary<KeyCode, int>
        {
            { KeyCode.Alpha1, 2 }, // Rifle
            { KeyCode.Alpha2, 1 }, // Pistol
            { KeyCode.Alpha3, 0 }, // Knife
            { KeyCode.Alpha4, 3 }  // Gravity Gun
        };

        weaponIndex.OnValueChanged += OnWeaponIndexChanged;

        if (IsServer || IsOwnedByServer)
        {
            InitializeWeapons();
        }
        else
        {
            UpdateWeaponOnClient();
        }
    }

    private void InitializeWeapons()
    {
        // Deactivate all weapons initially
        DeactivateAllWeapons();

        if (haveRifle.Value)
        {
            weapons[2].SetActive(true);
            weaponIndex.Value = 2;
        }
        else if (haveGravityGun.Value)
        {
            weapons[3].SetActive(true);
            weaponIndex.Value = 3;
        }
        else
        {
            weapons[1].SetActive(true);
            weaponIndex.Value = 1;
        }
    }

    private void Update()
    {
        if (canSwitchWeapon.Value && IsOwner)
        {
            SwitchWeapon();
        }
    }

    private void SwitchWeapon()
    {
        foreach (var key in keyToWeapon.Keys)
        {
            if (Input.GetKeyDown(key) && weaponIndex.Value != keyToWeapon[key])
            {
                int newWeaponIndex = keyToWeapon[key];

                if (newWeaponIndex == 2 && !haveRifle.Value) continue;
                if (newWeaponIndex == 3 && !haveGravityGun.Value) continue;

                RequestWeaponChangeServerRpc(newWeaponIndex);
                break;
            }
        }
    }

    [ServerRpc]
    private void RequestWeaponChangeServerRpc(int newWeaponIndex, ServerRpcParams rpcParams = default)
    {
        weaponIndex.Value = newWeaponIndex;
    }

    private void OnWeaponIndexChanged(int oldIndex, int newIndex)
    {
        if (IsOwner || IsServer || IsOwnedByServer)
        {
            UpdateWeapon(newIndex);
        }
    }

    private void UpdateWeapon(int newWeaponIndex)
    {
        DeactivateAllWeapons();
        if (newWeaponIndex >= 0 && newWeaponIndex < weapons.Count)
        {
            weapons[newWeaponIndex].SetActive(true);
        }
    }

    private void UpdateWeaponOnClient()
    {
        if (IsClient && IsOwner)
        {
            UpdateWeapon(weaponIndex.Value);
        }
    }

    public void CollectWeapon(bool isRifle)
    {
        if (IsServer)
        {
            if (isRifle)
            {
                haveRifle.Value = true;
                weaponIndex.Value = 2;
            }
            else
            {
                haveGravityGun.Value = true;
                weaponIndex.Value = 3;
            }
        }
    }

    public void TextBulletCount(int currentBullet, int spareBullet)
    {
        if (IsOwner)
        {
            bulletText.text = $"{currentBullet} / {spareBullet}";
            bulletText.gameObject.SetActive(weaponIndex.Value != 3 && weaponIndex.Value != 0);
        }
    }

    private void DeactivateAllWeapons()
    {
        foreach (var weapon in weapons)
        {
            weapon.SetActive(false);
        }
    }

    public void SetCanSwitchWeaponLocal(bool state)
    {
        canSwitchWeapon.Value = state;
    }

    [ServerRpc]
    public void SetCanSwitchWeaponServerRpc(bool state)
    {
        canSwitchWeapon.Value = state;
    }
}
