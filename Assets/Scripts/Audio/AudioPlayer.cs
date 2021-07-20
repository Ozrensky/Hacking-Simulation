using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public AudioController.SoundType audioType = AudioController.SoundType.sfx;

    public AudioClip audioClip;
    public int musicClipIndex;
    public bool playOnAwake = true;
    public bool loop = false;

    [HideInInspector]
    public AudioSource source;

    private void Start()
    {
        StartCoroutine(SetupPlayerCor());
    }

    IEnumerator SetupPlayerCor()
    {
        yield return new WaitUntil(() => SaveController.currentSaveData != null);

        SetupPlayer();
    }

    private void SetupPlayer()
    {
        if (AudioController.Instance != null)
        {
            AudioController.Instance.AddAudioPlayer(this);
            source = gameObject.AddComponent<AudioSource>();
            source.loop = loop;
            source.playOnAwake = playOnAwake;
            if (audioType == AudioController.SoundType.sfx)
            {
                source.clip = audioClip;
                if (!SaveController.currentSaveData.sfx)
                {
                    source.volume = 0;
                }
            }
            else if (audioType == AudioController.SoundType.music && musicClipIndex < AudioController.Instance.musicClips.Count)
            {
                source.clip = AudioController.Instance.musicClips[musicClipIndex];
                if (!SaveController.currentSaveData.music)
                {
                    source.volume = 0;
                }
            }
            if (playOnAwake && source)
            {
                source.Play();
            }
        }
    }

    public void PlayAudio()
    {
        if (source != null && source.clip != null)
            source.Play();
    }

    public void StopAudio()
    {
        if (source != null && source.clip != null)
            source.Stop();
    }

    public void EnableAudio()
    {
        if (source != null)
            source.volume = 1;
    }

    public void DisableAudio()
    {
        if (source != null)
            source.volume = 0;
    }

    private void OnDestroy()
    {
        if (AudioController.Instance != null)
        {
            AudioController.Instance.RemoveAudioPlayer(this);
        }
    }
}
