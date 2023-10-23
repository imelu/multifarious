using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ResourceManager : MonoBehaviour
{
    private BaseTexManager _baseTexManager;
    private BaseManager _baseManager;

    [Serializable]
    public struct ResourceCount
    {
        public ResourceManager.ResourceType type;
        public int count;
    }

    public enum ResourceType
    {
        Water,
        Minerals,
        Energy
    }

    [Header("Resources")]

    [SerializeField] private ResourceCount[] _totalResources = new ResourceCount[3];
    public ResourceCount[] totalResources { get { return _totalResources; } }

    private ResourceCount[] _remainingResources = new ResourceCount[3];
    public ResourceCount[] remainingResources { get { return _remainingResources; } }

    private ResourceCount[] _baseCost = new ResourceCount[3];
    public ResourceCount[] baseCost { get { return _baseCost; } }

    [SerializeField] private ResourceCount[] _startBaseCost = new ResourceCount[3];

    [Header("Resource Difficulty Settings")]

    [SerializeField] private float _costToBasePercentIncreaseRatio;

    [SerializeField] private float _TypeAWeight;
    [SerializeField] private float _TypeBWeight;
    [SerializeField] private float _TypeCWeight;

    private int _missingResourcesAmount;
    public int missingResourcesAmount { get { return _missingResourcesAmount; } }

    [SerializeField] private bool _everythingIsFree;
    public bool everythingIsFree { get { return _everythingIsFree; } }

    private void Start()
    {
        _baseTexManager = GetComponent<BaseTexManager>();
        _baseManager = GetComponent<BaseManager>();
        SetupResources();
        _baseTexManager.OnAreaCalculated += OnAreaCalculated;
    }

    private void OnDisable()
    {
        OnResourceChange = null;
    }

    private void OnAreaCalculated(float percentInc)
    {
        _baseCost[0].count = _startBaseCost[0].count + Mathf.FloorToInt((float)_startBaseCost[0].count * percentInc * _costToBasePercentIncreaseRatio / _TypeAWeight);
        _baseCost[1].count = _startBaseCost[1].count + Mathf.FloorToInt((float)_startBaseCost[1].count * percentInc * _costToBasePercentIncreaseRatio / _TypeBWeight);
        _baseCost[2].count = _startBaseCost[2].count + Mathf.FloorToInt((float)_startBaseCost[2].count * percentInc * _costToBasePercentIncreaseRatio / _TypeCWeight);
        CheckSurvival();
    }

    public void AddResources(List<ResourceCount> resources)
    {
        foreach(ResourceCount resource in resources)
        {
            _totalResources[(int)resource.type].count += resource.count;
            _remainingResources[(int)resource.type].count += resource.count;
        }
        
        OnResourceChange?.Invoke();
    }

    public void RemoveResources(List<ResourceCount> resources)
    {
        foreach (ResourceCount resource in resources)
        {
            _totalResources[(int)resource.type].count -= resource.count;
            _remainingResources[(int)resource.type].count -= resource.count;
        }
        CheckSurvival();
        OnResourceChange?.Invoke();
    }

    public void AssignResourcesToUpgrades(ResourceCount[] claimedResources)
    {
        for(int i = 0; i < 3; i++)
        {
            remainingResources[i].count -= claimedResources[i].count;
        }
        CheckSurvival();
    }

    public void RefundUpgrade(Upgrade upgrade)
    {
        for (int i = 0; i < 3; i++)
        {
            remainingResources[i].count += upgrade.resourceCost[i].count;
        }
    }

    private void SetupResources() 
    {
        Array.Copy(_totalResources, _remainingResources, 3);
        Array.Copy(_startBaseCost, _baseCost, 3);
    }

    private void CheckSurvival()
    {
        int missingResources = 0;
        for (int i = 0; i < 3; i++)
        {
            if (_remainingResources[i].count < _baseCost[i].count)
            {
                missingResources += _baseCost[i].count - _remainingResources[i].count;
            }
        }
        _missingResourcesAmount = missingResources;
        if (missingResources > 0)
        {
            _baseManager.StartSurvival();
        }
        else
        {
            _baseManager.StopSurvival();
        }
    }

    public delegate void OnResourceChangeDelegate();
    public event OnResourceChangeDelegate OnResourceChange;
}
