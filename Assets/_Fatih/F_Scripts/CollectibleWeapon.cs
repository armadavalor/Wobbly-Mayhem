using UnityEngine;
using Unity.Netcode;

public class CollectibleWeapon : NetworkBehaviour
{
    [SerializeField] private bool isRifle;

    private void Update()
    {
        if (gameObject.activeInHierarchy == true)
        {
            transform.Rotate(0f, 45f * Time.deltaTime, 0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Sunucuda silah toplama iþlemini gerçekleþtir
            if (IsServer)
            {
                HandleWeaponCollection(other.GetComponentInChildren<WeaponManager>(), isRifle);
            }
            else
            {
                // Ýstemci tarafýndan sunucuya silah toplama isteði gönder
                RequestWeaponCollectionServerRpc(isRifle);
            }
        }
    }

    private void HandleWeaponCollection(WeaponManager weaponManager, bool isRifle)
    {
        if (weaponManager != null)
        {
            // Silah koleksiyon iþlemini gerçekleþtir
            weaponManager.CollectWeapon(isRifle);

            // Silahý sahneden kaldýr
            gameObject.SetActive(false);

            // Silahýn kaldýrýldýðýný tüm istemcilere bildir
            NotifyClientsWeaponCollectedClientRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestWeaponCollectionServerRpc(bool isRifle, ServerRpcParams rpcParams = default)
    {
        // Silah toplama iþlemini sunucu tarafýnda gerçekleþtir
        var weaponManager = NetworkManager.Singleton.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject.GetComponentInChildren<WeaponManager>();
        if (weaponManager != null)
        {
            HandleWeaponCollection(weaponManager, isRifle);
        }
    }

    [ClientRpc]
    private void NotifyClientsWeaponCollectedClientRpc()
    {
        // Silahýn kaldýrýldýðýný tüm istemcilere bildir
        // Bu metod istemcilerin silahýn kaldýrýldýðýný görmesini saðlar
        gameObject.SetActive(false);
    }
}
