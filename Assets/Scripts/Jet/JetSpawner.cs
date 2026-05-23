using UnityEngine;

public class JetSpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private JetControllerOffline jetControllerPrefab; // your prefab, no body attached

    public JetData spawnedJetBody;

    void Start()
    {
        if (JetSelectionManager.Instance == null || JetSelectionManager.Instance.SelectedJet == null)
        {
            Debug.LogWarning("No jet selected — using default.");
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
        //body.transform.localPosition = Vector3.zero;
        //body.transform.localRotation = Quaternion.identity;

        // Apply JetData stats to the controller
        //controller.ApplyJetData(data);
    }
}