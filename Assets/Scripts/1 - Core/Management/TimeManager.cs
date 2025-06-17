using UnityEngine;

namespace TabletopShop
{
public class TimeManager : MonoBehaviour
{
    [Header("Time Control")]
    [SerializeField] private KeyCode pauseKey = KeyCode.P;
    [SerializeField] private KeyCode speedUpKey = KeyCode.RightShift;
    [SerializeField] private float[] speedOptions = { 0f, 0.5f, 1f, 2f, 4f }; // 0=pause, 1=normal, 2=2x, etc.
    
    private int currentSpeedIndex = 2; // Start at normal speed (1f)
    private bool isPaused = false;
    
    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
        
        if (Input.GetKeyDown(speedUpKey))
        {
            CycleSpeed();
        }
    }
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : speedOptions[currentSpeedIndex];
        Debug.Log($"Game {(isPaused ? "Paused" : "Unpaused")} - TimeScale: {Time.timeScale}");
    }
    
    public void CycleSpeed()
    {
        if (!isPaused)
        {
            currentSpeedIndex = (currentSpeedIndex + 1) % speedOptions.Length;
            
            // Skip pause speed when cycling (index 0)
            if (speedOptions[currentSpeedIndex] == 0f)
            {
                currentSpeedIndex = (currentSpeedIndex + 1) % speedOptions.Length;
            }
            
            Time.timeScale = speedOptions[currentSpeedIndex];
            Debug.Log($"Speed changed to {Time.timeScale}x");
        }
    }
    
    public void SetSpeed(float speed)
    {
        Time.timeScale = speed;
        isPaused = (speed == 0f);
        
        // Update currentSpeedIndex to match
        for (int i = 0; i < speedOptions.Length; i++)
        {
            if (Mathf.Approximately(speedOptions[i], speed))
            {
                currentSpeedIndex = i;
                break;
            }
        }
    }
}
}
