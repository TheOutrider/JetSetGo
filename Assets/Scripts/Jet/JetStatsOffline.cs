using UnityEngine;

public class JetStatsOffline : MonoBehaviour
{
    [SerializeField] private int health = 100;
    [SerializeField] private int selfLayer, otherLayer;

    public int Health => health;

    private JetCanvasOffline jetCanvas;



    void Start()
    {
        // var actualLayer = isOwner ? selfLayer : otherLayer;
        SetLayerRecursive(gameObject, selfLayer);
        jetCanvas = GetComponent<JetCanvasOffline>();

    //   health.on += ChangeHealth;
    }

    // private void OnDestroy() {
    //     health.onChanged -= ChangeHealth;
    // }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform) { 
            SetLayerRecursive(child.gameObject, layer);
        }
    }

    private void ChangeHealth(int amount)
    {
        health += amount;
        jetCanvas.SetHealth(health);
    }

    private void Update()
    {

    }

}