using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static GameState gameState = GameState.idle;
    public static bool loading = false;

    public enum LevelDifficulty { easy, medium, hard }
    public enum GameState { idle, play, pause }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        ReloadData();
    }

    public void ReloadData()
    {
        StartCoroutine(GameStartSetup());
    }

    IEnumerator GameStartSetup()
    {
        SaveController.LoadSaveData();

        yield return new WaitUntil(() => SaveController.currentSaveData != null);

        AudioController.Instance.SetupSources();
        HackingController.Instance.LoadData();
    }

    public void LoadLevel()
    {
        StartCoroutine(LoadingCoroutine());
    }

    IEnumerator LoadingCoroutine()
    {
        loading = true;
        Time.timeScale = 1;
        UIManager.Instance.OpenLoadingScreen();
        UIManager.Instance.ToggleLevelPanel(false);
        UIManager.Instance.CloseMainMenuPanel();
        HackingController.Instance.SetupLevel();
        yield return new WaitWhile(()=> !HackingController.setupDone);
        yield return new WaitForSeconds(1f);
        UIManager.Instance.l_levelPanel.SetActive(true);
        UIManager.Instance.CloseLoadingScreen();
        gameState = GameState.play;
        yield return null;
    }

    public void LevelFinished()
    {
        
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void StartNewGame()
    {
        
    }
}
