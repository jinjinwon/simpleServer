using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

// 보류.. 이펙트 관련되어 생각이 정리되면 작업

public class PoolManager : MonoSingleton<PoolManager>
{
    [Header("Other"), Space(1)]
    [SerializeField]
    private int OtherMaxSize;
    public GameObject OtherPrefab;

    [Header("HitEffect"), Space(1)]
    [SerializeField]
    private int HitEffectMaxSize;
    public GameObject HitPrefab;

    [Header("BulletPrefab"), Space(1)]
    [SerializeField]
    private int BulletMaxSize;
    public GameObject BulletPrefab;

    private void Start()
    {
        ObjectPool.CreatePool(OtherPrefab, OtherMaxSize);
        ObjectPool.CreatePool(HitPrefab, HitEffectMaxSize);
        ObjectPool.CreatePool(BulletPrefab, BulletMaxSize);
    }
    
    public void Spawn(GameObject go, Transform parent = null, Vector3 vector = new(), Quaternion quaternion = new())
    {
        if (parent == null)
            parent = this.transform;

        ObjectPool.Spawn(go, parent, vector, quaternion);
    }

    public GameObject SpawnObject(GameObject go, Transform parent = null, Vector3 vector = new(), Quaternion quaternion = new())
    {
        if (parent == null)
            parent = this.transform;

        return ObjectPool.Spawn(go, parent, vector, quaternion);
    }

    public void Recycle(GameObject go)
    {
        go.Recycle();
    }

    public void RecycleAll()
    {
        ObjectPool.RecycleAll();
    }

    public void DestroyPooled(GameObject go)
    {
        go.DestroyPooled();
    }

    public void DestroyAll(GameObject go)
    {
        ObjectPool.DestroyAll(go);
    }
}
