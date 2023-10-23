using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using UnityEngine;

public class CompassController : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private Transform _cam;
    private Transform _base;

    [SerializeField] private RectTransform _compass;

    [SerializeField] private GameObject _enemyNeedle;
    [SerializeField] private Transform _enemyNeedleParent;

    struct EnemyOnCompass
    {
        public Transform enemy;
        public RectTransform needle;

        public EnemyOnCompass(Transform enemy, RectTransform needle)
        {
            this.enemy = enemy;
            this.needle = needle;
        }
    }

    private List<EnemyOnCompass> _enemiesOnCompass = new List<EnemyOnCompass>();

    private Transform _tutorialObject;
    [SerializeField] private RectTransform _tutorialNeedle;

    private void Start()
    {
        _base = GlobalGameManager.Instance.baseTexManager.transform;
    }

    private void Update()
    {
        UpdateCompass(_compass, _base);
        UpdateEnemyCompasses();
        UpdateTutorialCompass();
    }

    public void CreateEnemyCompass(Transform enemy)
    {
        GameObject needle = Instantiate(_enemyNeedle, _enemyNeedleParent);
        RectTransform needleUI = needle.GetComponent<RectTransform>();
        _enemiesOnCompass.Add(new EnemyOnCompass(enemy, needleUI));
    }

    private void UpdateEnemyCompasses()
    {
        for(int i = _enemiesOnCompass.Count-1; i >= 0; i--)
        {
            if (_enemiesOnCompass[i].enemy == null)
            {
                Destroy(_enemiesOnCompass[i].needle.gameObject);
                _enemiesOnCompass.Remove(_enemiesOnCompass[i]);
            } 
            else
            {
                UpdateCompass(_enemiesOnCompass[i].needle, _enemiesOnCompass[i].enemy);
            }
        }
    }

    private void UpdateCompass(RectTransform needle, Transform target)
    {
        Vector3 dir = target.position - _player.position;
        float angle = Mathf.Atan2(dir.x, dir.z);
        Vector3 _compassRot = new Vector3(0, 0, -((angle * 180 / Mathf.PI) - _cam.eulerAngles.y));
        needle.localRotation = Quaternion.Euler(_compassRot);
    }

    public void ActivateTutorialNeedle(Transform tutorialObject)
    {
        _tutorialObject = tutorialObject;
        _tutorialNeedle.gameObject.SetActive(true);
    }

    public void DeactivateTutorialNeedle()
    {
        _tutorialObject = null; ;
        _tutorialNeedle.gameObject.SetActive(false);
    }

    private void UpdateTutorialCompass()
    {
        if (_tutorialObject == null) return;
        UpdateCompass(_tutorialNeedle, _tutorialObject);
    }
}
