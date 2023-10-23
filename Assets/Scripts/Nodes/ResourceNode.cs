using DG.Tweening;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ResourceNode : MonoBehaviour, INode, IInteractable
{
    private BaseTexManager _baseTexManager;
    private BaseManager _baseManager;
    private ResourceManager _resourceManager;
    private ResourceConnectionManager _resourceConnectionManager;
    private bool _connectedToBase;
    public bool connectedToBase { get { return _connectedToBase; } }
    private bool _wasDiscovered;
    public bool wasDiscovered { get { return _wasDiscovered; } }

    [SerializeField] private Color _notConnectedCol;
    [SerializeField] private Color _connectedCol;

    [SerializeField]
    private List<ResourceManager.ResourceCount> _resources = new List<ResourceManager.ResourceCount>();
    public List<ResourceManager.ResourceCount> resources { get { return _resources; } }

    private VineParticle _vineParticle;
    public VineParticle vineParticle { get { return _vineParticle; } set { _vineParticle = value; } }

    private ConnectParticle _connectParticle;
    public ConnectParticle connectParticle { get { return _connectParticle; } set { _connectParticle = value; } }
    private NodeType _type = NodeType.resource;
    public NodeType type { get { return _type; } }
    private List<Vector2> _paintPositions;
    public List<Vector2> paintPositions { get { return _paintPositions; } set { _paintPositions = value; } }

    [SerializeField] private Resource _resource;
    public Resource resource { get { return _resource; } set { _resource = value; } }

    [Header("Connected Glow up")]
    [SerializeField] private Renderer _visual;
    [SerializeField] private float _glowUpDuration;
    [SerializeField] private float _glowDownDuration;

    [SerializeField] private EventReference _reachedInteractable;

    private bool _currentTutorialStep;
    private TutorialAction _action;

    private void Start()
    {
        _baseTexManager = GlobalGameManager.Instance.baseTexManager;
        _baseManager = GlobalGameManager.Instance.baseManager;
        _resourceManager = _baseTexManager.GetComponent<ResourceManager>();
        _resourceConnectionManager = _baseTexManager.GetComponent<ResourceConnectionManager>();

        _baseTexManager.OnConnectionsCalculated += OnConnectionsCalculated;
    }

    private void OnConnectionsCalculated()
    {
        if(_connectedToBase != _baseTexManager.IsConnected(transform.position))
        {
            _connectedToBase = !_connectedToBase;
            if (_connectedToBase)
            {
                _resourceManager.AddResources(_resources);

                if (_connectParticle != null)
                {
                    _connectParticle.NodeConnected();
                    _connectParticle = null;
                }
                _visual.material.DOFloat(1, "_UITransitionMultiplier", _glowUpDuration).OnComplete(() => _visual.material.DOFloat(0, "_UITransitionMultiplier", _glowDownDuration));

                if (_currentTutorialStep) TutorialStepComplete();
            } 
            else
            {
                _resourceManager.RemoveResources(_resources);
                _wasDiscovered = false;
                if(vineParticle != null) vineParticle.Die();
                vineParticle = null;
            }
        }
        if(!_connectedToBase && _baseTexManager.IsDiscovered(transform.position) && !_wasDiscovered)
        {
            _wasDiscovered = true;
            _resourceConnectionManager.ConnectToNode(transform.position, this);

            if (_currentTutorialStep) TutorialStepComplete();
        }
    }

    public void Interact(PlayerStateManager player)
    {
        player.playerUI.OpenResourceWindow(this);
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
        foreach(PlayerStateManager player in _baseManager.players)
        {
            player.tutorialManager.ActionExecuted(_action);
        }
        _currentTutorialStep = false;
    }
}
