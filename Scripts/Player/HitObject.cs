using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitObject : MonoBehaviour
{
    [SerializeField]
    private float time = 1f;

    void Start()
    {
        Invoke("Recycle", time);
    }

    private void Recycle()
    {
        PoolManager.Instance.Recycle(this.gameObject);
    }
}
