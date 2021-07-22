using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public AudioController.SoundType audioType = AudioController.SoundType.sfx;

    public AudioClip audioClip;
    public int musicClipIndex;
    public bool playOnAwake = true;
    [Range(0, 1)]
    public float volume = 1;
    public bool loop = false;

    [HideInInspector]
    public AudioSource source;

    private void Start()
    {
        StartCoroutine(SetupPlayerCor());
    }

    IEnumerator SetupPlayerCor()
    {
        yield return new WaitUntil(() => FirebaseController.currentSaveData != null);

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
            source.volume = volume;
            if (audioType == AudioController.SoundType.sfx)
            {
                source.clip = audioClip;
                if (!FirebaseController.Instance.Sfx)
                {
                    source.volume = 0;
                }
            }
            else if (audioType == AudioController.SoundType.music && musicClipIndex < AudioController.Instance.musicClips.Count)
            {
                source.clip = AudioController.Instance.musicClips[musicClipIndex];
                if (!FirebaseController.Instance.Music)
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
