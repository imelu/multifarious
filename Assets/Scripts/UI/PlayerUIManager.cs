using DG.Tweening;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    private PlayerStateManager _player;
    [SerializeField] private UpgradeWindow _upgradeWindow;
    [SerializeField] private BaseWindow _baseWindow;
    [SerializeField] private AbilityWheel _abilityWheel;
    [SerializeField] private ResourcePreview _resourcePreview;

    [Header("Popups")]
    [SerializeField] private RectTransform _popupPosition;
    [SerializeField] private RectTransform _popupPrevPosition;
    [SerializeField] private RectTransform _upgradeInfo;
    [SerializeField] private float _upgradeInfoTime;
    private Coroutine _upgradeCor;
    [SerializeField] private RectTransform _diconnectedInfo;

    [SerializeField] private CanvasGroup _compass;
    private CompassController _compassController;
    public CompassController compassController { get { return _compassController; } }

    private UpgradeNode _upgradeNode;

    private IMenu _openMenu;

    [Header("Defense")]
    [SerializeField] private CanvasGroup _emitInfo;
    public CanvasGroup emitInfo { get { return _emitInfo; } }
    [SerializeField] private CanvasGroup _steerInfo;
    public CanvasGroup steerInfo { get { return _steerInfo; } }
    [SerializeField] private Image _steerInfoArrow;
    public Image steerInfoArrow { get { return _steerInfoArrow; } }
    [SerializeField] private CanvasGroup _defenseUI;
    [SerializeField] private CanvasGroup _defenseHints;


    [Header("Sound")]
    [SerializeField] private EventReference _enter;
    public EventReference enter { get { return _enter; } }
    [SerializeField] private EventReference _confirm;
    public EventReference confirm { get { return _confirm; } }
    [SerializeField] private EventReference _cancel;
    public EventReference cancel { get { return _cancel; } }
    [SerializeField] private EventReference _navigate;
    public EventReference navigate { get { return _navigate; } }
    [SerializeField] private EventReference _choose;
    public EventReference choose { get { return _choose; } }


    private void Start()
    {
        _player = GetComponent<PlayerStateManager>();
        _compassController = _compass.GetComponent<CompassController>();
    }

    public void OpenResourceWindow(ResourceNode node)
    {
        _player.inUI = true;
        _resourcePreview.GetComponent<CanvasGroup>().alpha = 1;
        _resourcePreview.DisplayResource(node);
        _openMenu = _resourcePreview;
    }

    public void OpenUpgradeWindow(Upgrade[] upgrades, UpgradeNode node)
    {
        _player.inUI = true;
        _upgradeNode = node;
        _upgradeWindow.GetComponent<CanvasGroup>().alpha = 1;   
        _upgradeWindow.DisplayUpgrades(upgrades, node);
        _openMenu = _upgradeWindow;
        HideCompass();
    }

    public void SelectUpgrade(int index)
    {
        ShowUpgradePopup();
        _upgradeNode.SelectedUpgrade(index);
        _upgradeNode = null;
        _player.inUI = false;
        ShowCompass();
    }

    public void OpenBaseWindow(List<Upgrade> upgrades)
    {
        _player.inUI = true;
        _baseWindow.GetComponent<CanvasGroup>().alpha = 1;
        _baseWindow.DisplayUpgrades(upgrades);
        _openMenu = _baseWindow;
        HideCompass();
    }

    public void OpenAbilityWheel(List<IDefenseAbility> abilities)
    {
        _defenseUI.alpha = 1;
        //_abilityWheel.DisplayAbilities(abilities);
    }

    public void CloseUI()
    {
        _player.inUI = false;
        _upgradeNode = null;
        _openMenu = null;
        ShowCompass();
    }

    private void OnLeft()
    {
        if (_player.currentState.ReturnStateName() != PlayerBaseState.PlayerState.UI) return;
        _openMenu.Left();
    }
    
    private void OnRight()
    {
        if (_player.currentState.ReturnStateName() != PlayerBaseState.PlayerState.UI) return;
        _openMenu.Right();
    }

    private void OnUp()
    {
        if (_player.currentState.ReturnStateName() != PlayerBaseState.PlayerState.UI) return;
        _openMenu.Up();
    }

    private void OnDown()
    {
        if (_player.currentState.ReturnStateName() != PlayerBaseState.PlayerState.UI) return;
        _openMenu.Down();
    }

    private void OnConfirm()
    {
        if (_player.currentState.ReturnStateName() != PlayerBaseState.PlayerState.UI) return;
        // _player.inUI = false; maybe not
        _openMenu.Confirm();
    }

    public void OnCancel()
    {
        if (_player.currentState.ReturnStateName() != PlayerBaseState.PlayerState.UI) return;
        _openMenu.Cancel();
        CloseUI();
    }

    public int SelectAbility()
    {
        return _abilityWheel.SelectAbility();
    }

    public void HideAbilities()
    {
        _defenseUI.alpha = 0;
    }

    public void HideDefenseHints()
    {
        _defenseHints.alpha = 0;
    }

    public void ShowDefenseHints()
    {
        _defenseHints.alpha = 1;
    }

    private void OnMove(InputValue inputValue)
    {
        if (_player.currentState.ReturnStateName() != PlayerBaseState.PlayerState.Defense) return;
        Vector2 MoveDelta = inputValue.Get<Vector2>();
        Vector3 direction = new Vector3(MoveDelta.x, 0, MoveDelta.y).normalized;
        _abilityWheel.Direction(direction);
    }

    private void ShowUpgradePopup()
    {
        DOTween.Kill(_upgradeInfo);
        _upgradeInfo.DOAnchorPosY(_popupPosition.anchoredPosition.y, 1);
        if (_upgradeCor != null) StopCoroutine(_upgradeCor);
        _upgradeCor = StartCoroutine(HideUpgradePopup());
    }

    private IEnumerator HideUpgradePopup()
    {
        yield return new WaitForSeconds(_upgradeInfoTime + 1);
        DOTween.Kill(_upgradeInfo);
        _upgradeInfo.DOAnchorPosY(_popupPrevPosition.anchoredPosition.y, 1);
    }

    public void ShowDisconnectedInfo()
    {
        DOTween.Kill(_diconnectedInfo);
        _diconnectedInfo.DOAnchorPosY(_popupPosition.anchoredPosition.y, 1);
    }

    public void HideDisconnectedInfo()
    {
        DOTween.Kill(_diconnectedInfo);
        _diconnectedInfo.DOAnchorPosY(_popupPrevPosition.anchoredPosition.y, 1);
    }

    private void ShowCompass()
    {
        _compass.alpha = 1;
    }

    private void HideCompass()
    {
        _compass.alpha = 0;
    }
}
