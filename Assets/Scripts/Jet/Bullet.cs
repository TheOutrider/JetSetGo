using PurrNet;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    protected override void OnSpawned()
    {
        base.OnSpawned();
        Destroy(gameObject, 2f);
    }

    void Update()
    {
       
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"COLLISION DETECTED WITH {collision.gameObject.name}");
        Destroy(gameObject);
    }



}
