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

    [Header("Pool Settings")]
    public List<Pool> pools;

    // 태그 -> 오브젝트 리스트 (Queue 대신 List 사용)
    private Dictionary<string, List<GameObject>> poolDictionary;
    private Dictionary<string, GameObject> poolParents;
    private Dictionary<string, Pool> poolSettings;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        poolDictionary = new Dictionary<string, List<GameObject>>();
        poolParents = new Dictionary<string, GameObject>();
        poolSettings = new Dictionary<string, Pool>();

        foreach (Pool pool in pools)
        {
            List<GameObject> objectPool = new List<GameObject>();

            GameObject poolParent = new GameObject($"Pool_{pool.tag}");
            poolParent.transform.SetParent(this.transform);
            poolParents.Add(pool.tag, poolParent);
            poolSettings.Add(pool.tag, pool);

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                obj.transform.SetParent(poolParent.transform);
                objectPool.Add(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector2 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }

        List<GameObject> pool = poolDictionary[tag];

        // 비활성화된 오브젝트 찾기
        GameObject objectToSpawn = null;
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                objectToSpawn = obj;
                break;
            }
        }

        // 모든 오브젝트가 활성화 상태면 새로 생성
        if (objectToSpawn == null)
        {
            if (poolSettings.TryGetValue(tag, out Pool poolSetting))
            {
                objectToSpawn = Instantiate(poolSetting.prefab);
                objectToSpawn.transform.SetParent(poolParents[tag].transform);
                pool.Add(objectToSpawn);
                Debug.Log($"[ObjectPooler] Pool '{tag}' 확장: 현재 {pool.Count}개");
            }
            else
            {
                Debug.LogWarning($"[ObjectPooler] Pool setting for tag {tag} not found.");
                return null;
            }
        }

        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        return objectToSpawn;
    }

    /// <summary>
    /// 오브젝트를 풀로 반환 (비활성화)
    /// </summary>
    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
    }
}