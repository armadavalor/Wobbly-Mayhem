using UnityEngine;
using Unity.Netcode;

public class CollectibleWeapon : NetworkBehaviour
{
    [SerializeField] private bool isRifle;
    private NetworkVariable<bool> isActive = new NetworkVariable<bool>(true);

    private void Update()
    {
        if (isActive.Value && gameObject.activeInHierarchy)
        {
            transform.Rotate(0f, 45f * Time.deltaTime, 0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (IsServer)
            {
                HandleWeaponCollection(other.GetComponentInChildren<WeaponManager>(), isRifle);
            }
            else
            {
                // �stemci taraf�ndan sunucuya silah toplama iste�i g�nder
                RequestWeaponCollectionServerRpc(isRifle);
            }
        }
    }

    private void HandleWeaponCollection(WeaponManager weaponManager, bool isRifle)
    {
        if (weaponManager != null)
        {
            weaponManager.CollectWeapon(isRifle);
            SetWeaponActiveServerRpc(false);  // Sunucu taraf�nda aktifli�i ayarla
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestWeaponCollectionServerRpc(bool isRifle, ServerRpcParams rpcParams = default)
    {
        // Silah toplama i�lemini sunucu taraf�nda ger�ekle�tir
        var weaponManager = NetworkManager.Singleton.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject.GetComponentInChildren<WeaponManager>();
        if (weaponManager != null)
        {
            HandleWeaponCollection(weaponManager, isRifle);
        }
    }

    [ServerRpc]
    private void SetWeaponActiveServerRpc(bool active)
    {
        isActive.Value = active; // Sunucu taraf�nda NetworkVariable'� g�ncelle
        NotifyClientsWeaponCollectedClientRpc(active); // T�m istemcileri bilgilendir
    }

    [ClientRpc]
    private void NotifyClientsWeaponCollectedClientRpc(bool active)
    {
        gameObject.SetActive(active);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        gameObject.SetActive(isActive.Value);
    }

    private void OnDisable()
    {
        Invoke(nameof(ReenableWeaponServerRpc), 30f);
    }

    [ServerRpc]
    private void ReenableWeaponServerRpc()
    {
        SetWeaponActiveServerRpc(true);
    }
}
