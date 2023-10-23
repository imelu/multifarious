using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Resources;
using DG.Tweening.Core.Easing;
using FMODUnity;

public class UpgradeNode : MonoBehaviour, IInteractable, INode
{
    private BaseTexManager _baseTexManager;
    private BaseManager _baseManager;
    private UpgradeManager _upgradeManager;
    private ResourceManager _resourceManager;
    private ResourceConnectionManager _resourceConnectionManager;
    private bool _connectedToBase;
    private bool _wasDiscovered;
    public bool wasDiscovered { get { return _wasDiscovered; } }

    /*[SerializeField] private Color _notConnectedCol;
    [SerializeField] private Color _connectedCol;
    [SerializeField] private Color _selectedCol;*/

    private VineParticle _vineParticle;
    public VineParticle vineParticle { get { return _vineParticle; } set { _vineParticle = value; } }

    private ConnectParticle _connectParticle;
    public ConnectParticle connectParticle { get { return _connectParticle; } set { _connectParticle = value; } }

    private NodeType _type = NodeType.upgrade;
    public NodeType type { get { return _type; } }

    public Upgrade[] upgrades = new Upgrade[3];

    [HideInInspector] public Upgrade selectedUpgrade;
    [HideInInspector] public bool isActivatedInBase;

    private IUpgrade upgradeFunction;

    [Header("Connected Glow up")]
    [SerializeField] private List<Renderer> _visual;
    [SerializeField] private float _glowUpDuration;
    [SerializeField] private float _glowDownDuration;

    private int _enabledCounter = 0;
    public int enabledCounter { get { return _enabledCounter; } }

    private ButtonUIPopup _buttonUI;
    private List<Vector2> _paintPositions;
    public List<Vector2> paintPositions { get { return _paintPositions; } set { _paintPositions = value; } }

    [SerializeField] private EventReference _reachedInteractable;

    private bool _currentTutorialStep;
    private TutorialAction _action;
    [SerializeField] private bool _isTutorialNode;

    private void Start()
    {
        _baseTexManager = GlobalGameManager.Instance.baseTexManager;
        _baseManager = _baseTexManager.GetComponent<BaseManager>();
        _upgradeManager = _baseTexManager.GetComponent<UpgradeManager>();
        _resourceManager = _baseTexManager.GetComponent<ResourceManager>();
        _resourceConnectionManager = _baseTexManager.GetComponent<ResourceConnectionManager>();
        _buttonUI = GetComponentInChildren<ButtonUIPopup>();
        _buttonUI.isEnabled = false;

        _baseTexManager.OnConnectionsCalculated += OnConnectionsCalculated;

        upgradeFunction = GetComponent<IUpgrade>();

        foreach(Upgrade upgrade in upgrades)
        {
            upgrade.selectedP1 = 0;
            upgrade.selectedP2 = 0;
            upgrade.upgradeNodesWithThisSelected = new List<UpgradeNode>();
        }
    }

    private void OnDisable()
    {
        // eventunsub
        //_baseTexManager.OnConnectionsCalculated -= OnConnectionsCalculated;
    }

