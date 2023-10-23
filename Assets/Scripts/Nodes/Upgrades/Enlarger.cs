using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enlarger : MonoBehaviour, IUpgrade
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
                player.abilitySizeMod += _node.upgrades[index - 1].modifier;
                break;

            case 1:
                GlobalGameManager.Instance.baseTexManager.GetComponent<BaseManager>().connectParticleSizeMod += _node.upgrades[index - 1].modifier;
                break;

            case 2:
                player.explorationGooSizeMod += _node.upgrades[index - 1].modifier;
                break;
        }
    }

    public void DisableUpgrade(int index, PlayerStateManager player)
    {
        switch (index - 1)
        {
            case 0:
                player.abilitySizeMod -= _node.upgrades[index - 1].modifier;
                break;

            case 1:
                GlobalGameManager.Instance.baseTexManager.GetComponent<BaseManager>().connectParticleSizeMod -= _node.upgrades[index - 1].modifier;
                break;

            case 2:
                player.explorationGooSizeMod -= _node.upgrades[index - 1].modifier;
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
