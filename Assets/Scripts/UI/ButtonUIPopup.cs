using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ButtonUIPopup : MonoBehaviour
{
    [SerializeField] private protected List<Transform> _buttons;
    [SerializeField] private float buttonHeight = 3;

    private int _playersInTrigger = 0;

    private bool _isEnabled = true;
    public bool isEnabled { get { return _isEnabled; } 
        set 
        { 
            _isEnabled = value; 
            if(value && _playersInTrigger > 0) ShowButton(); 
            if(!value) HideButton();
        } 
    }

    private void Start()
    {
        foreach(Transform button in _buttons)
        {
            button.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        _playersInTrigger++;
        if (!isEnabled) return;
        if (_playersInTrigger == 1)
        {
            ShowButton();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        _playersInTrigger--;
        if (!isEnabled) return;
        if (_playersInTrigger > 0) return;
        HideButton();
    }

    private void ShowButton()
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            _buttons[i].gameObject.SetActive(true);
            DOTween.Kill(_buttons[i]);
            _buttons[i].DOLocalMoveY(buttonHeight + i * 0.3f, (buttonHeight - _buttons[i].localPosition.y) / buttonHeight);
        }
    }

    private void HideButton()
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            DOTween.Kill(_buttons[i]);
            GameObject button = _buttons[i].gameObject;
            _buttons[i].DOLocalMoveY(0, _buttons[i].localPosition.y / buttonHeight).OnComplete(() => button.SetActive(false));
        }
    }
}
