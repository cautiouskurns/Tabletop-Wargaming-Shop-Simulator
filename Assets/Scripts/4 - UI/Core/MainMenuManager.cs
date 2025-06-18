using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "ShopScene"; // Your main game scene name
    
    public void StartGame()
    {
        Debug.Log("Starting game...");
        SceneManager.LoadScene(gameSceneName);
    }
    
    public void LoadGame()
    {
        Debug.Log("Load game functionality not implemented yet");
        // TODO: Implement save/load system
        // For now, just start the game
        StartGame();
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}