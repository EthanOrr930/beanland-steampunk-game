using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelCard : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI levelNumberText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject newIndicator;
    [SerializeField] private Button button;
    
    [Header("Display Settings")]
    [SerializeField] private Color playedColor = new Color(1f, 1f, 1f, 0.7f);
    [SerializeField] private Color newColor = Color.white;
    
    private string levelName;
    private MainMenu mainMenu;
    
    private void Awake()
    {
        // Auto-find references if not set
        if (levelNumberText == null)
        {
            Transform levelNumberTransform = transform.Find("LevelNumber");
            if (levelNumberTransform != null)
                levelNumberText = levelNumberTransform.GetComponent<TextMeshProUGUI>();
        }
        
        if (scoreText == null)
        {
            Transform scoreTransform = transform.Find("Score");
            if (scoreTransform != null)
                scoreText = scoreTransform.GetComponent<TextMeshProUGUI>();
        }
        
        if (newIndicator == null)
        {
            Transform indicatorTransform = transform.Find("NewIndicator");
            if (indicatorTransform != null)
                newIndicator = indicatorTransform.gameObject;
        }
        
        if (button == null)
        {
            button = GetComponent<Button>();
        }
        
        // Find MainMenu
        mainMenu = FindFirstObjectByType<MainMenu>();
        
        // Setup button click listener
        if (button != null)
        {
            button.onClick.AddListener(OnCardClicked);
        }
    }
    
    public void SetupCard(string levelName, bool hasBeenPlayed, float bestTime)
    {
        this.levelName = levelName;
        
        // Set level number/name
        if (levelNumberText != null)
        {
            levelNumberText.text = levelName;
        }
        
        // Set score display
        if (scoreText != null)
        {
            if (hasBeenPlayed && bestTime > 0)
            {
                scoreText.text = FormatTime(bestTime);
                scoreText.gameObject.SetActive(true);
            }
            else
            {
                scoreText.text = "";
                scoreText.gameObject.SetActive(false);
            }
        }
        
        // Show/hide new indicator
        if (newIndicator != null)
        {
            newIndicator.SetActive(!hasBeenPlayed);
        }
        
        // Update visual style based on played status
        UpdateCardAppearance(hasBeenPlayed);
    }
    
    private void UpdateCardAppearance(bool hasBeenPlayed)
    {
        // Update card opacity/color based on whether it's been played
        Image[] images = GetComponentsInChildren<Image>();
        foreach (Image img in images)
        {
            if (img.gameObject != newIndicator)
            {
                Color currentColor = img.color;
                currentColor.a = hasBeenPlayed ? playedColor.a : newColor.a;
                img.color = currentColor;
            }
        }
    }
    
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 100f) % 100f);
        
        if (minutes > 0)
        {
            return string.Format("{0}:{1:00}.{2:00}", minutes, seconds, milliseconds);
        }
        else
        {
            return string.Format("{0}.{1:00}", seconds, milliseconds);
        }
    }
    
    private void OnCardClicked()
    {
        if (mainMenu != null && !string.IsNullOrEmpty(levelName))
        {
            mainMenu.LoadLevel(levelName);
        }
        else
        {
            Debug.LogWarning($"LevelCard: Cannot load level. MainMenu: {mainMenu != null}, LevelName: {levelName}");
        }
    }
}

