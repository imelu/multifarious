using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using FMODUnity;

[Serializable]
public class UpgradeCard
{
    public Image image;
    public TMP_Text name;
    public TMP_Text desc;
    public ResourceUI cost;
    public Image selector;
}

public class UpgradeWindow : MonoBehaviour, IMenu
{
    [SerializeField] private PlayerUIManager _playerUI;

    [SerializeField] private List<UpgradeCard> upgradeCards = new List<UpgradeCard>();

    [SerializeField] private Color _normalCol;
    [SerializeField] private Color _selectedCol;

    private int _selectedIndex;

    private int _hoverIndex = 0;

    private UpgradeNode _currentNode;

    public void DisplayUpgrades(Upgrade[] upgrades, UpgradeNode node)
    {
        RuntimeManager.PlayOneShot(_playerUI.enter);

        _hoverIndex = 0;
        _currentNode = node;
        if (node.selectedUpgrade != null) _selectedIndex = Array.IndexOf(upgrades, node.selectedUpgrade);
        else _selectedIndex = -1;

        HoveredOption(0);

        for (int i = 0; i < upgrades.Length; i++)
        {
            /*if(i == 0 && node.enabledCounter == 0) upgradeCards[i].background.color = _hoverCol;
            else if (i == _selectedIndex) upgradeCards[i].background.color = _selectedCol;
            else upgradeCards[i].background.color = _normalCol;*/

            if (i == _selectedIndex)
            {
                upgradeCards[i].selector.color = _selectedCol;
                upgradeCards[i].selector.enabled = true;
            }
            else upgradeCards[i].selector.color = _normalCol;

            upgradeCards[i].image.sprite = upgrades[i].uiSprite45Deg;
            upgradeCards[i].name.text = upgrades[i].upgradeName;
            upgradeCards[i].desc.text = upgrades[i].description;
            upgradeCards[i].cost.SetResourceUIs(upgrades[i].resourceCost, true);
        }
    }

    public void ResetOption(int index)
    {
        /*if (index == _selectedIndex) upgradeCards[index].background.color = _selectedCol;
        else upgradeCards[index].background.color = _normalCol;*/

        if(index != _selectedIndex) upgradeCards[index].selector.enabled = false;
        else upgradeCards[index].selector.color = _selectedCol;
    }

    public void HoveredOption(int index)
    {
        /*if (_currentNode.enabledCounter > 0) return;
        upgradeCards[index].background.color = _hoverCol;*/

        upgradeCards[index].selector.enabled = true;
        upgradeCards[index].selector.color = _normalCol;
    }

    public void Left()
    {

    }

    public void Right()
    {

    }

    public void Up()
    {
        ResetOption(_hoverIndex);
        if (_hoverIndex == 0) _hoverIndex = 2;
        else _hoverIndex--;
        HoveredOption(_hoverIndex);

        RuntimeManager.PlayOneShot(_playerUI.navigate);
    }

    public void Down()
    {
        ResetOption(_hoverIndex);
        if (_hoverIndex == 2) _hoverIndex = 0;
        else _hoverIndex++;
        HoveredOption(_hoverIndex);

        RuntimeManager.PlayOneShot(_playerUI.navigate);
    }

    public void Confirm()
    {
        RuntimeManager.PlayOneShot(_playerUI.confirm);

        GetComponent<CanvasGroup>().alpha = 0;
        if (_selectedIndex != _hoverIndex)
        {
            if (_selectedIndex != -1 && _currentNode.isActivatedInBase) _currentNode.RefundUpgrade();
            _playerUI.SelectUpgrade(_hoverIndex);

        }
        else
        {
            _playerUI.OnCancel();
        }
        _currentNode = null;
        foreach (var item in upgradeCards)
        {
            item.selector.enabled = false;
        }
    }
    public void Cancel()
    {
        RuntimeManager.PlayOneShot(_playerUI.cancel);

        GetComponent<CanvasGroup>().alpha = 0;
        _currentNode = null;
        foreach (var item in upgradeCards)
        {
            item.selector.enabled = false;
        }
    }
}
