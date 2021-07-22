using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class SettingsController : MonoBehaviour
{
    public TextMeshProUGUI musicText;
    public TextMeshProUGUI sfxText;
    public TMP_InputField nodeCountInputField;
    public TMP_InputField firewallCountInputField;
    public TMP_InputField treasureCountInputField;
    public TMP_InputField spamCountInputField;
    public TMP_InputField spamDecreaseInputField;
    public TMP_InputField trapDelayInputField;
    public Button musicBtn;
    public Button sfxBtn;
    public Button confirmBtn;
    public Button cancelBtn;

    bool music;
    bool sfx;
    HackingController.GameSettings settingsData = new HackingController.GameSettings();

    //loading data and adding listeners to buttons and input fields to filter input
    private void Start()
    {
        LoadData();
        musicBtn.onClick.AddListener(()=> ToggleMusic());
        sfxBtn.onClick.AddListener(() => ToggleSfx());
        confirmBtn.onClick.AddListener(() => ConfirmChanges());
        cancelBtn.onClick.AddListener(() => CancelChanges());
        nodeCountInputField.onEndEdit.AddListener(delegate {
            if (nodeCountInputField.text == "")
                nodeCountInputField.text = "1";
            settingsData.nodeCount = int.Parse(nodeCountInputField.text);
            settingsData.nodeCount = Mathf.Clamp(settingsData.nodeCount, 1, 30);
            nodeCountInputField.text = settingsData.nodeCount.ToString();
            CheckFieldValue(firewallCountInputField, delegate { settingsData.firewallCount = int.Parse(firewallCountInputField.text); });
            CheckFieldValue(spamCountInputField, delegate { settingsData.spamCount = int.Parse(spamCountInputField.text); });
            CheckFieldValue(treasureCountInputField, delegate { settingsData.treasureCount = int.Parse(treasureCountInputField.text); });
        });
        firewallCountInputField.onEndEdit.AddListener(delegate { CheckFieldValue(firewallCountInputField, delegate { settingsData.firewallCount = int.Parse(firewallCountInputField.text); }); });
        treasureCountInputField.onEndEdit.AddListener(delegate { CheckFieldValue(treasureCountInputField, delegate { settingsData.treasureCount = int.Parse(treasureCountInputField.text); }); });
        spamCountInputField.onEndEdit.AddListener(delegate { CheckFieldValue(spamCountInputField, delegate { settingsData.spamCount = int.Parse(spamCountInputField.text); }); });
    }

    public void LoadData()
    {
        if (HackingController.Instance != null)
        {
            if (music = FirebaseController.Instance.Music)
                musicText.text = "On";
            else
                musicText.text = "Off";
            if (sfx = FirebaseController.Instance.Sfx)
                sfxText.text = "On";
            else
                sfxText.text = "Off";
            settingsData = HackingController.Instance.settings.Copy();
            nodeCountInputField.text = settingsData.nodeCount.ToString();
            firewallCountInputField.text = settingsData.firewallCount.ToString();
            treasureCountInputField.text = settingsData.treasureCount.ToString();
            spamCountInputField.text = settingsData.spamCount.ToString();
            spamDecreaseInputField.text = settingsData.spamDecrease.ToString();
            trapDelayInputField.text = settingsData.trapDelay.ToString();
        }
    }

    //checking if input field values are valid
    void CheckFieldValue(TMP_InputField inputField, System.Action setFieldValue = null)
    {
        if (inputField.text != "" && inputField.text != "0")
        {
            if (setFieldValue != null)
                setFieldValue.Invoke();
            int difference = settingsData.nodeCount - settingsData.firewallCount - settingsData.spamCount - settingsData.treasureCount;
            if (difference < 0)
            {
                inputField.text = (Mathf.Clamp(int.Parse(inputField.text) - Mathf.Abs(difference), 0, settingsData.nodeCount)).ToString();
            }
            if (setFieldValue != null)
                setFieldValue.Invoke();
        }
        else
        {
            if (inputField == treasureCountInputField)
                inputField.text = "1";
            else
                inputField.text = "0";
        }
    }

    public void ToggleMusic()
    {
        music = !music;
        if (music)
            musicText.text = "On";
        else
            musicText.text = "Off";
    }

    public void ToggleSfx()
    {
        sfx = !sfx;
        if (sfx)
            sfxText.text = "On";
        else
            sfxText.text = "Off";
    }

    //confirming and saving changes
    public void ConfirmChanges()
    {
        if (!music && AudioController.Instance)
            AudioController.Instance.DisableMusic();
        else if (AudioController.Instance)
            AudioController.Instance.EnableMusic();
        if (!sfx && AudioController.Instance)
            AudioController.Instance.DisableSFX();
        else if (AudioController.Instance)
            AudioController.Instance.EnableSFX();
        FirebaseController.Instance.Music = music;
        FirebaseController.Instance.Sfx = sfx;
        FirebaseController.Instance.NodeCount = int.Parse(nodeCountInputField.text);
        FirebaseController.Instance.TreasureCount = int.Parse(treasureCountInputField.text);
        FirebaseController.Instance.FirewallCount = int.Parse(firewallCountInputField.text);
        FirebaseController.Instance.SpamCount = int.Parse(spamCountInputField.text);
        FirebaseController.Instance.SpamDecrease = float.Parse(spamDecreaseInputField.text);
        FirebaseController.Instance.TrapDelay = float.Parse(trapDelayInputField.text);
        if (HackingController.Instance)
            HackingController.Instance.LoadData();
        UIManager.Instance.CloseSettingsPanel();
    }

    public void CancelChanges()
    {
        UIManager.Instance.CloseSettingsPanel();
    }

    private void OnDisable()
    {
        LoadData();
    }
}
