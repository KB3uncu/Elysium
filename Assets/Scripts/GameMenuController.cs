using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Pause Settings")]
    public bool canPause = true;
    public KeyCode pauseKey = KeyCode.Escape;

    [Header("Disable While Paused")]
    [Tooltip("Pause olunca kapanacak scriptler. Buraya FPSController ve gerekiyorsa PlayerInteractor ekle.")]
    public MonoBehaviour[] scriptsToDisableOnPause;

    [Header("Cursor Settings")]
    public bool lockCursorDuringGameplay = true;

    private bool isPaused = false;
    private bool gameplayScriptsDisabled = false;
    private bool[] previousScriptStates;

    void Start()
    {
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        SetGameplayCursor();
    }

    void Update()
    {
        if (!canPause) return;

        if (Input.GetKeyDown(pauseKey))
        {
            if (IsEndPanelOpen())
                return;

            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;

        DisableGameplayScripts();

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f;
        SetMenuCursor();
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;

        RestoreGameplayScripts();

        StartCoroutine(LockCursorAfterResume());
    }

    IEnumerator LockCursorAfterResume()
    {
        yield return null;

        SetGameplayCursor();

        yield return new WaitForEndOfFrame();

        SetGameplayCursor();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        RestoreGameplayScripts();

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ShowWinPanel()
    {
        canPause = false;
        isPaused = false;

        DisableGameplayScripts();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(true);

        if (losePanel != null)
            losePanel.SetActive(false);

        Time.timeScale = 0f;
        SetMenuCursor();
    }

    public void ShowLosePanel()
    {
        canPause = false;
        isPaused = false;

        DisableGameplayScripts();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (losePanel != null)
            losePanel.SetActive(true);

        if (winPanel != null)
            winPanel.SetActive(false);

        Time.timeScale = 0f;
        SetMenuCursor();
    }

    void DisableGameplayScripts()
    {
        if (gameplayScriptsDisabled) return;
        if (scriptsToDisableOnPause == null) return;

        previousScriptStates = new bool[scriptsToDisableOnPause.Length];

        for (int i = 0; i < scriptsToDisableOnPause.Length; i++)
        {
            if (scriptsToDisableOnPause[i] == null) continue;

            previousScriptStates[i] = scriptsToDisableOnPause[i].enabled;
            scriptsToDisableOnPause[i].enabled = false;
        }

        gameplayScriptsDisabled = true;
    }

    void RestoreGameplayScripts()
    {
        if (!gameplayScriptsDisabled) return;
        if (scriptsToDisableOnPause == null) return;
        if (previousScriptStates == null) return;

        for (int i = 0; i < scriptsToDisableOnPause.Length; i++)
        {
            if (scriptsToDisableOnPause[i] == null) continue;
            if (i >= previousScriptStates.Length) continue;

            scriptsToDisableOnPause[i].enabled = previousScriptStates[i];
        }

        gameplayScriptsDisabled = false;
    }

    bool IsEndPanelOpen()
    {
        bool winOpen = winPanel != null && winPanel.activeSelf;
        bool loseOpen = losePanel != null && losePanel.activeSelf;

        return winOpen || loseOpen;
    }

    void SetMenuCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void SetGameplayCursor()
    {
        if (!lockCursorDuringGameplay) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}