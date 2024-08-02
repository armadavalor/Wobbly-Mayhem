using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KillFeedManager : MonoBehaviour
{
    public static KillFeedManager Instance;

    public GameObject killFeedItemPrefab;
    public Transform killFeedContainer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddKillFeedItem(string killerName, string victimName)
    {
        if (string.IsNullOrEmpty(killerName) || string.IsNullOrEmpty(victimName))
        {
            Debug.LogError("Killer or victim name is null or empty");
            return;
        }

        Debug.Log($"Adding kill feed item: {killerName} killed {victimName}");
        if (killFeedItemPrefab == null)
        {
            Debug.LogError("killFeedItemPrefab is null");
            return;
        }

        if (killFeedContainer == null)
        {
            Debug.LogError("killFeedContainer is null");
            return;
        }

        GameObject killFeedItem = Instantiate(killFeedItemPrefab, killFeedContainer);
        TextMeshProUGUI killFeedText = killFeedItem.GetComponent<TextMeshProUGUI>();
        if (killFeedText == null)
        {
            Debug.LogError("killFeedItem does not have a Text component");
            return;
        }

        killFeedText.text = $"{killerName} killed {victimName}";
        Destroy(killFeedItem, 5f); // Destroy after 5 seconds
    }

}