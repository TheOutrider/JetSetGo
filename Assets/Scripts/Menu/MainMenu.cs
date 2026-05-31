using UnityEngine;

public class MainMenu : MonoBehaviour
{

    public GameObject ButtonSidePanel;
    public GameObject MapPanel;

    public void OnTimeLimitClicked()
    {
        GameManager.Instance.SelectTimeLimit();
        OnModeClicked();
    }

    public void OnSurvivalClicked()
    {
        GameManager.Instance.SelectSurvival();
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