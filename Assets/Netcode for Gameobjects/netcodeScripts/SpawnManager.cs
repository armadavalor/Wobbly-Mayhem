using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    public List<Transform> spawnPoints;

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
    public Transform GetRandomSpawnPoint()
    {
        if (spawnPoints.Count == 0) return null;
        int index = Random.Range(0, spawnPoints.Count);
        return spawnPoints[index];
    }
}