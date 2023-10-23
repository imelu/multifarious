using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Transactions;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using DG.Tweening;

public class NPCAIStateManager : MonoBehaviour
{
    [TypeDropDown(typeof(NPCBaseState))]
    [SerializeField]
    private string startStateType;
    private NPCBaseState _currentState;
    public NPCBaseState currentState { get { return _currentState; } set { _currentState = value; } }
    private NPCStateFactory _states;
    public NPCStateFactory states { get { return _states; } }

    public enum NPCType
    {
        normal
    }

    [SerializeField] private NPCType _type;
    public NPCType type { get { return _type; } }

    [SerializeField] private LayerMask _floorMask;

    [SerializeField] private Transform _visual;

    #region movement

    [Header("Brain")]
    [SerializeField] private float _speedMin;
    public float speedMin { get { return _speedMin; } }

    [SerializeField] private float _speedMax;
    public float speedMax { get { return _speedMax; } }

    [SerializeField] private float _speed;
    public float speed { get { return _speed; } }
    [SerializeField] private float _sprintSpeed;
    public float sprintSpeed { get { return _sprintSpeed; } }

    [SerializeField] private float _stopProximity;
    public float stopProximity { get { return _stopProximity; } }

    private NavMeshAgent _agent;
    public NavMeshAgent agent { get { return _agent; } }

    #endregion

    #region brain

    public enum NPCAction
    {
        walk,
        idle,
        NONE
    }

    [Header("Brain")]
    [SerializeField] private NPCActionData _actionData;
    public NPCActionData actionData { get { return _actionData; } }

    public Coroutine nextActionCoroutine;

    private NPCAction _selectedAction;
    public NPCAction selectedAction { get { return _selectedAction; } set { _selectedAction = value; } }

    #endregion

    private BaseTexManager _baseTexManager;
    public BaseTexManager baseTexManager { get { return _baseTexManager; } }

    private BaseManager _baseManager;
    public BaseManager baseManager { get { return _baseManager; } }

    private NPCHealthManager _npcHealth;
    public NPCHealthManager npcHealth { get { return _npcHealth; } }

    [SerializeField] private Animator _anim;
    public Animator anim { get { return _anim; } }

    #region eating

    [SerializeField] private bool _onlyEatConnectedBase;
    [SerializeField] private int _searchRadius;
    private Vector3 _targetFoodPos;
    public Vector3 targetFoodPos { get { return _targetFoodPos; } set { _targetFoodPos = value; } }

    [SerializeField] private float _eatingSpeed;
    public float eatingSpeed { get { return _eatingSpeed; } }

    [SerializeField] private Transform _eatingPoint;
    public Transform eatingPoint { get { return _eatingPoint; } }

    #endregion

    // Debug

    [SerializeField] private NPCBaseState.NPCStates currentStateName;

    [Header("Sound")]
    [SerializeField] private EventReference _eating;
    private EventInstance _eatingInstance;
    public EventInstance eatingInstance { get { return _eatingInstance; } }
    [SerializeField] private EventReference _death;
    public EventReference death { get { return _death; } }
    [SerializeField] private EventReference _moving;
    private EventInstance _movingInstance;
    public EventInstance movingInstance { get { return _movingInstance; } }
    [SerializeField] private EventReference _poisoned;
    private EventInstance _poisonedInstance;
    public EventInstance poisonedInstance { get { return _poisonedInstance; } }
    [SerializeField] private EventReference _decay;
    public EventReference decay { get { return _decay; } }

    private void Awake()
    {
        _states = new NPCStateFactory(this);
        _agent = GetComponent<NavMeshAgent>();        
    }

    private void Start()
    {
        _baseTexManager = GlobalGameManager.Instance.baseTexManager;
        _npcHealth = GetComponent<NPCHealthManager>();
        _baseManager = _baseTexManager.GetComponent<BaseManager>();
        Type startType = Type.GetType(startStateType);
        _currentState = (NPCBaseState)Activator.CreateInstance(startType, this, _states);
        _currentState.EnterState();


        _movingInstance = RuntimeManager.CreateInstance(_moving);
        _movingInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
        RuntimeManager.AttachInstanceToGameObject(_movingInstance, gameObject.transform, false);
        _eatingInstance = RuntimeManager.CreateInstance(_eating);
        _eatingInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
        RuntimeManager.AttachInstanceToGameObject(_eatingInstance, gameObject.transform, false);
        _poisonedInstance = RuntimeManager.CreateInstance(_poisoned);
        _poisonedInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
        RuntimeManager.AttachInstanceToGameObject(_poisonedInstance, gameObject.transform, false);
    }

    void Update()
    {
        _currentState.UpdateStates();

        if(Time.frameCount % 7 == 0)
        {
            // align enemy to ground
        }

        // Debug
        currentStateName = currentState.ReturnStateName();

        if (currentState.ReturnStateName() == NPCBaseState.NPCStates.Dead) return;
        AlignToGround();
    }

    private void FixedUpdate()
    {
        _currentState.FixedUpdateStates();
    }

    private void OnEnable()
    {
        agent.enabled = true;
        //currentState.EnterState();
    }

