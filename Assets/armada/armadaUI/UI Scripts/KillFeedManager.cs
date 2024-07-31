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

        GameObject killFeedItem = Instantiate(killFeedItemPrefab, killFeedContainer);
        killFeedItem.GetComponent<Text>().text = $"{killerName} killed {victimName}";
        Destroy(killFeedItem, 5f); // Destroy after 5 seconds
    }
}