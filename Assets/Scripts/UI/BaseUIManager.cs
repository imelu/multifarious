using DG.Tweening;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class BaseUIManager : MonoBehaviour
{
    [SerializeField] private Image _HPBar;
    [SerializeField] private Image _EnemyApproachingBar;
    private Color _initialColor;
    [SerializeField] private Color _pulsateColor;

    [SerializeField] private Image _maskPlayerOne;
    [SerializeField] private Image _maskPlayerTwo;

    [SerializeField] private CanvasGroup _deathScreen;
    public CanvasGroup deathScreen { get { return _deathScreen; } }
    [SerializeField] private Camera _deathCamera;
    public Camera deathCamera { get { return _deathCamera; } }
    [SerializeField] private Camera _tutorialCamera;
    public Camera tutorialCamera { get { return _tutorialCamera; } }

    private void Start()
    {
        _initialColor = _EnemyApproachingBar.color;
    }

    public void UpdateHP(float percent)
    {
        _HPBar.fillAmount = percent;
    }

    public void UpdateEnemyTimer(float percent)
    {
        _EnemyApproachingBar.fillAmount = percent;
        
        if (percent >= 1f)
        {
            //Debug.Log("start tween");
            DOTween.Kill(_EnemyApproachingBar);
            _EnemyApproachingBar.DOColor(_pulsateColor, 1f).SetLoops(-1, LoopType.Yoyo);
        }
    }

    public void EnemiesCleared()
    {
        DOTween.Kill(_EnemyApproachingBar);
        _EnemyApproachingBar.color = _initialColor;
    }

    public void EnableBarMask(bool isPlayerOne)
    {
        if (isPlayerOne) _maskPlayerOne.enabled = true;
        else _maskPlayerTwo.enabled = true;
    }

    public void DisableBarMasks()
    {
        _maskPlayerOne.enabled = false;
        _maskPlayerTwo.enabled = false;
    }
}