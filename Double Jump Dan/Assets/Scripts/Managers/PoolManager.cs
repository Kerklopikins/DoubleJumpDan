using UnityEngine;
using System.Collections.Generic;
public class PoolManager: MonoBehaviour
{
    public static PoolManager Instance;

    [Header("Custom Pools")]
    public CustomPool[] customPools;

    Dictionary<string, LinkedList<ObjectInstance>> poolDictionary = new Dictionary<string, LinkedList<ObjectInstance>>();
    
    void Awake()
    {
        Instance = this;
        
        if(customPools.Length > 0)
            for(int i = 0; i < customPools.Length; i++)
                CreatePool(customPools[i].poolID, customPools[i].prefab, customPools[i].poolSize);
    }

    public void CreatePool(string poolID, GameObject prefab, int poolSize)
    {        
        if(!poolDictionary.ContainsKey(poolID))
        {
            poolDictionary.Add(poolID, new LinkedList<ObjectInstance>());

            GameObject poolHolder = new GameObject(poolID + " Pool");
            poolHolder.transform.parent = transform;

            for(int i = 0; i < poolSize; i++)
            {
                ObjectInstance newObject = new ObjectInstance(Instantiate(prefab, poolHolder.transform));
                poolDictionary[poolID].AddLast(newObject);
            }
        }
        else
        {
            GameObject poolHolder = gameObject.transform.Find(poolID + " Pool").gameObject;

            for(int i = 0; i < poolSize; i++)
            {
                ObjectInstance newObject = new ObjectInstance(Instantiate(prefab, poolHolder.transform));
                poolDictionary[poolID].AddLast(newObject);
            }
        }
    }

    public GameObject ReuseObject(string poolID, object data)
    {
        if(poolDictionary.ContainsKey(poolID))
        {
            ObjectInstance objectToReuse = poolDictionary[poolID].First.Value;
            poolDictionary[poolID].RemoveFirst();
            poolDictionary[poolID].AddLast(objectToReuse);

            objectToReuse.ReuseObject(data);
            return objectToReuse.gameObject;
        }
        else
        {
            Debug.LogError("Pool ID : (" + poolID + ") doesn't exist.");
        }

        return null;
    }

    public class ObjectInstance
    {
        public GameObject gameObject;
        public Transform transform;

        bool hasPoolObjectComponent;
        IPoolable poolable;

        public ObjectInstance(GameObject objectInstance)
        {
            gameObject = objectInstance;
            transform = gameObject.transform;
            gameObject.SetActive(false);

            if(gameObject.GetComponent<IPoolable>() != null)
            {
                hasPoolObjectComponent = true;
                poolable = gameObject.GetComponent<IPoolable>();
            }
        }

        public void ReuseObject(object data)
        {
            gameObject.SetActive(true);

            if(hasPoolObjectComponent)
            {
                poolable.OnObjectReuse(data);
            }
        }

        public void SetParent(Transform parent)
        {
            transform.parent = parent;
        }
    }
}

[System.Serializable]
public struct CustomPool
{
    public string poolID;
    public GameObject prefab;
    public int poolSize;
}