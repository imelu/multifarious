using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[Serializable]
public struct UpgradeKey
{
    public Upgrade.UpgradeType type;
    [Range(1, 3)] public int index;
}

[CreateAssetMenu(fileName = "Upgrade", menuName = "ScriptableObjects/Upgrade", order = 1)]
public class Upgrade : ScriptableObject
{
    public enum UpgradeType
    {
        Speed,
        Poison,
        Enlarger,
        Enhancer
    }
    public string upgradeName;

    public Sprite uiSprite;
    public Sprite uiSprite45Deg;

    [TextArea(10, 15)] public string description;

    [Range(0, 10)] public float modifier;
    public float damage;
    public float duration;

    public bool isStackable;

    public ResourceManager.ResourceCount[] resourceCost = new ResourceManager.ResourceCount[3];

    public UpgradeKey key;

    [HideInInspector] public int selectedP1 = 0;
    [HideInInspector] public int selectedP2 = 0;

    public List<UpgradeNode> upgradeNodesWithThisSelected = new List<UpgradeNode>();
}