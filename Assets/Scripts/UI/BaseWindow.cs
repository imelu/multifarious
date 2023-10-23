using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using FMODUnity;
using System.Linq;
using DG.Tweening.Core.Easing;
using System.Collections.Specialized;

public class BaseWindow : MonoBehaviour, IMenu
{
    [SerializeField] private PlayerUIManager _playerUI;
    private UpgradeManager _upgradeManager;
    private ResourceManager _resourceManager;

    [Header("Resources")]
    [SerializeField] private ResourceUI _totalResources;
    [SerializeField] private ResourceUI _remainingResources;
    [SerializeField] private ResourceUI _baseCost;
    [SerializeField] private ResourceUI _upgradesCost;


    //[SerializeField] private Transform _upgradeListParent;
    //rivate List<UpgradeSelectionCard> _upgradeList = new List<UpgradeSelectionCard>();

    [SerializeField] private PlayerUpgradeSelectionCard[] _upgradeSelection = new PlayerUpgradeSelectionCard[2];
    private int _xIndex = 0;
    private int _yIndex = 0;

    private int _selectedIndex;

    private int _playerHoverIndex = 0;

    private int _upgradeCount;

    private List<Upgrade>[] _activateUpgrades = new List<Upgrade>[2];
    private List<Upgrade>[] _deactivateUpgrades = new List<Upgrade>[2];

    private ResourceManager.ResourceCount[] _currentCostDifference = new ResourceManager.ResourceCount[3];

    #region Upgrade Preview
    [Header("Upgrade Preview")]
    [SerializeField] private ResourceUI _upgradeCost;
    [SerializeField] private Image _upgradeSprite;
    private Color _upgradeSpriteColor;
    [SerializeField] private TMP_Text _upgradeDescription;
    private AbilityTileUI _hoveredTile;
    #endregion

    [Header("Confirm Button")]
    [SerializeField] private Image _confirmButton;
    [SerializeField] private Color _selectableColor;
    [SerializeField] private Color _nonSelectableColor;
    private Image _hoverConfirmButton;

    private bool _currentTutorialStep;
    private TutorialAction _action;

    private void Start()
    {
        //_upgradeList = _upgradeListParent.GetComponentsInChildren<UpgradeSelectionCard>().ToList<UpgradeSelectionCard>();
        _upgradeManager = GlobalGameManager.Instance.baseTexManager.GetComponent<UpgradeManager>();
        _resourceManager = GlobalGameManager.Instance.baseTexManager.GetComponent<ResourceManager>();
        _resourceManager.OnResourceChange += OnResourcesChanged;
        _hoverConfirmButton = _confirmButton.transform.GetChild(0).GetComponent<Image>();
        _hoverConfirmButton.enabled = false;
        _confirmButton.color = _nonSelectableColor;
        ResetCurrentCostDifference();
        _upgradeSpriteColor = _upgradeSprite.color;
    }

    private void OnDisable()
    {
        // eventunsub
        //_resourceManager.OnResourceChange -= OnResourcesChanged;
    }

    private void OnResourcesChanged()
    {
        //if (GetComponent<CanvasGroup>().alpha <= 0) return;
        ResourceManager.ResourceCount[] remResourcesAfterBase = new ResourceManager.ResourceCount[3];
        Array.Copy(_resourceManager.remainingResources, remResourcesAfterBase, 3);
        for (int i = 0; i < 3; i++)
        {
            remResourcesAfterBase[i].count -= _resourceManager.baseCost[i].count;
        }
        //_remainingResources.SetResourceUIs(remResourcesAfterBase, _resourceManager.totalResources);
        UpdateRemainingResourceDisplay();
        _baseCost.SetResourceUIs(_resourceManager.baseCost, _resourceManager.totalResources, true);

        ResourceManager.ResourceCount[] currentCost = new ResourceManager.ResourceCount[3];
        Array.Copy(_resourceManager.totalResources, currentCost, 3);
        for (int i = 0; i < 3; i++)
        {
            currentCost[i].count -= remResourcesAfterBase[i].count;
        }

        _totalResources.SetResourceUIs(currentCost, _currentCostDifference, _resourceManager.totalResources, true);

        ResourceManager.ResourceCount[] upgradeCost = new ResourceManager.ResourceCount[3];
        Array.Copy(currentCost, upgradeCost, 3);
        for (int i = 0; i < 3; i++)
        {
            upgradeCost[i].count -= _resourceManager.baseCost[i].count;
        }
        _upgradesCost.SetResourceUIs(upgradeCost, _currentCostDifference, _resourceManager.totalResources, false);
    }

