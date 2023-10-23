using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enhancer : MonoBehaviour, IUpgrade
{
    private UpgradeNode _node;
    private void Start()
    {
        _node = GetComponent<UpgradeNode>();
    }

    public void EnableUpgrade(int index, PlayerStateManager player)
    {
        switch (index - 1)
        {
            case 0:
                player.abilityPotencyMod += _node.upgrades[index - 1].modifier;
                break;

            case 1:
                player.detonateAmount += (int)_node.upgrades[index - 1].modifier;
                break;

            case 2:
                player.gooRefillOnResourceDiscover = true;
                break;
        }
    }

    public void DisableUpgrade(int index, PlayerStateManager player)
    {
        switch (index - 1)
        {
            case 0:
                player.abilityPotencyMod -= _node.upgrades[index - 1].modifier;
                break;

            case 1:
                player.detonateAmount -= (int)_node.upgrades[index - 1].modifier;
                break;

            case 2:
                player.gooRefillOnResourceDiscover = false;
                break;
        }
    }

    public void UpdateUpgrade(int index, PlayerStateManager player)
    {
        switch (index - 1)
        {
            case 0:

                break;

            case 1:

                break;

            case 2:

                break;
        }
    }
}
