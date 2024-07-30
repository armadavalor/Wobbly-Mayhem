using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public float spawnInterval = 2f;
    public float lifeTime = 5f;

    private void Start()
    {
        StartCoroutine(SpawnPrefabRoutine());
    }

    private IEnumerator SpawnPrefabRoutine()
    {
        while (true)
        {
            SpawnPrefab();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnPrefab()
    {
        GameObject spawnedObject = Instantiate(prefabToSpawn, transform.position, transform.rotation);
        StartCoroutine(DestroyAfterTime(spawnedObject, lifeTime));
    }

    private IEnumerator DestroyAfterTime(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(obj);
    }
}