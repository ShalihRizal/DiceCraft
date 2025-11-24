using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        Instance = this;
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        if (pools == null) pools = new List<Pool>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            if (pool.prefab == null)
            {
                Debug.LogError($"❌ Pool '{pool.tag}' has no prefab assigned!");
                continue;
            }

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        // Check if queue is empty - expand pool if needed
        if (poolDictionary[tag].Count == 0)
        {
            Debug.LogWarning($"Pool '{tag}' is empty! Expanding pool...");
            
            // Find the pool configuration
            Pool poolConfig = null;
            foreach (Pool p in pools)
            {
                if (p.tag == tag)
                {
                    poolConfig = p;
                    break;
                }
            }
            
            if (poolConfig != null)
            {
                // Create 5 more objects
                for (int i = 0; i < 5; i++)
                {
                    GameObject obj = Instantiate(poolConfig.prefab);
                    obj.SetActive(false);
                    poolDictionary[tag].Enqueue(obj);
                }
                Debug.Log($"Expanded pool '{tag}' by 5 objects");
            }
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        // Check if object was destroyed (null check)
        if (objectToSpawn == null)
        {
            Debug.LogWarning($"Object in pool '{tag}' was destroyed! Creating new one...");
            
            // Find the pool configuration and create a new object
            Pool poolConfig = null;
            foreach (Pool p in pools)
            {
                if (p.tag == tag)
                {
                    poolConfig = p;
                    break;
                }
            }
            
            if (poolConfig != null)
            {
                objectToSpawn = Instantiate(poolConfig.prefab);
            }
            else
            {
                Debug.LogError($"Cannot create new object for pool '{tag}' - pool config not found!");
                return null;
            }
        }

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        poolDictionary[tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }
}
