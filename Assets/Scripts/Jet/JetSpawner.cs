using UnityEngine;

public class JetSpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private JetControllerOffline jetControllerPrefab; // your prefab, no body attached

    public JetData spawnedJetBody;
    public GameObject spawnedBodyInstance; // reference to the spawned GO


    void Start()
    {
        if (JetSelectionManager.Instance == null || JetSelectionManager.Instance.SelectedJet == null)
        {
            Debug.LogWarning("No jet selected � using default.");
            return;
        }
        SpawnSelectedJet(JetSelectionManager.Instance.SelectedJet);
    }

    void SpawnSelectedJet(JetData data)
    {
        GameObject body = Instantiate(data.jetBody, spawnPoint.position, spawnPoint.rotation, jetControllerPrefab.gameObject.transform);
        spawnedJetBody = data;
        ApplyMeshCollider(body);
        jetControllerPrefab.OnJetSpawned(); 
        jetControllerPrefab.ApplyJetData(data);
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
        MeshCollider meshCollider = jetControllerPrefab.gameObject.GetComponent<MeshCollider>();
        if (meshCollider == null)
            meshCollider = jetControllerPrefab.gameObject.AddComponent<MeshCollider>();

        meshCollider.sharedMesh = colliderMesh;
        meshCollider.convex = true; 
    }
}