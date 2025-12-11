using System.Collections.Generic;
using UnityEngine;

// 디자인 패턴: 싱글톤 (Singleton)
// 어디서든 ObjectPooler.Instance로 접근 가능하게 만듭니다.
public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    // 인스펙터에서 설정할 풀 정보 클래스
    [System.Serializable]
    public class Pool
    {
        public string tag;        // 오브젝트 식별자 (예: "PlayerBullet", "EnemyBullet")
        public GameObject prefab; // 생성할 프리팹
        public int size;          // 미리 만들어둘 개수 (예: 200개)
    }

    [Header("Pool Settings")]
    public List<Pool> pools; // 인스펙터에서 여러 종류의 풀을 등록

    // 빠른 검색을 위한 딕셔너리 (태그 -> 대기열)
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 풀 생성 시작
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            // 1. 부모 정리용 오브젝트 생성 (Hierarchy 창 깔끔하게)
            GameObject poolParent = new GameObject($"Pool_{pool.tag}");
            poolParent.transform.SetParent(this.transform);

            for (int i = 0; i < pool.size; i++)
            {
                // 2. 오브젝트 미리 생성
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false); // 일단 꺼둠
                obj.transform.SetParent(poolParent.transform); // 정리용 부모 밑으로
                objectPool.Enqueue(obj); // 대기열에 추가
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    // 오브젝트를 꺼내오는 함수 (Spawn)
    public GameObject SpawnFromPool(string tag, Vector2 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }

        // 1. 대기열에서 하나 꺼냄
        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        // 2. 위치/회전 설정 및 활성화
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        // 3. (선택) 초기화 인터페이스가 있다면 실행 (예: IPooledObject)
        // IPooledObject pooledObj = objectToSpawn.GetComponent<IPooledObject>();
        // if (pooledObj != null) pooledObj.OnObjectSpawn();

        // 4. 다시 대기열 맨 뒤로 넣음 (재사용 준비)
        // 주의: 이렇게 하면 활성화된 애를 또 꺼낼 수도 있지만, 탄막 게임에선 
        // 큐 방식이 순환되므로 보통 탄환이 사라지기 전에 큐가 한 바퀴 돌지 않도록 
        // size를 넉넉히 잡으면 문제 없습니다.
        poolDictionary[tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }
}