    public void DisplayUpgrades(List<Upgrade> upgrades)
    {
        RuntimeManager.PlayOneShot(_playerUI.enter);

        _upgradeCount = upgrades.Count;
        _xIndex = 0;
        _yIndex = 0;
        _playerHoverIndex = 0;
        _hoverConfirmButton.enabled = false;
        _confirmButton.color = _nonSelectableColor;
        foreach (PlayerUpgradeSelectionCard upgradeSelection in _upgradeSelection)
        {
            upgradeSelection.DisplayUpgrades(upgrades);
        }
        /*for (int i = 0; i < upgrades.Count; i++)
        {
            _upgradeList[i].DisplayUpgrade(upgrades[i]);
            if (!_upgradeList[i].gameObject.activeInHierarchy) _upgradeList[i].gameObject.SetActive(true);
        }
        for(int i = upgrades.Count; i < _upgradeList.Count; i++)
        {
            if (_upgradeList[i].gameObject.activeInHierarchy) _upgradeList[i].gameObject.SetActive(false);
        }*/
        ResetCurrentCostDifference();
        OnResourcesChanged();
        GenerateUpgradeLists();
        HoverSelection();
    }

    private void GenerateUpgradeLists()
    {
        _activateUpgrades[0] = new List<Upgrade>();
        _activateUpgrades[1] = new List<Upgrade>();
        _deactivateUpgrades[0] = new List<Upgrade>();
        _deactivateUpgrades[1] = new List<Upgrade>();
    }

    private void ReleaseHover()
    {
        //if (_upgradeCount == 0) return;
        //_upgradeList[_hoverIndex].HoverRelease();

        if (_yIndex < 4)
        {
            _upgradeSelection[_playerHoverIndex].HoverTile(_xIndex - 3 * _playerHoverIndex, _yIndex, false);
        } 
        else if(_yIndex == 4)
        {
            // release confirm button
            _hoverConfirmButton.enabled = false;
        }
    }

    private void HoverSelection()
    {
        //if (_upgradeCount == 0) return;
        //_upgradeList[_hoverIndex].Hovered(_playerHoverIndex);

        //_upgradeCost.SetResourceTexts(_upgradeList[_hoverIndex].upgrade.resourceCost);
        //_upgradeName.text = _upgradeList[_hoverIndex].upgrade.name;
        //_upgradeDescription.text = _upgradeList[_hoverIndex].upgrade.description;

        if (_yIndex < 4)
        {
            _hoveredTile = _upgradeSelection[_playerHoverIndex].HoverTile(_xIndex - 3 * _playerHoverIndex, _yIndex, true);
        }
        else if (_yIndex == 4)
        {
            // hover confirm button
            _hoverConfirmButton.enabled = true;
            _hoveredTile = null;
        }

        DisplayPreview();
    }

    private void SwitchPlayer()
    {
        ReleaseHover();
        _playerHoverIndex = _playerHoverIndex == 0 ? 1 : 0;
        HoverSelection();
    }

    public void Left()
    {
        //SwitchPlayer();
        ReleaseHover();

        _xIndex--;
        if (_xIndex < 0) _xIndex = 5;
        _playerHoverIndex = _xIndex <= 2 ? 0 : 1;
        HoverSelection();

        RuntimeManager.PlayOneShot(_playerUI.navigate);
    }

    public void Right()
    {
        //SwitchPlayer();
        ReleaseHover();

        _xIndex++;
        if (_xIndex > 5) _xIndex = 0;
        _playerHoverIndex = _xIndex <= 2 ? 0 : 1;
        HoverSelection();

        RuntimeManager.PlayOneShot(_playerUI.navigate);
    }

    public void Up()
    {
        //if (_upgradeList.Count <= 0) return;
        ReleaseHover();
        _yIndex--;
        if(_yIndex < 0) _yIndex = 4;
        HoverSelection();

        RuntimeManager.PlayOneShot(_playerUI.navigate);
    }

    public void Down()
    {
        //if (_upgradeList.Count <= 0) return;
        ReleaseHover();
        _yIndex++;
        if (_yIndex > 4) _yIndex = 0;
        HoverSelection();

        RuntimeManager.PlayOneShot(_playerUI.navigate);
    }

