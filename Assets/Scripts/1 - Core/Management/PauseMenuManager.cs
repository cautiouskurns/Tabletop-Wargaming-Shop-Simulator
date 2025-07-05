using UnityEngine;
using UnityEngine.SceneManagement;
using TabletopShop;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    private bool isPaused = false;
    private CursorManager cursorManager;
    
    public bool IsPaused => isPaused;
    
    private void Start()
    {
        // Make sure pause menu starts hidden
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
            
        // Find the cursor manager
        cursorManager = FindAnyObjectByType<CursorManager>();
        if (cursorManager == null)
        {
            Debug.LogWarning("PauseMenuManager: No CursorManager found! Cursor control may not work properly.");
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }
    
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
            
        // Use cursor manager for proper cursor control
        if (cursorManager != null)
        {
            cursorManager.SetCursorState(false); // Unlock for menu interaction
        }
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
            
        // Use cursor manager for proper cursor control
        if (cursorManager != null)
        {
            cursorManager.SetCursorState(true); // Lock for gameplay
        }
    }
    
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Reset time scale
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
