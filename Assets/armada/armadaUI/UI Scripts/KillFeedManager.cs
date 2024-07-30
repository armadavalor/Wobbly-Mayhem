using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class KillFeedManager : MonoBehaviour
{
    public static KillFeedManager Instance;

    public GameObject killFeedItemPrefab;
    public Transform killFeedContent;

    private Queue<GameObject> killFeedItems = new Queue<GameObject>();

    void Awake()
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

    public void AddKillFeedItem(string killer, string victim)
    {
        GameObject item = Instantiate(killFeedItemPrefab, killFeedContent);
        item.GetComponent<TextMeshProUGUI>().text = $"{killer} killed {victim}";
        killFeedItems.Enqueue(item);

        if (killFeedItems.Count > 10)
        {
            Destroy(killFeedItems.Dequeue());
        }
    }
}