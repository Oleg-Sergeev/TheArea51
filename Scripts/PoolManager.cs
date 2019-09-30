using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;
    public Transform target;
    public List<Pool> pools;
    public Dictionary<PoolType, Queue<GameObject>> pooledObjects = new Dictionary<PoolType, Queue<GameObject>>();
    public Transform parent;

    private int preferedWidth = 1080;
    private int preferedHeight = 1920;

    private void Awake() => Instance = this;

    private void Start()
    {
        foreach (var currentPool in pools)
        {
            Queue<GameObject> pooledObjs = new Queue<GameObject>();

            for (int i = 0; i < currentPool.defaultSize; i++)
            {
                pooledObjs.Enqueue(CreatePoolObject(currentPool.prefab));
            }

            pooledObjects.Add(currentPool.prefabType, pooledObjs);
        }
    }

    public void Spawn(PoolType poolType)
    {
        if (pooledObjects[poolType].Count == 0)
        {
            var pool = GetPool(poolType);

            if (!pool.allowRuntimeCreating)
            {
                return;
            }

            pooledObjects[poolType].Enqueue(CreatePoolObject(pool.prefab));
        }
        GameObject objToSpawn = pooledObjects[poolType].Dequeue();

        objToSpawn.SetActive(true);
        //??
        objToSpawn.GetComponent<IPooledObj>().OnSpawn(poolType);
    }

    public void Hide(GameObject poolObj, PoolType poolType)
    {
        poolObj.SetActive(false);

        pooledObjects[poolType].Enqueue(poolObj);

        poolObj.transform.localPosition = CheckToReplace(new Vector2(Random.Range(-preferedWidth / 1.5f, preferedWidth / 1.5f), Random.Range(-preferedHeight / 1.5f, preferedHeight / 1.5f)));
    }

    private Pool GetPool(PoolType poolType)
    {
        foreach (var pool in pools)
        {
            if (pool.prefabType == poolType) return pool;
        }
        Debug.LogError($"Pool {poolType} not found");
        return null;
    }

    private GameObject CreatePoolObject(GameObject prefab)
    {
        GameObject objToCreate = Instantiate(prefab);

        objToCreate.transform.SetParent(parent);
        objToCreate.transform.localScale = new Vector3(1, 1, 1);
        objToCreate.transform.localPosition = CheckToReplace(new Vector2(Random.Range(-preferedWidth / 1.5f, preferedWidth / 1.5f), Random.Range(-preferedHeight / 1.5f, preferedHeight / 1.5f)));
        objToCreate.SetActive(false);

        return objToCreate;
    }

    private Vector2 CheckToReplace(Vector2 position)
    {
        float xPos = position.x;
        float yPos = position.y;

        if (xPos > -preferedWidth / 2 && xPos < preferedWidth / 2 && yPos > -preferedHeight / 2 && yPos < preferedHeight / 2)
        {
            if (preferedWidth - xPos >= preferedHeight / 2 - yPos)
            {
                if (xPos >= 0) xPos += Random.Range(preferedWidth / 2, preferedWidth / 1.8f) - xPos;
                else xPos -= Random.Range(preferedWidth / 2, preferedWidth / 1.8f) + xPos;
            }
            else
            {
                if (yPos >= 0) yPos += Random.Range(preferedHeight / 2, preferedHeight / 1.8f) - yPos;
                else yPos -= Random.Range(preferedHeight / 2, preferedHeight / 1.8f) + yPos;
            }

            position = new Vector2(xPos, yPos);
        }
        
        return position;
    }

    [System.Serializable]
    public class Pool
    {
        public PoolType prefabType;
        public GameObject prefab;
        public int defaultSize;
        public bool allowRuntimeCreating;
    }
}
// ???
public enum PoolType
{
    Square,
    Circle,
    Rectangle
}
