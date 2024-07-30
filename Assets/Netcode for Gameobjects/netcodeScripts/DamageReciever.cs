using UnityEngine;

public class DamageReceiver : MonoBehaviour
{
    public PlayerController2 playerController;

    public void ApplyDamage(float damage,ulong killerId)
    {
        if (playerController != null)
        {
            playerController.ApplyDamage(damage, killerId);
        }
        else
        {
            Debug.LogError("PlayerController2 reference is missing on DamageReceiver.");
        }
    }

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = GetComponentInParent<PlayerController2>();
            if (playerController == null)
            {
                Debug.LogError("PlayerController2 not found in parent hierarchy.");
            }
        }
    }
}