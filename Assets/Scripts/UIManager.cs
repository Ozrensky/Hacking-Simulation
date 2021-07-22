using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    [Header("--- Panels ---")]
    public GameObject mm_mainMenuPanel;
    public GameObject l_winPanel;
    public GameObject l_losePanel;
    public GameObject l_pausePanel;
    public GameObject l_levelPanel;
    public GameObject loadingScreenPanel;
    public GameObject settingsPanel;
    public GameObject fbRewardPanel;

    [Header("Text Elements")]
    [Header("--- Level Screen Elements ---")]
    public TextMeshProUGUI l_xpCounterText;
    public TextMeshProUGUI l_nukeCounterText;
    public TextMeshProUGUI l_trapCounterText;
    public TextMeshProUGUI l_spamActiveText;
    public TextMeshProUGUI l_tracersActiveText;
    [Header("--- Facebook Reward Elements ---")]
    public TextMeshProUGUI fb_xpText;
    public TextMeshProUGUI fb_nukeText;
    public TextMeshProUGUI fb_trapText;
    [Header("--- ActionPanel ---")]
    public GameObject actionPanel;
    public TextMeshProUGUI a_difficultyText;
    public TextMeshProUGUI a_detectionChanceText;
    public Button hackButton;
    public Button nukeButton;
    public Button trapButton;

    RectTransform actionPanelTransform;
    List<UIPanel> openedPanels = new List<UIPanel>();

    public class UIPanel
    {
        public GameObject panelObject;
        public bool escapeEnabled = true;
        public Action onClose;
    }

    public static UIManager Instance;

    UIPanel GetOpenedUIPanel(GameObject panelObject)
    {
        foreach (UIPanel panel in openedPanels)
        {
            if (panel.panelObject == panelObject)
                return panel;
        }
        return null;
    }

    public void OpenPanel(GameObject panel)
    {
        AddOpenedPanel(new UIPanel() { panelObject = panel });
    }

    void AddOpenedPanel(UIPanel panel)
    {
        if (GetOpenedUIPanel(panel.panelObject) == null)
        {
            panel.panelObject.SetActive(true);
            openedPanels.Add(panel);
        }
    }

    public void CloseOpenedPanel(GameObject panelObject)
    {
        UIPanel panel;
        if ((panel = GetOpenedUIPanel(panelObject)) != null)
        {
            panel.panelObject.SetActive(false);
            if (panel.onClose != null)
                panel.onClose.Invoke();
            openedPanels.Remove(panel);
        }
    }

    public void DisableAllOpenedPanels()
    {
        mm_mainMenuPanel.SetActive(false);
        l_levelPanel.SetActive(false);
        CloseMainMenuPanel();
        CloseWinPanel();
        CloseLosePanel();
        foreach (UIPanel panel in openedPanels)
        {
            if (panel != null)
                panel.panelObject.SetActive(false);
        }
        openedPanels.Clear();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
            ShowMainMenuPanel();
            actionPanelTransform = actionPanel.GetComponent<RectTransform>();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void OpenLoadingScreen()
    {
        DisableAllOpenedPanels();
        loadingScreenPanel.SetActive(true);
    }

    public void CloseLoadingScreen()
    {
        loadingScreenPanel.SetActive(false);
    }

    public void UpdateLevelUI()
    {
        l_nukeCounterText.text = FirebaseController.Instance.NukeCount.ToString();
        l_trapCounterText.text = FirebaseController.Instance.TrapCount.ToString();
        l_xpCounterText.text = FirebaseController.Instance.XpAmount.ToString();
        l_tracersActiveText.text = HackingController.tracersActive.ToString();
        l_spamActiveText.text = HackingController.spamActive.ToString();
    }

    public void ShowWinPanel()
    {
        l_winPanel.SetActive(true);
    }

    public void CloseWinPanel()
    {
        l_winPanel.SetActive(false);
    }

    public void ShowLosePanel()
    {
        l_losePanel.SetActive(true);
    }

    public void CloseLosePanel()
    {
        l_losePanel.SetActive(false);
    }

    public void ShowMainMenuPanel()
    {
        mm_mainMenuPanel.SetActive(true);
    }

    public void CloseMainMenuPanel()
    {
        mm_mainMenuPanel.SetActive(false);
    }

    public void ExitLevel()
    {
        DisableAllOpenedPanels();
        ToggleLevelPanel(false);
        ShowMainMenuPanel();
        GameManager.gameState = GameManager.GameState.idle;
    }

    public void ShowPausePanel()
    {
        l_pausePanel.SetActive(true);
        AddOpenedPanel(new UIPanel() { panelObject = l_pausePanel, escapeEnabled = true });
    }

    public void ClosePausePanel()
    {
        CloseOpenedPanel(l_pausePanel);
    }

    public void ToggleLevelPanel(bool value)
    {
        l_levelPanel.SetActive(value);
    }

    private void Update()
    {
        if (Input.GetKeyDown("escape"))
        {
            if (openedPanels.Count > 0)
            {
                CloseOpenedPanel(openedPanels[openedPanels.Count - 1].panelObject);
            }
            else
            {
                if (GameManager.gameState == GameManager.GameState.play)
                {
                    ShowPausePanel();
                }
            }
        }
    }

    public void OpenFbRewardPanel(int xpAmount, int nukeAmount, int trapAmount)
    {
        fb_nukeText.text = nukeAmount.ToString();
        fb_trapText.text = trapAmount.ToString();
        fb_xpText.text = xpAmount.ToString();
        OpenPanel(fbRewardPanel);
    }

    public void CloseFbRewardPanel()
    {
        CloseOpenedPanel(fbRewardPanel);
    }

    public void OpenSettingsPanel()
    {
        OpenPanel(settingsPanel);
    }

    public void CloseSettingsPanel()
    {
        CloseOpenedPanel(settingsPanel);
    }

    public void GenerateNewLevel(bool restart)
    {
        GameManager.Instance.LoadLevel(restart);
    }

    //show and set values in action panel
    public void ShowActionPanel(Node n)
    {
        if (HackingController.Instance.GetClickedNode() == null || HackingController.Instance.GetClickedNode() != n)
        {
            HackingController.Instance.SetClickedNode(n);
            a_difficultyText.text = n.hackingDifficulty.ToString();
            a_detectionChanceText.text = n.chanceToTriggerTracer.ToString("0.0") + "%";
            if (Mathf.Abs(n.trans.position.y - Camera.main.rect.yMin) < Mathf.Abs(n.trans.position.y - Camera.main.rect.yMax))
                actionPanelTransform.position = new Vector2(n.trans.position.x, n.trans.position.y + n.sRenderer.bounds.size.y * 2f);
            else
                actionPanelTransform.position = new Vector2(n.trans.position.x, n.trans.position.y - n.sRenderer.bounds.size.y * 2f);
            hackButton.onClick.RemoveAllListeners();
            hackButton.onClick.AddListener(delegate { n.HackNode(); CloseActionPanel(); });
            if (FirebaseController.Instance.NukeCount == 0)
            {
                nukeButton.interactable = false;
            }
            else
            {
                nukeButton.interactable = true;
                nukeButton.onClick.RemoveAllListeners();
                nukeButton.onClick.AddListener(delegate { n.Nuke(); CloseActionPanel(); });
            }
            if (FirebaseController.Instance.TrapCount == 0)
            {
                trapButton.interactable = false;
            }
            else
            {
                trapButton.interactable = true;
                trapButton.onClick.RemoveAllListeners();
                trapButton.onClick.AddListener(delegate { n.TrapNode(); CloseActionPanel(); });
            }
            actionPanel.SetActive(true);
        }
    }

    public void CloseActionPanel()
    {
        HackingController.Instance.SetClickedNode(null);
        actionPanel.SetActive(false);
    }
}