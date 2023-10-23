using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UpgradeScriptTemplate : MonoBehaviour, IUpgrade
{
    [SerializeField] private Sprite _activeAbilitySprite;
    [SerializeField] private GameObject _activeAbilityPrefab;

    public void EnableUpgrade(int index, PlayerStateManager player)
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

    public void DisableUpgrade(int index, PlayerStateManager player)
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
