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

    //loading saved data
    public void SetupSources()
    {
        if (FirebaseController.Instance.Music)
        {
            musicSource.volume = 1f;
        }
        else
        {
            musicSource.volume = 0f;
        }
        if (FirebaseController.Instance.Sfx)
        {
            sfxAudioSource.volume = 1f;
        }
        else
        {
            sfxAudioSource.volume = 0f;
        }
    }

    //enable music
    public void EnableMusic()
    {
        musicSource.volume = 1f;
        FirebaseController.Instance.Music = true;
        foreach (AudioPlayer ap in activeAudioPlayersList)
        {
            if (ap.audioType == SoundType.music)
            {
                ap.EnableAudio();
            }
        }
    }

    //disable music
    public void DisableMusic()
    {
        musicSource.volume = 0f;
        FirebaseController.Instance.Music = false;
        foreach (AudioPlayer ap in activeAudioPlayersList)
        {
            if (ap.audioType == SoundType.music)
            {
                ap.DisableAudio();
            }
        }
    }

    //toggle music
    public void ToggleMusic()
    {
        bool newState = !FirebaseController.Instance.Music;
        if (newState)
            musicSource.volume = 1f;
        else
            musicSource.volume = 0f;
        FirebaseController.Instance.Music = newState;
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

    //enable sfx
    public void EnableSFX()
    {
        sfxAudioSource.volume = 1f;
        FirebaseController.Instance.Sfx = true;
        foreach (AudioPlayer ap in activeAudioPlayersList)
        {
            if (ap.audioType == SoundType.sfx)
            {
                ap.EnableAudio();
            }
        }
    }

    //disable music
    public void DisableSFX()
    {
        sfxAudioSource.volume = 0f;
        FirebaseController.Instance.Sfx = false;
        foreach (AudioPlayer ap in activeAudioPlayersList)
        {
            if (ap.audioType == SoundType.sfx)
            {
                ap.DisableAudio();
            }
        }
    }

    //toggle sfx
    public void ToggleSFX()
    {
        bool newState = !FirebaseController.Instance.Sfx;
        if (newState)
        {
            sfxAudioSource.volume = 1f;
        }
        else
        {
            sfxAudioSource.volume = 0f;
        }
        FirebaseController.Instance.Sfx = newState;
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

    //play win
    public void PlayWin()
    {
        if (FirebaseController.Instance.Sfx && sfxAudioSource && winClip)
        {
            sfxAudioSource.clip = winClip;
            sfxAudioSource.Play();
        }
    }

    //play lose
    public void PlayLose()
    {
        if (FirebaseController.Instance.Sfx && sfxAudioSource)
        {
            sfxAudioSource.clip = loseClip;
            sfxAudioSource.Play();
        }
    }

    //adding audioplayer to active audioplayers list
    public void AddAudioPlayer(AudioPlayer ap)
    {
        if (!activeAudioPlayersList.Contains(ap))
            activeAudioPlayersList.Add(ap);
    }

    //removing audioplayer from active audioplayers list
    public void RemoveAudioPlayer(AudioPlayer ap)
    {
        activeAudioPlayersList.Remove(ap);
    }

    //pausing all audio sources
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

    //unpausing all audio sources
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
