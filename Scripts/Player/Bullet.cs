using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    private Vector3 direction;

    private void OnEnable()
    {
        if (this.gameObject.activeSelf)
            Invoke("Destroy", 5f);
    }

    public void SetDirection(Vector3 direction)
    {
        this.direction = direction.normalized;
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Entity entity))
        {
            entity.TakeDamage(10);
            PoolManager.Instance.Recycle(this.gameObject);
        }
    }

    private void Destroy()
    {
        if(this.gameObject.activeSelf)
            PoolManager.Instance.Recycle(this.gameObject);
    }
}
