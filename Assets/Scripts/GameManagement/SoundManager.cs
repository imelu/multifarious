using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum MusicStates
{
    inside,
    outside,
    enemies,
    inUI,
    inGame
}

public class SoundManager : MonoBehaviour
{

    private EventInstance _music;
    [SerializeField] private EventReference _musicEvent;

    private MusicStates musicState;
    private MusicStates musicUIState;

    [SerializeField] private bool _playMusic;

    #region Singleton
    public static SoundManager Instance;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            if (_playMusic)
            {
                _music = RuntimeManager.CreateInstance(_musicEvent);
                _music.start();
            }
        }
    }
    #endregion

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.buildIndex)
        {
            case 0:
                musicState = MusicStates.outside;
                musicUIState = MusicStates.inUI;
                SetMusicIntensity(100);
                break;

            case 1:
                musicState = MusicStates.inside;
                musicUIState = MusicStates.inGame;
                break;
        }
        _music.setParameterByName("status", (int)musicState);
        _music.setParameterByName("play-UI", (int)musicUIState);
    }

    public void SetMusicState(MusicStates state)
    {
        _music.setParameterByName("status", (int)state);
        //Debug.Log("music switch to: " + state.ToString());
    }

    public void SetMusicUIState(MusicStates state)
    {
        _music.setParameterByName("play-UI", (int)state);
    }

    public void SetMusicIntensity(int currentHealth)
    {
        _music.setParameterByName("resources", currentHealth);
    }
}
