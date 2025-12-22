using UnityEngine;
using PurrNet;

public class CubeSpawnner : NetworkBehaviour
{
    public GameObject cubePrefab;
    protected override void OnSpawned(bool asServer)
    {
        base.OnSpawned(asServer);
        enabled = isOwner;

    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            Instantiate(cubePrefab, transform.position + transform.forward * 2, Quaternion.identity);
        }
    }
}
