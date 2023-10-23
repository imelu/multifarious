using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectParticle : MonoBehaviour
{
    private BaseTexManager _baseTexManager;
    public BaseTexManager baseTexManager { set { _baseTexManager = value; } get { return _baseTexManager; } }
    private List<Vector2> _followPath;
    private bool _isMoving;
    private Vector3 _nextPos;
    private Vector3 _direction;
    [SerializeField] private float _speed;
    private ParticleSystem.MainModule _ParticleSystem;

    public INode nodeToConnect;

    [Header("Growing Plants")]
    [SerializeField] private List<GameObject> _growingPlants = new List<GameObject>();
    [SerializeField, Range(0, 1f)] private float _spawnChancePerBezierPoint;
    [SerializeField] private int _spawnAttempts;
    [SerializeField] private float _minSize;
    [SerializeField] private float _maxSize;
    [SerializeField] private LayerMask _floorMask;
    private Transform _growingPlantsParent;
    public Transform growingPlantsParent { get { return _growingPlantsParent; } set { _growingPlantsParent = value; } }

    [Header("Recolor Environment")]
    [SerializeField] private LayerMask _environmentMask;
    [SerializeField] private float _raycastDistance;

    [Header("Smoke Particles")]
    [SerializeField] private Transform _smokeParticles;


    private int _pathPositions;

    [Header("Sound")]
    [SerializeField] private EventReference _build;
    private EventInstance _buildInstance;
    [SerializeField] private EventReference _done;

    private void Start()
    {
       // _ParticleSystem = GetComponent<ParticleSystem>().main;
        nodeToConnect.connectParticle = this;
        ChangeSmokeHeight();

        _buildInstance = RuntimeManager.CreateInstance(_build);
        _buildInstance.set3DAttributes(RuntimeUtils.To3DAttributes(_smokeParticles));
        RuntimeManager.AttachInstanceToGameObject(_buildInstance, _smokeParticles, false);
        _buildInstance.start();
    }

    private void OnDisable()
    {
        _buildInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    void Update()
    {
        if (_isMoving) Move();
        ChangeSmokeHeight();
    }

    private void Move()
    {
        _direction = (_nextPos - transform.position).normalized;
        transform.Translate(_direction * _speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, _nextPos) < 0.2f)
        {
            GetNextPosition();
        }
    }

    public void FollowPath(List<Vector2> path, float SizeMod)
    {
        

        _ParticleSystem = GetComponent<ParticleSystem>().main;
        _ParticleSystem.startSize = new ParticleSystem.MinMaxCurve(_ParticleSystem.startSize.constantMin * SizeMod, _ParticleSystem.startSize.constantMax * SizeMod);
        _followPath = path;
        _pathPositions = _followPath.Count;
        GetNextPosition();
        _isMoving = true;
    }

    private void GetNextPosition()
    {
        if(_followPath.Count < _pathPositions - 1) SpawnPlants(_nextPos);
        if(_followPath.Count >= 2) ChangeEnvironmentColor(new Vector2(_nextPos.x, _nextPos.z), _followPath[_followPath.Count - 1], _followPath[_followPath.Count - 2]);
        if ( _followPath.Count > 0)
        {
            Vector2 pos = _followPath[_followPath.Count - 1];
            _nextPos = new Vector3(pos.x, transform.position.y, pos.y);
            _followPath.RemoveAt(_followPath.Count - 1);
        }
        else
        {
            DestinationReached();
        }
    }

    private void DestinationReached()
    {
        // call copy texture on baseTexManager and give this object as parameter so it can be destroyed when tex is copied
        _isMoving = false;
        //StartCoroutine(DestReachedDelay());
        //_smokeParticles.GetComponent<ParticleSystem>().Stop();
        _smokeParticles.GetComponent<StopAndDestroySelf>().enabled = true;
        _smokeParticles.parent = null;

        _buildInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        RuntimeManager.PlayOneShotAttached(_done, _smokeParticles.gameObject);
    }

    public void NodeConnected()
    {
        _buildInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        StartCoroutine(DestReachedDelay());
    }

    private IEnumerator DestReachedDelay()
    {
        yield return new WaitForSeconds(0.8f);
        _baseTexManager.ConnectionCompleted(gameObject);
    }

    private void SpawnPlants(Vector3 pos)
    {
        RaycastHit hit;
        for (int i = 0; i < _spawnAttempts; i++)
        {
            if(Random.Range(0,1f) < _spawnChancePerBezierPoint)
            {
                Vector2 randomPos = Random.insideUnitCircle * 3 + new Vector2(pos.x, pos.z); //
                Vector3 rayCastPoint = new Vector3(randomPos.x, 150, randomPos.y);
                if (Physics.Raycast(rayCastPoint, Vector3.down, out hit, 300, _floorMask))
                {
                    GameObject plant = Instantiate(_growingPlants[Random.Range(0, _growingPlants.Count)], hit.point, Quaternion.Euler(0, Random.Range(0, 360), 0), _growingPlantsParent);
                    plant.transform.localScale = Vector3.one * Random.Range(_minSize, _maxSize);
                }
            }
        }
    }

    private void ChangeEnvironmentColor(Vector2 prevPos, Vector2 pos, Vector2 nextPos)
    {
        Vector2 dir1 = (prevPos - pos).normalized;
        Vector2 dir2 = (nextPos - pos).normalized;

        Vector2 dirBetween = (dir1 + dir2).normalized;

        Vector3 raycastDir1 = new Vector3(dirBetween.x, 0, dirBetween.y);
        Vector3 raycastDir2 = Quaternion.Euler(0, 180, 0) * raycastDir1;

        Vector3 raycastPoint = new Vector3(pos.x, 150, pos.y);

        RaycastHit hit;
        if (Physics.Raycast(raycastPoint, Vector3.down, out hit, 300, _floorMask))
        {
            RaycastHit hit2;
            ChangeColor changeColor;
            if (Physics.Raycast(hit.point + new Vector3(0,1,0), raycastDir1, out hit2, _raycastDistance, _environmentMask))
            {
                changeColor = hit2.collider.GetComponent<ChangeColor>();
                if (changeColor != null) changeColor.ChangeColorToBlack();
                else Debug.Log("missing color change script?");
            }
            if (Physics.Raycast(hit.point + new Vector3(0, 1, 0), raycastDir2, out hit2, _raycastDistance, _environmentMask))
            {
                changeColor = hit2.collider.GetComponent<ChangeColor>();
                if (changeColor != null) changeColor.ChangeColorToBlack();
                else Debug.Log("missing color change script?");
            }
        }
    }

    private void ChangeSmokeHeight()
    {
        if (_smokeParticles == null) return;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, 300, _floorMask))
        {
            _smokeParticles.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
        }
    }
}
