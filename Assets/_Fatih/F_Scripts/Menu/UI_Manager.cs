using Unity.Netcode;
using UnityEngine;

public class UI_Manager : NetworkBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject playerCanvas;
    private bool escMenuIsOpened = false;

    public WeaponManager weaponManager;

    private void Start()
    {
        // Yerel oyuncunun Canvas'�n� etkinle�tir
        playerCanvas.SetActive(IsOwner);
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Escape tu�una bas�ld���nda ayarlar panelini a�/kapat
        if (IsOwner && Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingsPanel();
        }

        // escMenuIsOpened durumuna g�re ayarlar panelini etkinle�tir/devre d��� b�rak
        settingsPanel.SetActive(escMenuIsOpened);
    }

    private void ToggleSettingsPanel()
    {
        if (settingsPanel.activeInHierarchy == false)
        {
            escMenuIsOpened = true;
            settingsPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            escMenuIsOpened = false;
            settingsPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (IsOwner && weaponManager != null)
        {
            ToggleWeaponSwitchServerRpc(!escMenuIsOpened);
        }
    }


    [ServerRpc]
    private void ToggleWeaponSwitchServerRpc(bool canSwitch, ServerRpcParams rpcParams = default)
    {
        if (weaponManager != null)
        {
            weaponManager.SetCanSwitchWeaponServerRpc(canSwitch);
        }
    }
}
