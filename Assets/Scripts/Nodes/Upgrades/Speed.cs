using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Speed : MonoBehaviour, IUpgrade
{
    private UpgradeNode _node;
    private void Start()
    {
        _node = GetComponent<UpgradeNode>();
    }
    public void EnableUpgrade(int index, PlayerStateManager player)
    {
        Debug.Log("Enable Speed " + index + " for player " + (player.isPlayerOne ? 1 : 2));
        switch (index-1)
        {
            case 0:
                player.speedOutsideBaseMod += _node.upgrades[index - 1].modifier;
                break;

            case 1:
                player.speedInBaseMod += _node.upgrades[index - 1].modifier;
                break;

            case 2:
                player.speedMod += _node.upgrades[index - 1].modifier;
                break;
        }
    }

    public void DisableUpgrade(int index, PlayerStateManager player)
    {
        Debug.Log("Disable Speed " + index + " for player " + (player.isPlayerOne ? 1 : 2));
        switch (index - 1)
        {
            case 0:
                player.speedOutsideBaseMod -= _node.upgrades[index - 1].modifier;
                break;

            case 1:
                player.speedInBaseMod -= _node.upgrades[index - 1].modifier;
                break;

            case 2:
                player.speedMod -= _node.upgrades[index - 1].modifier;
                break;
        }
    }

    public void UpdateUpgrade(int index, PlayerStateManager player)
    {
        Debug.Log("Update Speed " + index + " for player " + (player.isPlayerOne ? 1 : 2));
    }
}
