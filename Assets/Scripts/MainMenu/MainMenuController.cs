using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;
using Cinemachine;
using DG.Tweening;
using FMOD;
using FMODUnity;
using Zhdk.Gamelab;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Transform _buttons;

    private List<Transform> _buttonsList = new List<Transform>();

    private int _hoverIndex = 0;

    [SerializeField] private GameObject _creditsGoo;

    [SerializeField] private CinemachineVirtualCamera _creditsCamera;

    private CanvasGroup _buttonsUI;
    [SerializeField] private CanvasGroup _controls;
    [SerializeField] private CanvasGroup _credits;


    [SerializeField] private EventReference _navigate;
    [SerializeField] private EventReference _confirm;

    private enum MenuState
    {
        main,
        controls,
        credits, 
        blocked
    }

    private MenuState _state = MenuState.main;
    private MenuState _prevState;

    private void Start()
    {
        for(int i = 0; i < 4; i++)
        {
            _buttonsList.Add(_buttons.GetChild(i));
        }

        _buttonsUI = _buttons.GetComponent<CanvasGroup>();

        HoverButton();
    }

    private void ClearButton()
    {
        _buttonsList[_hoverIndex].GetChild(0).GetComponent<Image>().enabled = false;
    }

    private void HoverButton()
    {
        _buttonsList[_hoverIndex].GetChild(0).GetComponent<Image>().enabled = true;
    }

    private void OnUp()
    {
        if (_state != MenuState.main) return; // || AttractionScreen.Instance.attractionScreenIsOn
        ClearButton();
        _hoverIndex--;
        if (_hoverIndex < 0) _hoverIndex = _buttonsList.Count - 1;
        HoverButton();

        RuntimeManager.PlayOneShot(_navigate);
    }

    private void OnDown()
    {
        if (_state != MenuState.main) return; // || AttractionScreen.Instance.attractionScreenIsOn
        ClearButton();
        _hoverIndex++;
        if (_hoverIndex > _buttonsList.Count - 1) _hoverIndex = 0;
        HoverButton();

        RuntimeManager.PlayOneShot(_navigate);
    }

    private void OnConfirm()
    {
        if (_state == MenuState.blocked) return; // || AttractionScreen.Instance.attractionScreenIsOn
        if (_state == MenuState.main)
        {
            switch (_hoverIndex)
            {
                case 0:
                    GlobalGameManager.Instance.LoadSceneIn(0, 1);
                    break;

                case 1:
                    _state = MenuState.controls;
                    _controls.DOFade(1, 0.6f).OnComplete(()=>ReleaseUI());
                    BlockUI();
                    break;

                case 2:
                    _creditsCamera.enabled = true;
                    if(_creditsGoo != null)_creditsGoo.SetActive(true);
                    _state = MenuState.credits;
                    _buttonsUI.DOFade(0, 0.7f);
                    Invoke("ShowCredits", 2.5f);
                    BlockUI();
                    break;

                case 3:
                    Application.Quit();
                    break;
            }
        }
        else if(_state == MenuState.credits)
        {
            _creditsCamera.enabled = false;
            _state = MenuState.main;
            _credits.DOFade(0, 0.7f);
            ClearButton();
            _hoverIndex = 0;
            HoverButton();
            Invoke("ShowMainMenu", 2.5f);
            BlockUI();

        }
        else if(_state == MenuState.controls)
        {
            _state = MenuState.main;
            _controls.DOFade(0, 0.7f);
            ClearButton();
            _hoverIndex = 0;
            HoverButton();
            _buttonsUI.DOFade(1, 0.7f).OnComplete(()=>ReleaseUI());
            BlockUI();
        }

        RuntimeManager.PlayOneShot(_confirm);
    }

    private void ShowMainMenu()
    {
        _buttonsUI.DOFade(1, 0.7f);
        ReleaseUI();
    }

    private void ShowCredits()
    {
        _credits.DOFade(1, 0.7f).OnComplete(() => ReleaseUI());
    }

    private void OnCancel()
    {
        //Debug.Log("cancel");
        if(_state == MenuState.credits || _state == MenuState.controls)
        {
            OnConfirm();
        }
    }

    private void BlockUI()
    {
        _prevState = _state;
        _state = MenuState.blocked;
    }

    private void ReleaseUI()
    {
        _state = _prevState;
    }
}
