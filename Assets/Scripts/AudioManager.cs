using UnityEngine;

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
    [SerializeField] private AudioClip buildSound;


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
}
