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
            // Sunucuda silah toplama i�lemini ger�ekle�tir
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
            // Silah koleksiyon i�lemini ger�ekle�tir
            weaponManager.CollectWeapon(isRifle);

            // Silah� sahneden kald�r
            gameObject.SetActive(false);

            // Silah�n kald�r�ld���n� t�m istemcilere bildir
            NotifyClientsWeaponCollectedClientRpc();
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

    [ClientRpc]
    private void NotifyClientsWeaponCollectedClientRpc()
    {
        // Silah�n kald�r�ld���n� t�m istemcilere bildir
        // Bu metod istemcilerin silah�n kald�r�ld���n� g�rmesini sa�lar
        gameObject.SetActive(false);
    }
}
