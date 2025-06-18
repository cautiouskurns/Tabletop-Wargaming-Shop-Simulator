using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuManager : MonoBehaviour
{
    [Header("Menu Controls")]
    [SerializeField] private KeyCode menuKey = KeyCode.Escape;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    private void Update()
    {
        if (Input.GetKeyDown(menuKey))
        {
            ReturnToMainMenu();
        }
    }
    
    public void ReturnToMainMenu()
    {
        Debug.Log("Returning to main menu...");
        
        // Reset time scale in case game was paused
        Time.timeScale = 1f;
        
        SceneManager.LoadScene(mainMenuSceneName);
    }
}