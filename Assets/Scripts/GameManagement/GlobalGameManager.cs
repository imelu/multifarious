using DG.Tweening;
using FMOD.Studio;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.VisualScripting;
using DG.Tweening.Core.Easing;
using Zhdk.Gamelab;

public class GlobalGameManager : MonoBehaviour
{
    #region Singleton
    public static GlobalGameManager Instance;

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
            _soundManager = GetComponent<SoundManager>();
        }
    }
    #endregion

    private BaseTexManager _baseTexManager;
    public BaseTexManager baseTexManager { get { return _baseTexManager; } }

    private BaseManager _baseManager;
    public BaseManager baseManager { get { return _baseManager; } }

    [SerializeField] private Image _screenFader;
    [SerializeField] private float _fadeInTime;
    [SerializeField] private float _fadeOutTime;

    private SoundManager _soundManager;
    public SoundManager soundManager { get { return _soundManager; } }

    private Coroutine _loadSceneCor;

    private IDisposable _anyButtonListener;

    [SerializeField] private bool _playIntro;

    private void OnDisable()
    {
        if (_anyButtonListener != null) _anyButtonListener.Dispose();
    }

    private void OnApplicationFocus(bool focus)
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //FadeIn();
        //if (scene.buildIndex == 0) _baseTexManager = GameObject.FindGameObjectWithTag("Base").GetComponent<BaseTexManager>();
        
        GameObject Base = GameObject.FindGameObjectWithTag("Base");
        if (Base != null)
        {
            _baseTexManager = Base.GetComponent<BaseTexManager>();
            _baseManager = Base.GetComponent<BaseManager>();
            if (!_playIntro)
            {
                _baseManager.gameIntroManager.StopIntro();
                PlayerStateManager[] players = _baseTexManager.GetComponent<BaseManager>().players;
                foreach (PlayerStateManager p in players)
                {
                    if(p != null) p.transform.parent.gameObject.SetActive(true);
                }
            }
        }

        FadeIn();
        
        if (SceneManager.GetActiveScene().buildIndex == 0) baseTexManager.StartCalculations(); //&& !AttractionScreen.Instance.attractionScreenIsOn
    }

    public bool IsPlaying(EventInstance instance)
    {
        PLAYBACK_STATE state;
        instance.getPlaybackState(out state);
        return state != PLAYBACK_STATE.STOPPED;
    }

    public void LoadSceneIn(float delay, int buildIndex)
    {
        if (_loadSceneCor != null) StopCoroutine(_loadSceneCor);
        _loadSceneCor = StartCoroutine(LoadSceneDelay(delay, buildIndex));
    }

    IEnumerator LoadSceneDelay(float delay, int buildIndex)
    {
        if (delay - _fadeOutTime > 0) yield return new WaitForSeconds(delay - _fadeOutTime);
        FadeOut();
        yield return new WaitForSeconds(_fadeOutTime);
        SceneManager.LoadScene(buildIndex);
    }

    public void FadeOut()
    {
        _screenFader.DOFade(1, _fadeOutTime);
    }

    public void FadeIn()
    {
        _screenFader.DOFade(0, _fadeInTime);
    }

    private void Start()
    {
        Screen.SetResolution(2560, 1440, true);
        InputSystem.settings.SetInternalFeatureFlag("DISABLE_SHORTCUT_SUPPORT", true);
        Application.targetFrameRate = -1;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    public void PlayersDied()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        _screenFader.DOFade(1, _fadeOutTime).OnComplete(() => ActivateDeathScreen());

        
    }

    private void ActivateDeathScreen()
    {
        baseManager.baseUIManager.deathCamera.enabled = true;
        baseManager.baseUIManager.tutorialCamera.enabled = true;
        PlayerStateManager[] players = baseManager.players;
        foreach (PlayerStateManager p in players)
        {
            p.transform.parent.gameObject.SetActive(false);
        }
        _screenFader.DOFade(0, _fadeInTime).OnComplete(() => _anyButtonListener = InputSystem.onAnyButtonPress.Call(ctrl => OnButtonPressed()));
    }

    private void OnButtonPressed()
    {
        if (_anyButtonListener != null) _anyButtonListener.Dispose();
        _screenFader.DOFade(1, _fadeOutTime).OnComplete(() => SceneManager.LoadScene(2));
        //_deathScreen.DOFade(0, 0.5f);
    }

    public void StartGame(Camera tutorialCam)
    {
        if (!_playIntro) return;
        _screenFader.DOFade(1, _fadeOutTime).OnComplete(() => SwitchCameras(tutorialCam));
    }

    private void SwitchCameras(Camera tutorialCam)
    {
        tutorialCam.enabled = false;
        PlayerStateManager[] players = baseManager.players;
        foreach (PlayerStateManager p in players)
        {
            p.transform.parent.gameObject.SetActive(true);
        }
        FadeIn();
    }
}
