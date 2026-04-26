
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Main Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("Random Pitch Audio Sources")]
    public List<AudioSource> sfxRandomSources = new List<AudioSource>();

    [Header("Music")]
    [SerializeField] AudioClip tutorialMusic;
    [SerializeField] AudioClip mainMenuMusic;
    [SerializeField] AudioClip desertMusic;

    public enum AudioChannel { Sfx, Music };
    public float sfxVolumePercent { get; set; }
    public float musicVolumePercent { get; set; }
    
    LocalWorldManager localWorldManager;
    
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        sfxVolumePercent = GameManager.Instance.sfxVolume;
        musicVolumePercent = GameManager.Instance.musicVolume;
        
        if(!GameManager.Instance.InMainMenu())
            localWorldManager = LevelManager.Instance.localWorldManager;
        else
            localWorldManager = GameObject.FindWithTag("Level Managers").GetComponent<LocalWorldManager>();
        
         //Music Selection
        switch(localWorldManager.world)
        {
            case LocalWorldManager.World.Tutorial:
                musicSource.clip = tutorialMusic;
                break;
            case LocalWorldManager.World.MainMenu:
                musicSource.clip = mainMenuMusic;
                break;

            case LocalWorldManager.World.Desert:
                musicSource.clip = desertMusic;
                break;
        }

        FadeMusicIn(0.9f);
    }

    public void SetVolume(float volumePercent, AudioChannel channel)
    {
        switch(channel)
        {
            case AudioChannel.Sfx:
                sfxVolumePercent = volumePercent;
                break;
            case AudioChannel.Music:
                musicVolumePercent = volumePercent;
                break;
        }

        musicSource.volume = musicVolumePercent;
    }
    public void FadeMusicIn(float duration)
    {
        musicSource.Play();
        StartCoroutine(FadeMusicInCo(duration));
    }

    public void FadeMusicOut(float duration)
    {
        StartCoroutine(FadeMusicOutCo(duration));
    }

    public void PlaySound2D(AudioClip clip)
    {
        if(clip != null)
            sfxSource.PlayOneShot(clip, sfxVolumePercent);
    }

    public void PlayRandomSound2D(AudioClip clip, float min, float max, int audioSourceIndex)
    {
        if(clip != null)
        {
            sfxRandomSources[audioSourceIndex].pitch = Random.Range(min, max);
            sfxRandomSources[audioSourceIndex].PlayOneShot(clip, sfxVolumePercent);
        }
    }

    IEnumerator FadeMusicInCo(float duration)
    {
        float percent = 0;
       
        while(percent < 1)
        {
            percent += Time.unscaledDeltaTime * 1 / duration;
            musicSource.volume = Mathf.Lerp(0, musicVolumePercent, percent);
            yield return null;
        }
    }
    IEnumerator FadeMusicOutCo(float duration)
    {
        float percent = 0;

        while(percent < 1)
        {
            percent += Time.unscaledDeltaTime * 1 / duration;
            musicSource.volume = Mathf.Lerp(musicVolumePercent, 0, percent);
            yield return null;
        }
    }
}