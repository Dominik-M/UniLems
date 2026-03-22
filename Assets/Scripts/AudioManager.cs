using UnityEngine;
public enum SoundEffect
{
    CLICK,
    WIN,
    LOOSE,
    DIG,
    EXPLODE,
    BUILD,
    SAVED,
    FALL_DEATH,
    ENDLESS_FALL_DEATH,
    BURN,
    SHOCK,
    DROWN

}
public class AudioManager : MonoBehaviour
{

    [Header("UI References")]
    [SerializeField] private AudioSource audioSourceMusic;
    [SerializeField] private AudioSource audioSourceSFX;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip clicksound;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip defeatSound;
    [SerializeField] private AudioClip digSound;
    [SerializeField] private AudioClip explodeSound;
    [SerializeField] private AudioClip savedSound;
    [SerializeField] private AudioClip buildSound;
    [SerializeField] private AudioClip fallDeathSound;
    [SerializeField] private AudioClip endlessFallDeathSound;
    [SerializeField] private AudioClip burnSound;
    [SerializeField] private AudioClip shockSound;
    [SerializeField] private AudioClip drownSound;

    public AudioClip GetSound(SoundEffect sfx)
    {
        switch (sfx)
        {
            case SoundEffect.CLICK: return clicksound;
            case SoundEffect.WIN: return victorySound;
            case SoundEffect.LOOSE: return defeatSound;
            case SoundEffect.DIG: return digSound;
            case SoundEffect.EXPLODE: return explodeSound;
            case SoundEffect.BUILD: return buildSound;
            case SoundEffect.SAVED: return savedSound;
            case SoundEffect.FALL_DEATH: return fallDeathSound;
            case SoundEffect.ENDLESS_FALL_DEATH: return endlessFallDeathSound;
            case SoundEffect.BURN: return burnSound;
            case SoundEffect.SHOCK: return shockSound;
            case SoundEffect.DROWN: return drownSound;
            default:
                Debug.LogWarning("Sound effect not defined: " + sfx);
                return null;
        }
    }

    void OnEnable()
    {
        Core.Instance.OnGameOver += HandleGameOver;
        Core.Instance.OnPauseResume += HandlePauseResume;
        Core.Instance.OnNewLevelStarted += PlayLevelMusic;
    }

    void OnDisable()
    {
        Core.Instance.OnGameOver -= HandleGameOver;
        Core.Instance.OnPauseResume -= HandlePauseResume;
        Core.Instance.OnNewLevelStarted -= PlayLevelMusic;
    }

    public void PlayLevelMusic(Level level)
    {
        PlayMusic(level.Music);
    }

    public void PlayMusic(AudioClip music)
    {
        audioSourceMusic.clip = music;
        audioSourceMusic.loop = true;
        audioSourceMusic.Play();
    }

    public void StopMusic()
    {
        audioSourceMusic.Stop();
    }
    public void PlayClickSound()
    {
        AudioClip sound = GetSound(SoundEffect.CLICK);
        if (sound != null)
        {
            audioSourceSFX.PlayOneShot(sound);
        }
    }

    public void PlaySound(SoundEffect sfx)
    {
        AudioClip sound = GetSound(sfx);
        if (sound != null)
        {
            audioSourceSFX.PlayOneShot(sound);
        }
    }

    private void HandleGameOver(bool win)
    {
        StopMusic();
    }

    private void HandlePauseResume(bool running)
    {
        if (running)
            audioSourceMusic.Play();
        else
            audioSourceMusic.Pause();
    }

    public float MusicVolume
    {
        get { return audioSourceMusic.volume; }
        set { audioSourceMusic.volume = value; }
    }
    public float EffectsVolume
    {
        get { return audioSourceSFX.volume; }
        set { audioSourceSFX.volume = value; }
    }
}
