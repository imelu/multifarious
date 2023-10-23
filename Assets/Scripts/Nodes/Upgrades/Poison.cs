using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum PoisonType
{
    spores,
    trail,
    toxic
}

public class Poison : MonoBehaviour, IUpgrade
{
    [SerializeField] private Sprite _poisonSporesSprite;
    [SerializeField] private GameObject _ExplosionPrefab;

    // TODO cleanup
    private int _toxicMycelCount;

    private UpgradeNode _node;
    private void Start()
    {
        _node = GetComponent<UpgradeNode>();

        //EnableUpgrade(1, GlobalGameManager.Instance.baseManager.players[0]);
    }

    public void EnableUpgrade(int index, PlayerStateManager player)
    {
        Debug.Log("Enable Poison " + index + " for player " + (player.isPlayerOne ? 1 : 2));
        switch (index-1)
        {
            case 0:
                PoisonSpores poisonSpores = player.gameObject.AddComponent<PoisonSpores>();
                player.defenseAbilities.Add(poisonSpores);
                poisonSpores.sprite = _poisonSporesSprite;
                poisonSpores.ExplosionPrefab = _ExplosionPrefab;
                poisonSpores.dps = _node.upgrades[index - 1].damage;
                poisonSpores.posionDuration = _node.upgrades[index - 1].duration;

                // TODO remove this and add to tutorial
                GlobalGameManager.Instance.baseTexManager.GetComponent<EnemySpawner>().StartFirstWave();
                break;

            case 1:
                BaseManager baseManager = GlobalGameManager.Instance.baseTexManager.GetComponent<BaseManager>();
                baseManager.toxicMycelium = true;
                baseManager.toxicDOT += _node.upgrades[index - 1].damage;
                baseManager.toxicDuration = _node.upgrades[index - 1].duration;
                _toxicMycelCount++;

                // TODO remove this and add to tutorial
                GlobalGameManager.Instance.baseTexManager.GetComponent<EnemySpawner>().StartFirstWave();
                break;

            case 2:
                // players passively leave a poison trail
                player.poisonTrail.dps = _node.upgrades[index - 1].damage;
                player.poisonTrail.duration = _node.upgrades[index - 1].duration;
                player.poisonTrailParent.SetActive(true);

                // TODO remove this and add to tutorial
                GlobalGameManager.Instance.baseTexManager.GetComponent<EnemySpawner>().StartFirstWave();
                break;
        }
    }

    public void DisableUpgrade(int index, PlayerStateManager player)
    {
        Debug.Log("Disable Poison " + index + " for player " + (player.isPlayerOne ? 1 : 2));
        switch (index - 1)
        {
            case 0:
                player.defenseAbilities.Remove(player.GetComponent<PoisonSpores>());
                Destroy(player.GetComponent<PoisonSpores>());
                break;

            case 1:
                _toxicMycelCount--;
                BaseManager baseManager = GlobalGameManager.Instance.baseTexManager.GetComponent<BaseManager>();
                baseManager.toxicDOT -= _node.upgrades[index - 1].damage;
                if (_toxicMycelCount <= 0)
                {
                    baseManager.toxicMycelium = false;
                    baseManager.toxicDuration = 0;
                }
                break;

            case 2:
                player.poisonTrailParent.SetActive(false);
                break;
        }
    }

    public void UpdateUpgrade(int index, PlayerStateManager player)
    {
    }
}
