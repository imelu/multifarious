using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourcePreview : MonoBehaviour, IMenu
{
    [SerializeField] private PlayerUIManager _playerUI;
    [SerializeField] private Image _resourceInfo;
    [SerializeField] private Image _resourceIcon;
    [SerializeField] private TMP_Text _resourceCount;
    [SerializeField] private TMP_Text _connectionState;
    [SerializeField] private Color _connectedTexCol;
    [SerializeField] private Color _notConnectedTexCol;
    private CanvasGroup _canvasGroup;
    private ResourceNode _currentNode;
    private void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        if (_currentNode != null)
        {
            _connectionState.text = _currentNode.connectedToBase ? "Connected" : "Not Connected";
            _connectionState.color = _currentNode.connectedToBase ? _connectedTexCol : _notConnectedTexCol;
        }
    }

    public void DisplayResource(ResourceNode node)
    {
        RuntimeManager.PlayOneShot(_playerUI.enter);

        _currentNode = node;
        _resourceInfo.sprite = node.resource.resourceInfoSprite;
        _resourceIcon.sprite = node.resource.resourceTypeSprite;
        _resourceCount.text = "+" + node.resources[0].count.ToString();
        _connectionState.text = node.connectedToBase ? "Connected" : "Not Connected";
        _connectionState.color = node.connectedToBase ? _connectedTexCol : _notConnectedTexCol;
    }

    public void Cancel()
    {
        RuntimeManager.PlayOneShot(_playerUI.confirm);

        _canvasGroup.alpha = 0;
        _currentNode = null;
    }

    public void Confirm()
    {
        RuntimeManager.PlayOneShot(_playerUI.confirm);

        _canvasGroup.alpha = 0;
        _playerUI.OnCancel();
        _currentNode = null;
    }

    public void Down()
    {
    }

    public void Left()
    {
    }

    public void Right()
    {
    }

    public void Up()
    {
    }
}
