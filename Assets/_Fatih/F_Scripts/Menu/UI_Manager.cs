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
        // Yerel oyuncunun Canvas'ýný etkinleþtir
        playerCanvas.SetActive(IsOwner);
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Escape tuþuna basýldýðýnda ayarlar panelini aç/kapat
        if (IsOwner && Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingsPanel();
        }

        // escMenuIsOpened durumuna göre ayarlar panelini etkinleþtir/devre dýþý býrak
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
