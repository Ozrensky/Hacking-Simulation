﻿using System.Collections;
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
    public TMP_InputField trasureCountInputField;
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

    private void Start()
    {
        LoadData();
        musicBtn.onClick.AddListener(()=> ToggleMusic());
        sfxBtn.onClick.AddListener(() => ToggleSfx());
        confirmBtn.onClick.AddListener(() => ConfirmChanges());
        cancelBtn.onClick.AddListener(() => CancelChanges());
        nodeCountInputField.onValueChanged.AddListener(delegate {
            settingsData.nodeCount = int.Parse(nodeCountInputField.text);
            CheckFieldValue(firewallCountInputField, delegate { settingsData.firewallCount = int.Parse(firewallCountInputField.text); });
            CheckFieldValue(trasureCountInputField, delegate { settingsData.treasureCount = int.Parse(trasureCountInputField.text); });
            CheckFieldValue(spamCountInputField, delegate { settingsData.spamCount = int.Parse(spamCountInputField.text); });
        });
        firewallCountInputField.onValueChanged.AddListener(delegate { CheckFieldValue(firewallCountInputField, delegate { settingsData.firewallCount = int.Parse(firewallCountInputField.text); }); });
        trasureCountInputField.onValueChanged.AddListener(delegate { CheckFieldValue(trasureCountInputField, delegate { settingsData.treasureCount = int.Parse(trasureCountInputField.text); }); });
        spamCountInputField.onValueChanged.AddListener(delegate { CheckFieldValue(spamCountInputField, delegate { settingsData.spamCount = int.Parse(spamCountInputField.text); }); });
    }

    public void LoadData()
    {
        if (HackingController.Instance != null)
        {
            if (music = SaveController.currentSaveData.music)
                musicText.text = "On";
            else
                musicText.text = "Off";
            if (sfx = SaveController.currentSaveData.sfx)
                musicText.text = "On";
            else
                musicText.text = "Off";
            settingsData = HackingController.Instance.settings.Copy();
            nodeCountInputField.text = settingsData.nodeCount.ToString();
            firewallCountInputField.text = settingsData.firewallCount.ToString();
            trasureCountInputField.text = settingsData.treasureCount.ToString();
            spamCountInputField.text = settingsData.spamCount.ToString();
            spamDecreaseInputField.text = settingsData.spamDecrease.ToString();
            trapDelayInputField.text = settingsData.trapDelay.ToString();
        }
    }

    void UpdateSettingsData()
    {
        settingsData.nodeCount = int.Parse(nodeCountInputField.text);
        settingsData.firewallCount = int.Parse(firewallCountInputField.text);
        settingsData.spamCount = int.Parse(spamCountInputField.text);
        settingsData.spamDecrease = float.Parse(spamDecreaseInputField.text);
        settingsData.trapDelay = float.Parse(trapDelayInputField.text);
    }

    void CheckFieldValue(TMP_InputField inputField, System.Action setFieldValue = null)
    {
        if (inputField.text != "")
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
            inputField.text = 0.ToString();
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

    public void ConfirmChanges()
    {
        if (music && AudioController.Instance)
            AudioController.Instance.DisableMusic();
        else if (AudioController.Instance)
            AudioController.Instance.EnableMusic();
        if (sfx && AudioController.Instance)
            AudioController.Instance.DisableSFX();
        else if (AudioController.Instance)
            AudioController.Instance.EnableSFX();
        SaveController.currentSaveData.music = music;
        SaveController.currentSaveData.sfx = sfx;
        SaveController.currentSaveData.nodeCount = int.Parse(nodeCountInputField.text);
        SaveController.currentSaveData.firewallCount = int.Parse(firewallCountInputField.text);
        SaveController.currentSaveData.spamCount = int.Parse(spamCountInputField.text);
        SaveController.currentSaveData.spamDecrease = float.Parse(spamDecreaseInputField.text);
        SaveController.currentSaveData.trapDelay = float.Parse(trapDelayInputField.text);
        SaveController.WriteSaveData();
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
