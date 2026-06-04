using UnityEngine;

public class MainMenu : MonoBehaviour
{

    public GameObject ButtonSidePanel;
    public GameObject MapPanel;

     [SerializeField] private float rotationSpeed = 50f; // Degrees per second
     [SerializeField] private RectTransform rectTransform ;

    private void Update()
    {
        rectTransform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    public void OnTimeLimitClicked()
    {
        GameManager.Instance.SelectTimeLimit();
        OnModeClicked();
    }

    public void OnRaceOfflineClicked()
    {
        GameManager.Instance.SelectRaceOffline();
        OnModeClicked();
    }

    public void OnEscortMissionClicked()
    {
        GameManager.Instance.SelectEscortMission();
        OnModeClicked();
    }

    private void OnModeClicked()
    {
        ButtonSidePanel.SetActive(false);
        MapPanel.SetActive(true);
    }

}