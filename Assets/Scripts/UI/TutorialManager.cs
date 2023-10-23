using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class TutorialManager : MonoBehaviour
{
    private BaseManager _baseManager;
    [SerializeField] private PlayerStateManager _player;
    [SerializeField] private BaseWindow _baseWindow;

    [SerializeField] private CompassController _compass;

    [SerializeField] private RectTransform _popup;
    [SerializeField] private TMP_Text _upperText;
    [SerializeField] private TMP_Text _lowerText;

    [SerializeField] private RectTransform _hidePos;
    [SerializeField] private RectTransform _showPos;

    [SerializeField] private TutorialData _data;

    private TutorialAction _currentAction = TutorialAction.findResource;

    public void StartTutorial()
    {
        _baseManager = GlobalGameManager.Instance.baseManager;
        _upperText.text = _data.steps[(int)_currentAction].upperText;
        _lowerText.text = _data.steps[(int)_currentAction].lowerText;

        PrepareAction();
        ShowPopup();
    }

    private void ShowPopup()
    {
        _popup.DOAnchorPosY(_showPos.anchoredPosition.y, 1);
    }

    private void HidePopup()
    {
        _popup.DOAnchorPosY(_hidePos.anchoredPosition.y, 1);
    }

    private void HidePopupShowNext()
    {
        _popup.DOAnchorPosY(_hidePos.anchoredPosition.y, 1).OnComplete(()=>LoadNextAction());
    }

    private void LoadNextAction()
    {
        int current = (int)_currentAction;
        _currentAction = (TutorialAction)(current + 1);
        _upperText.text = _data.steps[(int)_currentAction].upperText;
        _lowerText.text = _data.steps[(int)_currentAction].lowerText;

        PrepareAction();
        ShowPopup();
    }

    private void PrepareAction()
    {
        switch (_currentAction)
        {
            case TutorialAction.findResource:

                // activate tutorial compass needle
                // Set tutoral compass needle to tutorial resource plant
                _compass.ActivateTutorialNeedle(_baseManager.tutorialResourceNode.transform);
                // tell tutorial resource plant to callback on discovered
                _baseManager.tutorialResourceNode.ActivateTutorialMode(_currentAction);

                break;

            case TutorialAction.connectResource:

                // tell tutorial resource plant to callback on connect
                _baseManager.tutorialResourceNode.ActivateTutorialMode(_currentAction);

                break;

            case TutorialAction.findUpgrade:

                // Set tutoral compass needle to tutorial upgrade plant
                _compass.ActivateTutorialNeedle(_baseManager.tutorialUpgradeNode.transform);
                // tell tutorial upgrade plant to callback on discovered
                _baseManager.tutorialUpgradeNode.ActivateTutorialMode(_currentAction);

                break;

            case TutorialAction.connectUpgrade:

                // tell tutorial upgrade plant to callback on connect
                _baseManager.tutorialUpgradeNode.ActivateTutorialMode(_currentAction);

                break;

            case TutorialAction.chooseUpgrade:

                // tell tutorial upgrade plant to callback on chosen
                _baseManager.tutorialUpgradeNode.ActivateTutorialMode(_currentAction);

                break;

            case TutorialAction.returnToBase:

                // Set tutoral compass needle to base
                _compass.ActivateTutorialNeedle(_baseManager.transform);
                // tell player to callback on interactable base reached
                _player.ActivateTutorialMode(_currentAction);

                break;

            case TutorialAction.activateUpgrade:

                // tell baseui to callback on purchased upgrade
                _baseWindow.ActivateTutorialMode(_currentAction);

                break;
        }
    }

    public void ActionExecuted(TutorialAction action)
    {
        if (_currentAction != action) return;
        if(_currentAction != TutorialAction.activateUpgrade)
        {
            _currentAction = (TutorialAction)(int)_currentAction++;
            HidePopupShowNext();
        }
        else
        {
            HidePopup();
            _compass.DeactivateTutorialNeedle();
        }
    }
}
