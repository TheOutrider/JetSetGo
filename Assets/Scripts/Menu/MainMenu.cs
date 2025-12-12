using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public string sceneName;

    private void Start() {
        
    }

    private void Update() {
        
    }

    public void OnGameStart()
    {
        SceneManager.LoadScene(sceneName);
    }
}