    public void Confirm()
    {
        if (_upgradeCount == 0) return;
        if (_yIndex == 4 && (_activateUpgrades[0].Count > 0 || _activateUpgrades[1].Count > 0 || _deactivateUpgrades[0].Count > 0 || _deactivateUpgrades[1].Count > 0))
        {
            for (int i = 0; i < 3; i++)
            {
                if (_resourceManager.remainingResources[i].count < _currentCostDifference[i].count)
                {
                    Debug.Log("not enough resources for these changes");
                    return;
                }
            }
            GetComponent<CanvasGroup>().alpha = 0;
            _upgradeManager.baseManager.baseUIOpen = false;
            ApplyChanges();
            _playerUI.CloseUI();
            _upgradeManager.baseManager.baseUIManager.DisableBarMasks();

            //tutorial
            if (_currentTutorialStep && (_activateUpgrades[0].Count > 0 || _activateUpgrades[1].Count > 0))
            {
                TutorialStepComplete();
            }

            RuntimeManager.PlayOneShot(_playerUI.confirm);
            return;
        } else if(_yIndex == 4)
        {
            return;
        }

        if (_hoveredTile.state == AbilityTileState.nonSelectable) return;

        if(_hoveredTile.state == AbilityTileState.selectable)
        {
            BuyUpgrade();
        }
        else
        {
            if(_hoveredTile.selectedStackNumber < _hoveredTile.upgradeStackNumber && _hoveredTile.upgrade.isStackable)
            {
                BuyUpgrade();
            }
            else
            {
                SellUpgrade();
                RuntimeManager.PlayOneShot(_playerUI.choose);
            }
        }

        

        OnResourcesChanged();
        if (_activateUpgrades[0].Count > 0 || _activateUpgrades[1].Count > 0 || _deactivateUpgrades[0].Count > 0 || _deactivateUpgrades[1].Count > 0) _confirmButton.color = _selectableColor;
        else _confirmButton.color = _nonSelectableColor;
    }

