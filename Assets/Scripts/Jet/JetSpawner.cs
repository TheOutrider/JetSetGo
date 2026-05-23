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
            // Optionally load a fallback
            return;
        }

        SpawnSelectedJet(JetSelectionManager.Instance.SelectedJet);
    }

    void SpawnSelectedJet(JetData data)
    {
        // Spawn the controller prefab
        //JetControllerOffline controller = Instantiate(
        //    jetControllerPrefab,
        //    spawnPoint.position,
        //    spawnPoint.rotation
        //);

        // Attach the jet body mesh under the controller
        GameObject body = Instantiate(data.jetBody, spawnPoint.position, spawnPoint.rotation, jetControllerPrefab.gameObject.transform);

        spawnedJetBody = data;

        ApplyMeshCollider(body);
        jetControllerPrefab.OnJetSpawned(); // <-- notify controller
    jetControllerPrefab.ApplyJetData(data);
        //body.transform.localPosition = Vector3.zero;
        //body.transform.localRotation = Quaternion.identity;

        // Apply JetData stats to the controller
        //controller.ApplyJetData(data);
    }

    void ApplyMeshCollider(GameObject body)
    {
        // Collect all MeshFilters from the spawned body (including children)
        MeshFilter[] meshFilters = body.GetComponentsInChildren<MeshFilter>();

        if (meshFilters.Length == 0)
        {
            Debug.LogWarning("No MeshFilter found on spawned jet body.");
            return;
        }

        // Use the first (or largest) mesh for the collider
        // If your jet is a single mesh, meshFilters[0] is fine
        Mesh colliderMesh = meshFilters[0].sharedMesh;

        // Add or reuse MeshCollider on THIS player GameObject
        MeshCollider meshCollider = jetControllerPrefab.gameObject.GetComponent<MeshCollider>();
        if (meshCollider == null)
            meshCollider = jetControllerPrefab.gameObject.AddComponent<MeshCollider>();

        meshCollider.sharedMesh = colliderMesh;
        meshCollider.convex = true; // Required if using Rigidbody physics
    }
}