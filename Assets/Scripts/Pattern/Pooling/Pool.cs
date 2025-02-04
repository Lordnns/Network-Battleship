using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Base class for any pool that are using pool item
/// </summary>
public class Pool : MonoBehaviour
{
    [SerializeField] private PoolItem prefab;
    [SerializeField, Range(0, 20)] private int defaultSize = 0;

    private List<PoolItem> actives = new List<PoolItem>();
    private List<PoolItem> inactives = new List<PoolItem>();


    // create a starting pool of object
    private void Start()
    {
        for(int i = 0; i < defaultSize; i++)
        {
            AddToPool();
        }
    }

    private void AddToPool()
    {
        PoolItem obj = Instantiate(prefab, transform);
        obj.OnRemove(OnRemoveCallBack);
    }

    //get a pool object or add one if none is available 
    public PoolItem GetAPoolObject()
    {
        int index = inactives.Count - 1;
        if(index < 0)
        {
            AddToPool();
            index = 0;
        }

        PoolItem obj = inactives[index];
        inactives.RemoveAt(index);
        actives.Add(obj);
        obj.Activate();
        return obj;
    }

    //when the object is used deactivate it
    public void OnRemoveCallBack(PoolItem obj)
    {
        actives.Remove(obj);
        inactives.Add(obj);
    }

    public List<PoolItem> GetActivePool()
    {
        return actives;
    }
}
