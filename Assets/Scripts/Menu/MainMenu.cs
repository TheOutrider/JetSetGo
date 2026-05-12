using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public string sceneName;
    public string offlineSeneName;

    private void Start() {
        
    }

    private void Update() {
        
    }

    public void OnGameStart()
    {
        SceneManager.LoadScene(sceneName);
    }

    public void OnOfflineGameStart()
    {
        SceneManager.LoadScene(offlineSeneName);
    }
}