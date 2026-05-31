using UnityEngine;

public class BulletOffline : MonoBehaviour
{

    private void Start() {
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
