using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class JetMenuUI : MonoBehaviour
{
    [SerializeField] private JetDatabase jetDatabase;
    [SerializeField] private Transform previewRoot;       // 3D preview position in menu
    [SerializeField] private TextMeshProUGUI jetNameText;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button selectButton;

    private int currentIndex = 0;
    private GameObject currentPreviewInstance;

    void Start()
    {
        // Ensure manager exists in menu scene too
        if (JetSelectionManager.Instance == null)
        {
            new GameObject("JetSelectionManager")
                .AddComponent<JetSelectionManager>();
        }

        prevButton.onClick.AddListener(PreviousJet);
        nextButton.onClick.AddListener(NextJet);
        selectButton.onClick.AddListener(ConfirmSelection);

        ShowJet(currentIndex);
    }

    void ShowJet(int index)
    {
        // Destroy old preview
        if (currentPreviewInstance != null)
            Destroy(currentPreviewInstance);

        JetData jet = jetDatabase.jets[index];

        // Spawn preview mesh
        currentPreviewInstance = Instantiate(jet.jetBody, previewRoot);
        currentPreviewInstance.transform.localPosition = Vector3.zero;
        currentPreviewInstance.transform.localRotation = Quaternion.identity;

        jetNameText.text = jet.jetBody.name;
    }

    void PreviousJet()
    {
        currentIndex = (currentIndex - 1 + jetDatabase.jets.Count) % jetDatabase.jets.Count;
        ShowJet(currentIndex);
    }

    void NextJet()
    {
        currentIndex = (currentIndex + 1) % jetDatabase.jets.Count;
        ShowJet(currentIndex);
    }

    void ConfirmSelection()
    {
        JetSelectionManager.Instance.SelectJet(jetDatabase.jets[currentIndex]);
        SceneManager.LoadScene(GameManager.Instance.SelectedMap.ToString());
    }
}