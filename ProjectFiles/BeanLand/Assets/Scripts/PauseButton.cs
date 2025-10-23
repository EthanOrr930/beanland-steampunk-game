using UnityEngine;

public class PauseButton : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenu; // The UI element to show when paused (assign in inspector)
    
    private bool isPaused = false;
    
    // Called when the pause button is clicked
    public void OnPauseButtonClicked()
    {
        if (pauseMenu != null)
        {
            // Show the pause menu
            pauseMenu.SetActive(true);
            
            // Hide the pause button itself
            gameObject.SetActive(false);
            
            // Pause the game
            PauseGame();
        }
        else
        {
            Debug.LogWarning("PauseButton: No pause menu assigned in inspector!");
        }
    }
    
    // Public function to resume the game (can be called from resume button)
    public void ResumeGame()
    {
        if (pauseMenu != null)
        {
            // Hide the pause menu
            pauseMenu.SetActive(false);
            
            // Show the pause button again
            gameObject.SetActive(true);
            
            // Resume the game
            UnpauseGame();
        }
    }
    
    // Pause the game by setting time scale to 0
    private void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
        Debug.Log("Game Paused");
    }
    
    // Resume the game by setting time scale back to 1
    private void UnpauseGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        Debug.Log("Game Resumed");
    }
    
    // Public property to check if game is paused
    public bool IsPaused
    {
        get { return isPaused; }
    }
}
