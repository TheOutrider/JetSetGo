using UnityEngine;

public class EnemyJetSpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private JetAiEnemy jetAiEnemy; 

    public JetData spawnedJetBody;
    public GameObject spawnedBodyInstance; // reference to the spawned GO


    // void Start()
    // {
    //     if (JetSelectionManager.Instance == null || JetSelectionManager.Instance.SelectedJet == null)
    //     {
    //         Debug.LogWarning("No jet selected � using default.");
    //         return;
    //     }
    //     SpawnSelectedJet(JetSelectionManager.Instance.SelectedJet);
    // }

    public void SpawnSelectedJet(JetData data)
    {
        GameObject body = Instantiate(data.jetBody, spawnPoint.position, spawnPoint.rotation, jetAiEnemy.gameObject.transform);
        spawnedJetBody = data;
        spawnedBodyInstance = body;                          // ← store reference
        ApplyMeshCollider(body);
        jetAiEnemy.OnJetSpawned();
        jetAiEnemy.ApplyJetData(data);
    }

    void ApplyMeshCollider(GameObject body)
    {
        MeshFilter[] meshFilters = body.GetComponentsInChildren<MeshFilter>();

        if (meshFilters.Length == 0)
        {
            Debug.LogWarning("No MeshFilter found on spawned jet body.");
            return;
        }

        Mesh colliderMesh = meshFilters[0].sharedMesh;
        MeshCollider meshCollider = jetAiEnemy.gameObject.GetComponent<MeshCollider>();
        if (meshCollider == null)
            meshCollider = jetAiEnemy.gameObject.AddComponent<MeshCollider>();

        meshCollider.sharedMesh = colliderMesh;
        meshCollider.convex = true;

        // Wire the collider reference into SelectedJetBody
        SelectedJetBody selectedJetBody = body.GetComponent<SelectedJetBody>();
        if (selectedJetBody == null)
            selectedJetBody = body.AddComponent<SelectedJetBody>();

        selectedJetBody.meshCollider = meshCollider;        // ← point it at the main GO's collider
    }
}