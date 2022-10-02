using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    private Dictionary<GameObject, Queue<GameObject>> _prefabInstances;

    private void Awake()
    {
        Debug.Assert(Instance == null, "ObjectPool.Awake: Attempted to create multiple instances of ObjectPool.");

        Instance = this;
        _prefabInstances = new Dictionary<GameObject, Queue<GameObject>>();
    }

    public GameObject RequestInstance(GameObject prefab,
        Transform desiredParent = null, Vector3? desiredPosition = null, Vector3? desiredRight = null)
    {
        _prefabInstances.TryGetValue(prefab, out var freeInstances);

        GameObject prefabInstance;
        if (freeInstances?.Count > 0)
        {
            prefabInstance = freeInstances.Dequeue();
            if (freeInstances.Count <= 0)
            {
                _prefabInstances.Remove(prefab);
            }
        }
        else
        {
            prefabInstance = Instantiate(prefab, desiredParent ?? this.transform);
        }

        if (desiredPosition.HasValue)
        {
            prefabInstance.transform.position = desiredPosition.Value;
        }
        if (desiredRight.HasValue)
        {
            prefabInstance.transform.right = desiredRight.Value;
        }

        return prefabInstance;
    }

    public GameObject[] RequestInstances(GameObject prefab, int count, Transform desiredParent = null)
    {
        var prefabInstances = new GameObject[count];
        for (int i = 0; i < count; ++i)
        {
            prefabInstances[i] = RequestInstance(prefab, desiredParent);
        }
        return prefabInstances;
    }

    public void ReleaseInstance(GameObject prefabInstance)
    {
        var prefab = PrefabUtility.GetCorrespondingObjectFromSource(prefabInstance);
        if (!_prefabInstances.TryGetValue(prefab, out var freeInstances))
        {
            freeInstances = new Queue<GameObject>();
            _prefabInstances.Add(prefab, freeInstances);
        }
        freeInstances.Enqueue(prefabInstance);
    }
}