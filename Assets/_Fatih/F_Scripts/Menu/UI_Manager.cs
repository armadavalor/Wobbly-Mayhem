using UnityEngine;

public class UI_Manager : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    private bool escMenuIsOpened = false;

    public WeaponManager weaponManager;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingsPanel();
        }

        // Update the UI based on the current state of escMenuIsOpened
        settingsPanel.SetActive(escMenuIsOpened);
    }

    private void ToggleSettingsPanel()
    {
        escMenuIsOpened = !escMenuIsOpened;

        if (weaponManager != null)
        {
            weaponManager.canSwitchWeapon = escMenuIsOpened;
        }
    }
}
