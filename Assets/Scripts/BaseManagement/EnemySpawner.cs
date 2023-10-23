using DG.Tweening;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    private BaseUIManager _baseUI;
    private BaseManager _baseManager;
    [SerializeField] private Transform _spawnPointParent;
    [SerializeField] private Transform _enemyParent;

    private List<Vector3> _spawnPoints = new List<Vector3>();

    [SerializeField] private float _waveCooldown;
    [SerializeField] private float _enemyAmount;
    [SerializeField] private float _enemyAmountInc;

    private int _currentWave = 0;

    [SerializeField] private GameObject _EnemyPrefab;

    private Coroutine _spawnerCor;

    [SerializeField] private GameObject _enemySpawnerUI;

    [Header("Mycelium Shiver")]
    [SerializeField] private Renderer _floor;
    [SerializeField] private float _shiverDurationUp;
    [SerializeField] private float _shiverDurationDown;
    private float _initialValue;


    [SerializeField] private bool _testBarPulsate;


    private void Start()
    {
        _baseUI = GetComponent<BaseUIManager>();
        _baseManager = GetComponent<BaseManager>();
        foreach(Transform child in _spawnPointParent) _spawnPoints.Add(child.position);

        //StartFirstWave();

        _initialValue = _floor.material.GetFloat("_BgDisStrength");

        if (!_testBarPulsate) _enemySpawnerUI.SetActive(false);
        else TestPulsate();
    }

    public void StartFirstWave()
    {
        if(_enemyParent.childCount == 0 && _spawnerCor == null)
        {
            if(_enemySpawnerUI != null) _enemySpawnerUI.SetActive(true);
            _spawnerCor = StartCoroutine(SpawnCooldown(_waveCooldown));
        } 
    }

    private IEnumerator SpawnCooldown(float cooldown)
    {
        for(float t = 0; t < cooldown; t += Time.deltaTime)
        {
            if (t / cooldown >= 0.995) t = cooldown;
            if(_baseUI != null) _baseUI.UpdateEnemyTimer(t / cooldown);
            yield return null;
        }

        SpawnEnemies();

        while (_enemyParent.childCount > 0)
        {
            yield return null;
        }

        _baseUI.EnemiesCleared();
        _currentWave++;
        _spawnerCor = StartCoroutine(SpawnCooldown(_waveCooldown));
    }

    private void SpawnEnemies()
    {
        for(int i = 0; i < (int)(_enemyAmount + _enemyAmountInc*_currentWave); i++)
        {
            GameObject enemy = Instantiate(_EnemyPrefab, _spawnPoints[Random.Range(0, _spawnPoints.Count)], Quaternion.identity, _enemyParent);
            foreach(PlayerStateManager player in _baseManager.players)
            {
                player.playerUI.compassController.CreateEnemyCompass(enemy.transform);
            }
        }
        _floor.material.DOFloat(1, "_BgDisStrength", _shiverDurationUp / 2).OnComplete(() => _floor.material.DOFloat(_initialValue, "_BgDisStrength", _shiverDurationDown / 2));
    }

    private void TestPulsate()
    {
        _enemySpawnerUI.SetActive(true);

        _baseUI.UpdateEnemyTimer(1);
    }
}
