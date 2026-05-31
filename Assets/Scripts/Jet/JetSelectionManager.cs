using UnityEngine;

public class JetSelectionManager : MonoBehaviour
{
    public static JetSelectionManager Instance { get; private set; }

    public JetData SelectedJet { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SelectJet(JetData jet)
    {
        SelectedJet = jet;
        Debug.Log($"Selected jet: {jet.jetName}");
    }
}