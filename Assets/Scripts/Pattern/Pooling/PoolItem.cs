using System;
using UnityEngine;
/// <summary>
/// Base class for any item that is used in a pool
/// </summary>
public class PoolItem : MonoBehaviour
{
    public Action<PoolItem> onRemoveCallBack;
    
    //callback to notify when the object has to be removed
    public void OnRemove(Action<PoolItem> callback)
    {
        onRemoveCallBack = callback;
        Remove();
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }

    public void Remove()
    {
        onRemoveCallBack?.Invoke(this);
        gameObject.SetActive(false);
    }
}