    private void OnDisable()
    {
        _eatingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _poisonedInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _movingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    private void OnTriggerEnter(Collider other)
    {
        _currentState.OnTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        _currentState.OnTriggerExit(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        _currentState.OnCollisionEnter(collision);
    }

    public void StartActionCooldown()
    {
        NextAction nextAction = new NextAction();
        if (actionData != null)
        {
            nextAction = actionData.GetNextAction(currentState.ReturnStateName());
        }
        if (nextAction != null) nextActionCoroutine = StartCoroutine(ActionCooldown(nextAction.actionDelay, nextAction.nextAction));
    }

    IEnumerator ActionCooldown(float delay, NPCAction action)
    {
        yield return new WaitForSecondsRealtime(delay);
        selectedAction = action;
        yield return null;
    }

    public Vector3 FindFood()
    {
        Cell[,] grid = _baseTexManager.ConnectivityGrid;

        for (int j = 0; j < grid.GetLength(1); j++)
        {
            for (int i = 0; i < grid.GetLength(0); i++)
            { 
                if (grid[i, j] == null) Debug.LogError("no cell at index of: [" + i + "/" + j + "]");
            }
        }

        //_baseTexManager.DrawGridAsTexture(grid);

        Vector2 percentPos = _baseTexManager.GetPercentPos(new Vector2(transform.position.x, -transform.position.z));

        Vector2Int gridPos = Vector2Int.zero;
        gridPos.x = (int)Mathf.Round(grid.GetLength(0) * percentPos.x);
        gridPos.y = (int)Mathf.Round(grid.GetLength(1) * (1 - percentPos.y));

        //Debug.Log("pos: "+gridPos);

        Vector2Int foodPos = new Vector2Int(-1, -1);

        for (int i = 0; i < _searchRadius; i++)
        {
            foodPos = CheckAdjacentCellsForFood(grid, gridPos, i);
            if (foodPos.x >= 0) break;
        }

        if(foodPos.x < 0)
        {
            Debug.Log("No Food found!");
        }

        //Debug.Log("food: "+foodPos);

        Vector2 worldPos = _baseTexManager.GetPosFromPercent(new Vector2((float)foodPos.x / grid.GetLength(0), (float)foodPos.y / grid.GetLength(1)));

        //Debug.Log("food in world: " + worldPos);

        selectedAction = NPCAction.walk;

        // get food y level

        return _baseTexManager.GetPosWithHeight(worldPos);
    }

    private Vector2Int CheckAdjacentCellsForFood(Cell[,] grid, Vector2Int startingPoint, int distance)
    {
        List<Vector2Int> foodLocations = new List<Vector2Int>();
        if(distance == 0)
        {
            if (grid[startingPoint.x, startingPoint.y].isConnected) return startingPoint;
        }
        for(int i = startingPoint.x - distance; i < startingPoint.x + distance; i++)
        {
            int j = Mathf.Clamp(i, 0, grid.GetLength(0) - 1);

            if(grid[j, Mathf.Clamp(startingPoint.y + distance, 0, grid.GetLength(1) - 1)] != null)
            {
                if (grid[j, Mathf.Clamp(startingPoint.y + distance, 0, grid.GetLength(1) - 1)].isConnected) foodLocations.Add(new Vector2Int(i, startingPoint.y + distance));
            }
            else
            {
                Debug.Log("no cell at index of: [" + j + "/" + Mathf.Clamp(startingPoint.y + distance, 0, grid.GetLength(1) - 1) + "]");
            }
            
            
            if (grid[j, Mathf.Clamp(startingPoint.y - distance, 0, grid.GetLength(1) - 1)].isConnected) foodLocations.Add(new Vector2Int(i, startingPoint.y - distance));
        }
        for (int i = startingPoint.y - distance + 1; i < startingPoint.y + distance - 1; i++)
        {
            int j = Mathf.Clamp(i, 0, grid.GetLength(1) - 1);
            if (grid[Mathf.Clamp(startingPoint.x + distance, 0, grid.GetLength(0) - 1), j].isConnected) foodLocations.Add(new Vector2Int(startingPoint.x + distance, i));
            if (grid[Mathf.Clamp(startingPoint.x - distance, 0, grid.GetLength(0) - 1), j].isConnected) foodLocations.Add(new Vector2Int(startingPoint.x - distance, i));
        }

        if(foodLocations.Count > 0)
        {
            return foodLocations[Random.Range(0, foodLocations.Count)];
        }
        else
        {
            return new Vector2Int(-1, -1);
        }
    }

    private void AlignToGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 5, _floorMask))
        {
            Vector3 angle = Quaternion.FromToRotation(Vector3.up, hit.normal).eulerAngles;
            Quaternion targetRot = Quaternion.Euler(new Vector3(angle.x, angle.y, angle.z));


            Vector3 up = hit.normal;
            Vector3 vel = transform.forward.normalized;
            Vector3 forward = vel - up * Vector3.Dot(vel, up);

            Quaternion newRot = Quaternion.LookRotation(forward.normalized, up);

            _visual.rotation = Quaternion.RotateTowards(_visual.rotation, newRot, 5 * Time.deltaTime);

            //DOTween.Kill(_visual);
            //_visual.DORotateQuaternion(targetRot, 0.8f);
        }
    }

    public void Die()
    {
        eatingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        movingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _currentState = _states.Dead();
        _currentState.EnterState();

        DOTween.Kill(_visual);
    }
}