    private void OnConnectionsCalculated()
    {
        if (_connectedToBase != _baseTexManager.IsConnected(transform.position))
        {
            _connectedToBase = !_connectedToBase;
            if (_connectedToBase)
            {
                //_resourceManager.connectedResources++;
                //_visual.material.color = _connectedCol;

                foreach(Renderer rend in _visual)
                {
                    rend.material.DOFloat(1, "_UITransitionMultiplier", _glowUpDuration).OnComplete(() => rend.material.DOFloat(0, "_UITransitionMultiplier", _glowDownDuration));
                }
                

                if (_connectParticle != null)
                {
                    _connectParticle.NodeConnected();
                    _connectParticle = null;
                }
                _buttonUI.isEnabled = true;

                if (_currentTutorialStep) TutorialStepComplete();
            }
            else
            {
                //_resourceManager.connectedResources--;
                _wasDiscovered = false;
                //_visual.material.color = _notConnectedCol;
                _buttonUI.isEnabled = false;

                if (vineParticle != null) vineParticle.Die();

                RefundUpgrade();
            }
        }
        if (!_connectedToBase && _baseTexManager.IsDiscovered(transform.position) && !_wasDiscovered && (!_isTutorialNode || _currentTutorialStep))
        {
            // tell base to connect
            //Debug.Log("check if connected");
            _wasDiscovered = true;
            _resourceConnectionManager.ConnectToNode(transform.position, this);

            if (_currentTutorialStep) TutorialStepComplete();
        }
    }
    /*
    public void RefundUpgrades()
    {
        if (selectedUpgrade != null) _upgradeManager.UpgradeDeselected(selectedUpgrade);
        foreach (Upgrade upgrade in upgrades)
        {
            Debug.Log("Check Upgrade: " + upgrade.upgradeName);
            for (int i = 0; i < upgrade.selectedP1; i++)
            {
                upgradeFunction.DisableUpgrade(upgrade.key.index, _baseManager.players[0]);
                _resourceManager.RefundUpgrade(upgrade);
            }
            for (int i = 0; i < upgrade.selectedP2; i++)
            {
                Debug.Log("Disable all Upgrades: " + upgrade.upgradeName + " for player 2");
                upgradeFunction.DisableUpgrade(upgrade.key.index, _baseManager.players[1]);
                _resourceManager.RefundUpgrade(upgrade);
            }
            upgrade.selectedP1 = 0;
            upgrade.selectedP2 = 0;
        }
    }*/

    public void RefundUpgrade()
    {
        if (selectedUpgrade != null) _upgradeManager.UpgradeDeselected(selectedUpgrade);
        if (!isActivatedInBase) return;
        isActivatedInBase = false;
        foreach (Upgrade upgrade in upgrades)
        {
            Debug.Log("Check Upgrade: " + upgrade.upgradeName);
            if(upgrade.selectedP1 > 0)
            {
                if (upgrade.upgradeNodesWithThisSelected.Contains(this))
                {
                    Debug.Log("Disable Upgrade once: " + upgrade.upgradeName + " for player 1");
                    upgradeFunction.DisableUpgrade(upgrade.key.index, _baseManager.players[0]);
                    _resourceManager.RefundUpgrade(upgrade);
                    upgrade.selectedP1--;
                }
            }
            if (upgrade.selectedP2 > 0)
            {
                if (upgrade.upgradeNodesWithThisSelected.Contains(this))
                {
                    Debug.Log("Disable Upgrade once: " + upgrade.upgradeName + " for player 2");
                    upgradeFunction.DisableUpgrade(upgrade.key.index, _baseManager.players[1]);
                    _resourceManager.RefundUpgrade(upgrade);
                    upgrade.selectedP2--;
                }
            }
        }
    }

    public void Interact(PlayerStateManager player)
    {
        if(_connectedToBase) player.playerUI.OpenUpgradeWindow(upgrades, this);
    }

    public void SelectedUpgrade(int index)
    {
        if (selectedUpgrade != null) DeselectUpgrade();
        selectedUpgrade = upgrades[index];
        _upgradeManager.UpgradeSelected(selectedUpgrade, this);
        selectedUpgrade.upgradeNodesWithThisSelected.Add(this);
        if (_currentTutorialStep) TutorialStepComplete();
    }

    private void DeselectUpgrade()
    {
        _upgradeManager.UpgradeDeselected(selectedUpgrade);
        selectedUpgrade.upgradeNodesWithThisSelected.Remove(this);
        selectedUpgrade = null;
    }

    public void EnableUpgrade(int index, PlayerStateManager player)
    {
        if (_enabledCounter == 0) _resourceConnectionManager.CreateVine(this);
        _enabledCounter++;
        upgradeFunction.EnableUpgrade(index, player);
    }

    public void DisableUpgrade(int index, PlayerStateManager player)
    {
        _enabledCounter--;
        if (_enabledCounter == 0 && vineParticle != null) vineParticle.Die();
        upgradeFunction.DisableUpgrade(index, player);
    }

    public void PlayUpgradeNodeSound()
    {
        RuntimeManager.PlayOneShotAttached(_reachedInteractable, gameObject);
    }

    public void ActivateTutorialMode(TutorialAction action)
    {
        _currentTutorialStep = true;
        _action = action;
    }

    private void TutorialStepComplete()
    {
        foreach (PlayerStateManager player in _baseManager.players)
        {
            player.tutorialManager.ActionExecuted(_action);
        }
        _currentTutorialStep = false;
    }
}
