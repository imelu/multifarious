using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using DG.Tweening;
using System;


public class UpgradeManager : MonoBehaviour
{
    private BaseManager _baseManager;
    public BaseManager baseManager { get { return _baseManager; } }

    private Dictionary<Upgrade, UpgradeNode> _upgradesDict = new Dictionary<Upgrade, UpgradeNode>();
    public Dictionary<Upgrade, UpgradeNode> upgradesDict { get { return _upgradesDict; } }

    private List<Upgrade> _upgrades = new List<Upgrade>();
    public List<Upgrade> upgrades { get { return _upgrades; } }

    private void Start()
    {
        _baseManager = GetComponent<BaseManager>();
    }

    public void UpgradeSelected(Upgrade upgrade, UpgradeNode node)
    {
        if(!_upgradesDict.ContainsKey(upgrade)) _upgradesDict.Add(upgrade, node);
        _upgrades.Add(upgrade);
    }

    public void UpgradeDeselected(Upgrade upgrade)
    {
        _upgrades.Remove(upgrade);
    }


    // Todo call this after selecting the upgrades
    public void ActivateUpgrade(Upgrade upgrade, int playerIndex)
    {
        _upgradesDict.TryGetValue(upgrade, out UpgradeNode node);
        if (node != null) node.EnableUpgrade(upgrade.key.index, _baseManager.players[playerIndex]);
    }

    public void DeactivateUpgrade(Upgrade upgrade, int playerIndex)
    {
        _upgradesDict.TryGetValue(upgrade, out UpgradeNode node);
        if (node != null) node.DisableUpgrade(upgrade.key.index, _baseManager.players[playerIndex]);
    }
}
