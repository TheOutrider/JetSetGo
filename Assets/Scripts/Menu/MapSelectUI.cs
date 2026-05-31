using UnityEngine;
using UnityEngine.UI;
public class MapSelectUI : MonoBehaviour
{
    public void OnTropicsClicked() => GameManager.Instance.SelectTropics();
    public void OnCityLineClicked() => GameManager.Instance.SelectCityLine();
    public void OnNeonJungleClicked() => GameManager.Instance.SelectNeonJungle();
    public void OnBackClicked() => GameManager.Instance.ReturnToMainMenu();
}