    private void BuyUpgrade()
    {
        if (!_resourceManager.everythingIsFree)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_resourceManager.remainingResources[i].count - _hoveredTile.upgrade.resourceCost[i].count - _currentCostDifference[i].count - _resourceManager.baseCost[i].count < 0)
                {
                    Debug.Log("not enough resources for these changes");
                    SellUpgrade();
                    _hoveredTile.SelectionFailed();
                    RuntimeManager.PlayOneShot(_playerUI.cancel);
                    return;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                _currentCostDifference[i].count += _hoveredTile.upgrade.resourceCost[i].count;
            }
            UpdateRemainingResourceDisplay();
        }
        _hoveredTile.SetStack(_hoveredTile.selectedStackNumber + 1);

        RuntimeManager.PlayOneShot(_playerUI.choose);

        if (_deactivateUpgrades[_playerHoverIndex].Contains(_hoveredTile.upgrade)) _deactivateUpgrades[_playerHoverIndex].Remove(_hoveredTile.upgrade);
        else _activateUpgrades[_playerHoverIndex].Add(_hoveredTile.upgrade);
        _hoveredTile.state = AbilityTileState.selected;
        _upgradeSelection[_playerHoverIndex].HoverTile(_xIndex - 3 * _playerHoverIndex, _yIndex, true);
    }

    private void SellUpgrade()
    {
        if (!_resourceManager.everythingIsFree)
        {
            for (int j = 0; j < _hoveredTile.selectedStackNumber; j++)
            {
                for (int i = 0; i < 3; i++)
                {
                    _currentCostDifference[i].count -= _hoveredTile.upgrade.resourceCost[i].count;
                }
            }
            UpdateRemainingResourceDisplay();
        }

        // remove difference between already bought and newly added stacks from activateUpgrades
        // add already bought upgrades to deactivateUpgrades
        if (_playerHoverIndex == 0)
        {
            for (int i = 0; i < _hoveredTile.upgrade.selectedP1; i++)
            {
                _deactivateUpgrades[_playerHoverIndex].Add(_hoveredTile.upgrade);
            }
            for (int i = 0; i < (_hoveredTile.selectedStackNumber - _hoveredTile.upgrade.selectedP1); i++)
            {
                _activateUpgrades[_playerHoverIndex].Remove(_hoveredTile.upgrade);
            }
        }
        else
        {
            for (int i = 0; i < _hoveredTile.upgrade.selectedP2; i++)
            {
                _deactivateUpgrades[_playerHoverIndex].Add(_hoveredTile.upgrade);
            }
            for (int i = 0; i < (_hoveredTile.selectedStackNumber - _hoveredTile.upgrade.selectedP2); i++)
            {
                _activateUpgrades[_playerHoverIndex].Remove(_hoveredTile.upgrade);
            }
        }

        _hoveredTile.SetStack(0);

        //_deactivateUpgrades[_playerHoverIndex].Add(_hoveredTile.upgrade);
        //if (_activateUpgrades[_playerHoverIndex].Contains(_hoveredTile.upgrade)) _activateUpgrades[_playerHoverIndex].Remove(_hoveredTile.upgrade);

        _hoveredTile.state = AbilityTileState.selectable;
        _upgradeSelection[_playerHoverIndex].HoverTile(_xIndex - 3 * _playerHoverIndex, _yIndex, true);
    }

    public void Cancel()
    {
        RuntimeManager.PlayOneShot(_playerUI.cancel);

        GetComponent<CanvasGroup>().alpha = 0;
        _upgradeManager.baseManager.baseUIOpen = false;
        _upgradeManager.baseManager.baseUIManager.DisableBarMasks();
    }

    private void ApplyChanges()
    {
        foreach (Upgrade upgrade in _activateUpgrades[0])
        {
            _upgradeManager.ActivateUpgrade(upgrade, 0);
            foreach(UpgradeNode node in upgrade.upgradeNodesWithThisSelected)
            {
                if (!node.isActivatedInBase)
                {
                    node.isActivatedInBase = true;
                    break;
                }
            }
            upgrade.selectedP1++;
        }
        foreach (Upgrade upgrade in _activateUpgrades[1])
        {
            _upgradeManager.ActivateUpgrade(upgrade, 1);
            foreach (UpgradeNode node in upgrade.upgradeNodesWithThisSelected)
            {
                if (!node.isActivatedInBase)
                {
                    node.isActivatedInBase = true;
                    break;
                }
            }
            upgrade.selectedP2++;
        }
        foreach (Upgrade upgrade in _deactivateUpgrades[0])
        {
            _upgradeManager.DeactivateUpgrade(upgrade, 0);
            foreach (UpgradeNode node in upgrade.upgradeNodesWithThisSelected)
            {
                if (node.isActivatedInBase)
                {
                    node.isActivatedInBase = false;
                    break;
                }
            }
            upgrade.selectedP1--;
        }
        foreach (Upgrade upgrade in _deactivateUpgrades[1])
        {
            _upgradeManager.DeactivateUpgrade(upgrade, 1);
            foreach (UpgradeNode node in upgrade.upgradeNodesWithThisSelected)
            {
                if (node.isActivatedInBase)
                {
                    node.isActivatedInBase = false;
                    break;
                }
            }
            upgrade.selectedP2--;
        }

        _resourceManager.AssignResourcesToUpgrades(_currentCostDifference);
    }

    private void ResetCurrentCostDifference()
    {
        _currentCostDifference[0].type = ResourceManager.ResourceType.Water;
        _currentCostDifference[0].count = 0;
        _currentCostDifference[1].type = ResourceManager.ResourceType.Minerals;
        _currentCostDifference[1].count = 0;
        _currentCostDifference[2].type = ResourceManager.ResourceType.Energy;
        _currentCostDifference[2].count = 0;
    }

    private void UpdateRemainingResourceDisplay()
    {
        ResourceManager.ResourceCount[] remainingResourceDisplay = new ResourceManager.ResourceCount[3];

        for (int i = 0; i < 3; i++)
        {
            remainingResourceDisplay[i].type = _currentCostDifference[i].type;
            remainingResourceDisplay[i].count = _resourceManager.remainingResources[i].count - _currentCostDifference[i].count - _resourceManager.baseCost[i].count;
        }

        _remainingResources.SetResourceUIs(remainingResourceDisplay, _resourceManager.totalResources, false);
    }

    private void DisplayPreview()
    {
        if(_hoveredTile == null)
        {
            _upgradeDescription.text = "";
            _upgradeSprite.color = Color.clear;
            _upgradeCost.SetResourceUIs(new ResourceManager.ResourceCount[3], _resourceManager.totalResources, false);
        }
        else if(_hoveredTile.state == AbilityTileState.nonSelectable)
        {
            _upgradeDescription.text = "";
            _upgradeSprite.color = Color.clear;
            _upgradeCost.SetResourceUIs(new ResourceManager.ResourceCount[3], _resourceManager.totalResources, false);
        }
        else
        {
            _upgradeSprite.color = _upgradeSpriteColor;
            _upgradeSprite.sprite = _hoveredTile.upgrade.uiSprite45Deg;
            _upgradeDescription.text = _hoveredTile.upgrade.description;
            _upgradeCost.SetResourceUIs(_hoveredTile.upgrade.resourceCost, _resourceManager.totalResources, false);
        }
    }

    public void ActivateTutorialMode(TutorialAction action)
    {
        _currentTutorialStep = true;
        _action = action;
    }

    private void TutorialStepComplete()
    {
        foreach (PlayerStateManager player in GlobalGameManager.Instance.baseManager.players)
        {
            player.tutorialManager.ActionExecuted(_action);
        }
        _currentTutorialStep = false;
    }
}
