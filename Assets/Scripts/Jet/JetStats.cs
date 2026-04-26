using UnityEngine;
using PurrNet;
using UnityEngine.SceneManagement;

public class JetStats : NetworkBehaviour
{

    [SerializeField] private SyncVar<int> health = new SyncVar<int>(100);
    [SerializeField] private int selfLayer, otherLayer;

    public int Health => health.value;

    protected override void OnSpawned()
    {
        base.OnSpawned();
        enabled = isOwner;
        var actualLayer = isOwner ? selfLayer : otherLayer;
        SetLayerRecursive(gameObject, actualLayer);

    }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform) { 
            SetLayerRecursive(child.gameObject, layer);
        }
    }

    [ServerRpc(requireOwnership:false)]
    private void ChangeHealth(int amount)
    {
        health.value += amount;
    }

    private void Update()
    {

    }

}