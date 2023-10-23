using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using System;
using UnityEngine.InputSystem.LowLevel;

namespace Zhdk.Gamelab
{
    public class AttractionScreen : MonoBehaviour
    {
        private static AttractionScreen instance;
        public static AttractionScreen Instance => instance;
        public float IdleTime => idleTime;
        public float IdleTimeLimit => idleTimeLimit;

        [Tooltip("The time the game can idle before starting the attraction screen")]
        [SerializeField] private float idleTimeLimit = 180;
        [Tooltip("If this option is enabled the idle time goes up even if Time.timeScale is on 0")]
        [SerializeField] private bool useUnscaledTime = true;
        [Tooltip("Game-play time will be frozen during the attraction screen")]
        [SerializeField] private bool freezeTimeDuringScreen = false;
        [Tooltip("Start the video once the scenes got loaded")]
        [SerializeField] private bool startVideoAfterSceneLoad;
        [Tooltip("The scene / scenes you want to load once we start the attraction screen")]
        [SerializeField] private List<string> restartScenes = new List<string>();

        private Canvas canvas;
        private VideoPlayer videoPlayer;
        private float idleTime;
        private bool _attractionScreenIsOn;
        public bool attractionScreenIsOn { get { return _attractionScreenIsOn; } }
        private Coroutine attractionScreenRoutine;
        [SerializeField] private RenderTexture _attractionScreenRT;

        bool anyInput;
        private IDisposable _anyButtonListener;

        private void OnEnable()
        {
            _anyButtonListener = InputSystem.onAnyButtonPress.Call(ctrl => OnButtonPressed());
        }

        private void OnDisable()
        {
            if(_anyButtonListener != null) _anyButtonListener.Dispose();
        }

        private void OnButtonPressed()
        {
            anyInput = true;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            if(TryGetComponent(out videoPlayer) == false)
            {
                Debug.LogError("Please attach a video player component to this GameObject!");
            }

            if (TryGetComponent(out canvas) == false)
            {
                Debug.LogError("Please attach this script to a GameObject with a UI canvas!");
            }
        }

        private void Update()
        {
            if (Gamepad.all.Count > 0)
            {
                if (Gamepad.current.leftStick.ReadValue().x > 0.1f || Gamepad.current.leftStick.ReadValue().y > 0.1f) OnButtonPressed();
                if (Gamepad.current.rightStick.ReadValue().x > 0.1f || Gamepad.current.rightStick.ReadValue().y > 0.1f) OnButtonPressed();
            }

            if (_attractionScreenIsOn)
            {
                return;
            }

            if (anyInput == false && idleTime > idleTimeLimit)
            {
                if(attractionScreenRoutine != null)
                {
                    StopCoroutine(attractionScreenRoutine);
                }

                attractionScreenRoutine = StartCoroutine(StartAttractionScreen());
                idleTime = 0;
            }
            else
            {
                if(anyInput)
                {
                    idleTime = 0;
                }
                else
                {
                    idleTime += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                }
            }

            anyInput = false;
        }

        private IEnumerator StartAttractionScreen()
        {
            _attractionScreenIsOn = true;

            _attractionScreenRT.Release();

            var timeBeforeFreezing = Time.timeScale;
            if(freezeTimeDuringScreen)
            {
                Time.timeScale = 0;
            }

            if(startVideoAfterSceneLoad == false)
            {
                videoPlayer.Play();
                canvas.enabled = true;
            }

            // load scenes
            for(int i = 0; i < restartScenes.Count; i++)
            {
                // load first scene and make it the active one
                SceneManager.LoadScene(restartScenes[i], i == 0 ? LoadSceneMode.Single: LoadSceneMode.Additive);
            }

            if (startVideoAfterSceneLoad)
            {
                videoPlayer.Play();
                canvas.enabled = true;
            }

            while (!anyInput)
            {
                yield return null;
            }
            
            GlobalGameManager.Instance.baseTexManager.StartCalculations();

            canvas.enabled = false;
            videoPlayer.Stop();
            _attractionScreenIsOn = false;
            attractionScreenRoutine = null;
            
            if(freezeTimeDuringScreen)
            {
                Time.timeScale = timeBeforeFreezing;
            }

            yield return null;
        }
    }
}
