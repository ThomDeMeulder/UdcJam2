using System.Collections.Generic;
using UnityEngine;

public class GenericPool : MonoBehaviour
{
    protected int defaultSize;
    protected GameObject prefab;

    protected readonly Queue<GameObject> pooledObjects = new Queue<GameObject>();

    public static GenericPool Create(GameObject gameObject, int defaultSize, GameObject prefab)
    {
        var component = gameObject.AddComponent<GenericPool>();
        component.defaultSize = defaultSize;
        component.prefab = prefab;
        return component;
    }

    protected virtual void Start()
    {
        for (var i = 0; i < defaultSize; i++) pooledObjects.Enqueue(SpawnInactiveGameObject());
    }

    public virtual void ReturnObject(GameObject gameObject)
    {
        if (pooledObjects.Count == defaultSize)
        {
            Destroy(gameObject);
            return;
        }

        gameObject.SetActive(false);
        pooledObjects.Enqueue(gameObject);
    }

    public virtual GameObject GetActiveGameObject()
    {
        if (pooledObjects.Count == 0) return SpawnActiveGameObject();
        else
        {
            var gameObject = pooledObjects.Dequeue();
            gameObject.SetActive(true);
            return gameObject;
        }
    }

    public virtual GameObject GetInactiveGameObject()
    {
        if (pooledObjects.Count == 0) return SpawnInactiveGameObject();
        else return pooledObjects.Dequeue();
    }

    protected virtual GameObject SpawnInactiveGameObject()
    {
        var gameObject = SpawnActiveGameObject();
        gameObject.SetActive(false);
        return gameObject;
    }

    protected virtual GameObject SpawnActiveGameObject()
    {
        return Instantiate(prefab);
    }
}
