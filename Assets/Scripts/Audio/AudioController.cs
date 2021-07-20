using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance;

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxAudioSource;

    [Header("Clips")]
    [Header("Music")]
    public List<AudioClip> musicClips = new List<AudioClip>();
    [Header("SFX")]
    public AudioClip winClip;
    public AudioClip loseClip;

    public enum SoundType { music, sfx}

    List<AudioPlayer> activeAudioPlayersList = new List<AudioPlayer>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    public void SetupSources()
    {
        if (SaveController.currentSaveData.music)
        {
            musicSource.volume = 1f;
        }
        else
        {
            musicSource.volume = 0f;
        }
        if (SaveController.currentSaveData.sfx)
        {
            sfxAudioSource.volume = 1f;
        }
        else
        {
            sfxAudioSource.volume = 0f;
        }
    }

    public void EnableMusic()
    {
        musicSource.volume = 1f;
        SaveController.currentSaveData.music = true;
        SaveController.WriteSaveData();
        foreach (AudioPlayer ap in activeAudioPlayersList)
        {
            if (ap.audioType == SoundType.music)
            {
                ap.EnableAudio();
            }
        }
    }

    public void DisableMusic()
    {
        musicSource.volume = 0f;
        SaveController.currentSaveData.music = false;
        SaveController.WriteSaveData();
        foreach (AudioPlayer ap in activeAudioPlayersList)
        {
            if (ap.audioType == SoundType.music)
            {
                ap.DisableAudio();
            }
        }
    }

    public void ToggleMusic()
    {
        bool newState = !SaveController.currentSaveData.music;
        if (newState)
            musicSource.volume = 1f;
        else
            musicSource.volume = 0f;
        SaveController.currentSaveData.music = newState;
        SaveController.WriteSaveData();
        foreach (AudioPlayer ap in activeAudioPlayersList)
        {
            if (ap.audioType == SoundType.music)
            {
                if (newState)
                    ap.EnableAudio();
                else
                    ap.DisableAudio();
            }
        }
    }

    public void EnableSFX()
    {
        sfxAudioSource.volume = 1f;
        SaveController.currentSaveData.sfx = true;
        SaveController.WriteSaveData();
        foreach (AudioPlayer ap in activeAudioPlayersList)
        {
            if (ap.audioType == SoundType.sfx)
            {
                ap.EnableAudio();
            }
        }
    }

    public void DisableSFX()
    {
        sfxAudioSource.volume = 0f;
        SaveController.currentSaveData.sfx = false;
        SaveController.WriteSaveData();
        foreach (AudioPlayer ap in activeAudioPlayersList)
        {
            if (ap.audioType == SoundType.sfx)
            {
                ap.DisableAudio();
            }
        }
    }

    public void ToggleSFX()
    {
        bool newState = !SaveController.currentSaveData.sfx;
        if (newState)
        {
            sfxAudioSource.volume = 1f;
        }
        else
        {
            sfxAudioSource.volume = 0f;
        }
        SaveController.currentSaveData.sfx = newState;
        SaveController.WriteSaveData();
        foreach (AudioPlayer ap in activeAudioPlayersList)
        {
            if (ap.audioType == SoundType.sfx)
            {
                if (newState)
                    ap.EnableAudio();
                else
                    ap.DisableAudio();
            }
        }
    }

    public void PlayWin()
    {
        if (SaveController.currentSaveData.sfx && sfxAudioSource && winClip)
        {
            sfxAudioSource.clip = winClip;
            sfxAudioSource.Play();
        }
    }

    public void PlayLose(string tag)
    {
        if (SaveController.currentSaveData.sfx && sfxAudioSource)
        {
            sfxAudioSource.clip = loseClip;
            sfxAudioSource.Play();
        }
    }

    public void AddAudioPlayer(AudioPlayer ap)
    {
        if (!activeAudioPlayersList.Contains(ap))
            activeAudioPlayersList.Add(ap);
    }

    public void RemoveAudioPlayer(AudioPlayer ap)
    {
        activeAudioPlayersList.Remove(ap);
    }

    public void PauseAllSources()
    {
        musicSource.Pause();
        sfxAudioSource.Pause();
        foreach (AudioPlayer player in activeAudioPlayersList)
        {
            if (player.source)
            {
                player.source.Pause();
            }
        }
    }

    public void UnPauseAllSources()
    {
        musicSource.UnPause();
        sfxAudioSource.UnPause();
        foreach (AudioPlayer player in activeAudioPlayersList)
        {
            if (player.source)
            {
                player.source.UnPause();
            }
        }
    }
}
