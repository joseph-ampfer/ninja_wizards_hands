using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Global Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Common SFX Clips")]
    [SerializeField] private AudioClip gestureTing;
    [SerializeField] private AudioClip gestureThump;
    [SerializeField] private AudioClip spellConfirm;
    [SerializeField] private AudioClip uiClick;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // persists across scenes
    }

    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (clip && sfxSource)
            sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayGestureTing()
    {
        PlayOneShot(gestureThump);
        PlayOneShot(gestureTing, 0.1f);
    } 

    public void PlaySpellConfirm() => PlayOneShot(spellConfirm);
    public void PlayUIClick() => PlayOneShot(uiClick);

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (!musicSource || clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource) musicSource.Stop();
    }
}
