using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerUpgradeSelectionCard : MonoBehaviour
{
    private AbilityTileUI[,] _abilityTiles = new AbilityTileUI[3,4];
    public AbilityTileUI[,] abilityTiles { get { return _abilityTiles; } }

    [SerializeField] private bool _isPlayerOne;

    void Start()
    {
        for (int i = 0; i < _abilityTiles.GetLength(1); i++)
        {
            for(int j = 0; j < _abilityTiles.GetLength(0); j++)
            {
                _abilityTiles[j, i] = transform.GetChild(i * _abilityTiles.GetLength(0) + j).GetComponent<AbilityTileUI>();
                //Debug.Log(_abilityTiles[i, j].gameObject.name);
            }
        }
    }

    public void DisplayUpgrades(List<Upgrade> upgrades)
    {
        if (upgrades.Count == 0)
        {
            foreach (AbilityTileUI abilityTile in _abilityTiles)
            {
                abilityTile.state = AbilityTileState.nonSelectable;
            }
        }
        else
        {
            foreach (AbilityTileUI abilityTile in _abilityTiles)
            {
                if (upgrades.Contains(abilityTile.upgrade))
                {
                    abilityTile.upgradeStackNumber = CheckUpgradeAmount(abilityTile.upgrade, upgrades);
                    if (abilityTile.upgrade.selectedP1 > 0 && _isPlayerOne)
                    {
                        abilityTile.state = AbilityTileState.selected;
                        abilityTile.SetStack(abilityTile.upgrade.selectedP1);
                    }
                    else if (abilityTile.upgrade.selectedP2 > 0 && !_isPlayerOne)
                    {
                        abilityTile.state = AbilityTileState.selected;
                        abilityTile.SetStack(abilityTile.upgrade.selectedP2);
                    }
                    else
                    {
                        abilityTile.state = AbilityTileState.selectable;
                        abilityTile.SetStack(0);
                    }
                }
                else
                {
                    abilityTile.upgradeStackNumber = 0;
                    abilityTile.SetStack(0);
                    abilityTile.state = AbilityTileState.nonSelectable;
                }
            }
        }
    }

    public AbilityTileUI HoverTile(int xIndex, int yIndex, bool state)
    {
        if (state)
        {
            _abilityTiles[xIndex, yIndex].Hover();
            return _abilityTiles[xIndex, yIndex];
        } 
        else
        {
            _abilityTiles[xIndex, yIndex].ReleaseHover();
            return null;
        }
    }

    private int CheckUpgradeAmount(Upgrade checkUpgrade, List<Upgrade> upgrades)
    {
        int amount = 0;

        foreach(Upgrade upgrade in upgrades)
        {
            if (upgrade == checkUpgrade) amount++;
        }

        return amount;
    }
}
