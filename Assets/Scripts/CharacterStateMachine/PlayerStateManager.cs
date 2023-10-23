using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Cinemachine;
using System.Linq;
using FMODUnity;
using FMOD.Studio;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class PlayerStateManager : MonoBehaviour
{
    private PlayerBaseState _currentState;
    public PlayerBaseState currentState { get { return _currentState; } set { _currentState = value; } }
    private PlayerStateFactory _states;

    private NavMeshObstacle _obstacle;

    [SerializeField] private CinemachineFreeLook _freelookCam;
    public CinemachineFreeLook freeLookCam { get { return _freelookCam; } }

    [SerializeField] private CinemachineVirtualCamera _topDownCam;
    public CinemachineVirtualCamera topDownCam { get { return _topDownCam; } }

    [SerializeField] private InputHandler _IH;
    public InputHandler IH { get { return _IH; } set { _IH = value; } }

    private PlayerInput _input;

    private PlayerUIManager _playerUI;
    public PlayerUIManager playerUI { get { return _playerUI; } }

    [SerializeField] private bool _isPlayerOne;
    public bool isPlayerOne { get { return _isPlayerOne; } set { _isPlayerOne = value; } }

    private IInteractable interactable;

    private bool _inUI;
    public bool inUI { get { return _inUI; } set { _inUI = value; } }

    private BaseManager _baseManager;

    private PlayerBaseState _statePreBlock;

    private bool _isConnected;

    #region Exploration
    [Header("Exploration")]
    [SerializeField] private GameObject _ExplorationGoo;
    private ParticleSystem.MainModule _ExplorationGooPS;
    private ParticleSystem.MinMaxCurve _gooStartLifeTime;
    private ParticleSystem.MinMaxCurve _gooStartSize;
    [SerializeField] private AnimationCurve _gooSizeDecay;
    [SerializeField] private AnimationCurve _gooLifeTimeDecay;
    [SerializeField] private float _maxTimeOutsideBase;
    private Coroutine _gooDecayCor;
    private Coroutine _disconnectedWarningCor;

    #endregion

    #region Defense
    [Header("Defense")]
    [SerializeField] private Transform _defenseDirection;
    private List<IDefenseAbility> _defenseAbilities = new List<IDefenseAbility>();
    public List<IDefenseAbility> defenseAbilities { get { return _defenseAbilities; } set { _defenseAbilities = value; } }
    private Vector3 _projectileDir;
    [SerializeField] private float _projectileRotSpeed;
    private int _selectedAbility;
    private Coroutine _defenseCameraSwing;
    private bool _inDefenseMode;
    private enum DefenseStep
    {
        NONE,
        Aim, 
        Emit,
        Steer
    }
    private DefenseStep _defenseStep;

    [SerializeField] private GameObject _attackTrail;

    #endregion

    #region Upgrades

    // poison
    [SerializeField] private PoisonTrail _poisonTrail;
    public PoisonTrail poisonTrail { get { return _poisonTrail; } set { _poisonTrail = value; } }
    [SerializeField] private GameObject _poisonTrailParent;
    public GameObject poisonTrailParent { get { return _poisonTrailParent; } }

    // speed
    private float _speedMod = 1;
    public float speedMod { get { return _speedMod; } set { _speedMod = value; } }
    private float _speedInBaseMod = 1;
    public float speedInBaseMod { get { return _speedInBaseMod; } set { _speedInBaseMod = value; } }
    private float _speedOutsideBaseMod = 1;
    public float speedOutsideBaseMod { get { return _speedOutsideBaseMod; } set { _speedOutsideBaseMod = value; } }

    // enlarger
    private float _abilitySizeMod = 1;
    public float abilitySizeMod { get { return _abilitySizeMod; } set { _abilitySizeMod = value; } }
    private float _explorationGooSizeMod = 1;
    public float explorationGooSizeMod { get { return _explorationGooSizeMod; } 
        set 
        { 
            _explorationGooSizeMod = value; 
            _ExplorationGooPS.startSize = value * _gooStartSize.constant;
            _ExplorationGooPS.startLifetime = value * _gooStartLifeTime.constant;
        } 
    }

    // enhancer
    private float _abilityPotencyMod = 1;
    public float abilityPotencyMod { get { return _abilityPotencyMod; } set { _abilityPotencyMod = value; } }
    private int _timesDetonated = 0;
    private int _detonateAmount = 1;
    public int detonateAmount { get { return _detonateAmount; } set { _detonateAmount = value; } }
    private bool _gooRefillOnResourceDiscover = false;
    public bool gooRefillOnResourceDiscover { get { return _gooRefillOnResourceDiscover; } set { _gooRefillOnResourceDiscover = value; } }

    // idk
    private float _abilityLifetimeMod = 1;
    public float abilityLifetimeMod { get { return _abilityLifetimeMod; } set { _abilityLifetimeMod = value; } }

    #endregion


    // test drawing
    [SerializeField] private bool _paintingAllowed;
    private BaseTexManager _baseTexManager;
    private bool _isExpanding;

    private bool _onBase;

    private bool _isRemoving;

    #region CharacterController

    [Header("Character Controller")]

    [SerializeField] Transform _cam;
    public Transform cam { get { return _cam; } }
    private CharacterController controller;

    private bool _sprint = false;
    private bool _movePlayer = false;
    public bool movePlayer { get { return _movePlayer; } }
    [SerializeField] private float speed = 12f;
    [SerializeField] private float speedOutsideBase = 12f;
    [SerializeField] private float sprintSpeed = 20f;
    private Vector3 direction;

    private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    private float gravity = -250f;
    private Vector3 velocity;
    private bool isGrounded;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    public bool sprint { get { return _sprint; } }

    #endregion

    #region Sound
    [Header("Sound")]
    [SerializeField] private EventReference _moving;
    private EventInstance _movingInstance;
    public EventInstance movingInstance { get { return _movingInstance; } }
    [SerializeField] private EventReference _startAttack;
    [SerializeField] private EventReference _movingAttack;
    private EventInstance _movingAttackInstance;
    public EventInstance movingAttackInstance { get { return _movingAttackInstance; } }
    [SerializeField] private EventReference _combust;
    [SerializeField] private EventReference _warning;
    [SerializeField] private EventReference _connectionFailed;
    #endregion

    private TutorialManager _tutorialManager;
    public TutorialManager tutorialManager { get { return _tutorialManager; } }
    private bool _currentTutorialStep;
    private TutorialAction _action;


    private bool _select;
    private bool _start;

    private void Awake()
    {
        _obstacle = GetComponent<NavMeshObstacle>();
        controller = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInput>();
        _playerUI = GetComponent<PlayerUIManager>();
        _tutorialManager = GetComponent<TutorialManager>();
        _states = new PlayerStateFactory(this);
        _currentState = _states.Idle();
        _currentState.EnterState();
    }

    private void Start()
    {
        //if (isPlayerOne && _input != null) _input.actions.Disable();
        _baseTexManager = GlobalGameManager.Instance.baseTexManager;
        _baseTexManager.GetComponent<ResourceConnectionManager>().OnResourceDiscovered += OnResourceDiscovered;
        _baseManager = _baseTexManager.GetComponent<BaseManager>();
        IH.horizontal = _input.actions.FindAction("Look");
        Physics.IgnoreLayerCollision(7, 8);
        //Physics.IgnoreLayerCollision(7, 11);
        //Physics.IgnoreLayerCollision(8, 11);
        _defenseAbilities = GetComponents<IDefenseAbility>().ToList<IDefenseAbility>();
        _ExplorationGooPS = _ExplorationGoo.GetComponent<ParticleSystem>().main;
        _gooStartLifeTime = _ExplorationGooPS.startLifetime;
        _gooStartSize = _ExplorationGooPS.startSize;
        CheckOnBase();

        _movingInstance = RuntimeManager.CreateInstance(_moving);
        _movingInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
        _movingAttackInstance = RuntimeManager.CreateInstance(_movingAttack);
        _movingAttackInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
    }

    private void OnEnable()
    {
        tutorialManager.StartTutorial();
    }

    void Update()
    {
        _currentState.UpdateStates();
        ApplyGravity();
        
        if (_isRemoving) _baseTexManager.RemoveOnPos(transform.position, BaseTexManager.DrawSize.large);
        if (Time.frameCount % 15 == 0 || _movePlayer) CheckOnBase();
        if (_onBase)
        {
            if (_isExpanding) _baseTexManager.DrawOnBase(transform.position, BaseTexManager.DrawSize.large); // TODO: make a state for this and only draw when movement is detected
        }
    }

    public void EnablePlayer()
    {
        if (_input != null) _input.actions.Enable();
    }

    public void RemovePlayerActions()
    {
        if (_input != null)
        {
            _input.actions = null;
        }
        else
        {
            transform.GetComponentInParent<PlayerInput>().actions = null;
        }

    }

    private void FixedUpdate()
    {
        _currentState.FixedUpdateStates();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentState.ReturnStateName() == PlayerBaseState.PlayerState.Projectile) return;
        _currentState.OnTriggerEnter(other);

        if (other.CompareTag("Upgrade") || other.CompareTag("Base") || other.CompareTag("Resource"))
        {
            interactable = other.GetComponentInParent(typeof(IInteractable)) as IInteractable;
            if (!other.CompareTag("Base"))
            {
                interactable.gameObject.GetComponent<INode>().PlayUpgradeNodeSound();
                if (!_isConnected) RuntimeManager.PlayOneShot(_connectionFailed);
            }
            else
            {
                if(_defenseAbilities.Count > 0) other.GetComponent<BaseButtonPopup>().EnableBaseDefenseButton();
                else other.GetComponent<BaseButtonPopup>().DisableBaseDefenseButton();
                if(_currentTutorialStep) TutorialStepComplete();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Upgrade") || other.CompareTag("Base") || other.CompareTag("Resource"))
        {
            interactable = null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        _currentState.OnCollisionEnter(collision);
    }

    public void BlockPlayer()
    {
        _statePreBlock = _currentState;
        movingAttackInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        movingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _currentState = _states.Blocked();
        _currentState.EnterState();
    }

    public void UnblockPlayer()
    {
        if (_statePreBlock != null) _currentState = _statePreBlock;
        else _currentState = _states.Idle();
        _currentState.EnterState();
    }

    private void OnInteract()
    {
        if (currentState.ReturnStateName() == PlayerBaseState.PlayerState.UI) return;
        if (currentState.ReturnStateName() == PlayerBaseState.PlayerState.Defense) return;
        if (currentState.ReturnStateName() == PlayerBaseState.PlayerState.Projectile) return;
        if (interactable != null)
        {
            interactable.Interact(this);
        }
    }

    private void OnExpandBase()
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 5))
        {
            Debug.Log(hit.normal);
        }
        if (!_paintingAllowed) return;
        if(_baseTexManager.OnBase(transform.position)) _isExpanding = true;
    }

    private void OnExpandBaseRelease()
    {
        if (!_paintingAllowed) return;
        _isExpanding = false;
    }

    private void OnRemoveBase()
    {
        if (!_paintingAllowed) return;
        _isRemoving = true;
    }

    private void OnRemoveBaseRelease()
    {
        if (!_paintingAllowed) return;
        _isRemoving = false;
    }

    // update resource manager if player is using resources or not, activate faster move speed on base
    private void CheckOnBase()
    {
        if (_onBase != _baseTexManager.OnBase(transform.position))
        {
            _onBase = !_onBase;

            if (_onBase)
            {
                _isConnected = true;
                if (_gooDecayCor != null)
                {
                    StopCoroutine(_gooDecayCor);
                    _gooDecayCor = null;
                    _ExplorationGooPS.startSize = _gooStartSize.constant;
                    _ExplorationGooPS.startLifetime = _gooStartLifeTime.constant;
                }
                if(_disconnectedWarningCor != null)
                {
                    StopCoroutine(_disconnectedWarningCor);
                    _disconnectedWarningCor = null;
                    _playerUI.HideDisconnectedInfo();
                }

                _baseManager.playersOutsideBase--;
            }
            else
            {
                if (_gooDecayCor == null) _gooDecayCor = StartCoroutine(GooDecay());
                if (_disconnectedWarningCor == null) _disconnectedWarningCor = StartCoroutine(NotConnectedWarning());

                _baseManager.playersOutsideBase++;
            }
        }
    }

    private IEnumerator NotConnectedWarning()
    {
        yield return new WaitForSeconds(_ExplorationGooPS.startLifetime.constant + 3);
        _playerUI.ShowDisconnectedInfo();
        RuntimeManager.PlayOneShot(_warning);
        _isConnected = false;
    }

    private IEnumerator GooDecay()
    {
        for(float t = 0; t < (_maxTimeOutsideBase * explorationGooSizeMod); t += Time.deltaTime)
        {
            float timePercent = t / _maxTimeOutsideBase;
            float sizeMod = _gooSizeDecay.Evaluate(timePercent);
            float lifeTimeMod = _gooLifeTimeDecay.Evaluate(timePercent);

            _ExplorationGooPS.startLifetime = _gooStartLifeTime.constant * lifeTimeMod;
            _ExplorationGooPS.startSize = _gooStartSize.constant * sizeMod;
            yield return null;
        }
    }

    private void OnResourceDiscovered()
    {
        if (!gooRefillOnResourceDiscover) return;
        _ExplorationGooPS.startSize = _gooStartSize.constant;
        _ExplorationGooPS.startLifetime = _gooStartLifeTime.constant;
        if (_gooDecayCor != null)
        {
            StopCoroutine(_gooDecayCor);
            _gooDecayCor = StartCoroutine(GooDecay());
        }
    }

    #region Defense
    private void OnActivate()
    {
        // just the 0 cause there are no others right now
        _selectedAbility = 0;// playerUI.SelectAbility(); 
        if (_currentState.ReturnStateName() == PlayerBaseState.PlayerState.Defense)
        {
            // activate aim arrow and lock the wheel
            if (_defenseAbilities[_selectedAbility].type == DefenseType.Projectile)
            {
                _defenseStep = DefenseStep.Steer;
            }
            else
            {
                _defenseAbilities[_selectedAbility].ActivateAbility(this);
                playerUI.OpenAbilityWheel(_defenseAbilities);
            }
        }
        else if(_currentState.ReturnStateName() == PlayerBaseState.PlayerState.Projectile)
        {
            if (!_defenseDirection.gameObject.activeInHierarchy) return;
            _defenseAbilities[_selectedAbility].ActivateAbility(this);
            _timesDetonated++;
            RuntimeManager.PlayOneShot(_combust);
            if (_timesDetonated >= detonateAmount)
            {
                _timesDetonated = 0;
                _projectileDir = Vector3.zero;
                _defenseDirection.gameObject.SetActive(false);
                Invoke("ReturnAfterDetonate", 0.3f);
                _attackTrail.SetActive(false);
            }
        }
    }

    private void ReturnAfterDetonate()
    {
        //playerUI.OpenAbilityWheel(_defenseAbilities);
        playerUI.ShowDefenseHints();
        _defenseDirection.gameObject.SetActive(true);
        controller.enabled = false;
        transform.position = new Vector3(0, 1, 0);
        controller.enabled = true;
        movingAttackInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        movingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _currentState = _states.Defense();
        _currentState.EnterState();
    }

    private void OnActivateRelease()
    {
        if (currentState.ReturnStateName() != PlayerBaseState.PlayerState.Defense || _defenseStep == DefenseStep.NONE) return;
        playerUI.HideDefenseHints();
        movingAttackInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        movingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _currentState = _states.Projectile();
        _currentState.EnterState();
        RuntimeManager.PlayOneShot(_startAttack);
        if (direction == Vector3.zero) _projectileDir = Vector3.forward;
        else _projectileDir = direction;

        _attackTrail.SetActive(true);

        _defenseStep = DefenseStep.Aim;
        _playerUI.steerInfo.alpha = 1;
        _playerUI.steerInfoArrow.DOFade(1, 0);
        _playerUI.steerInfo.DOFade(0.2f, 0.7f).SetLoops(-1, LoopType.Yoyo);
        _playerUI.steerInfoArrow.DOFade(0.2f, 0.7f).SetLoops(-1, LoopType.Yoyo);
        DOTween.Kill(_playerUI.emitInfo);
        _playerUI.emitInfo.DOFade(0.5f, 0.5f);

    }

    private void OnCancel()
    {
        // Todo allow to close wheel with cancel before completely closing the defense
        if (_currentState.ReturnStateName() != PlayerBaseState.PlayerState.Defense) return;
        StopDefense();
    }

    private void OnStartDefense()
    {
        if (interactable == null || _defenseAbilities.Count == 0) return;
        if (currentState.ReturnStateName() == PlayerBaseState.PlayerState.UI) return;
        if(interactable.gameObject.CompareTag("Base")) StartDefense();
    }

    public void StartDefense()
    {
        if(_currentState.ReturnStateName() == PlayerBaseState.PlayerState.Defense) return;
        topDownCam.enabled = true;
        movingAttackInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        movingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _currentState = _states.Defense();
        _currentState.EnterState();
        _defenseCameraSwing = StartCoroutine(DefenseCameraSwing());
        StartCoroutine(MoveToBaseCenter());
        _ExplorationGoo.SetActive(false);
    }

    private IEnumerator DefenseCameraSwing()
    {
        yield return new WaitForSeconds(2f);
        //_defenseDirection.gameObject.SetActive(true);
        playerUI.OpenAbilityWheel(_defenseAbilities);
        _defenseDirection.gameObject.SetActive(true);
        _defenseStep = DefenseStep.Aim;
        _playerUI.steerInfo.alpha = 1;
        _playerUI.steerInfoArrow.DOFade(1, 0);
        _playerUI.steerInfo.DOFade(0.2f, 0.7f).SetLoops(-1, LoopType.Yoyo);
        _playerUI.steerInfoArrow.DOFade(0.2f, 0.7f).SetLoops(-1, LoopType.Yoyo);
        
        //GetComponent<CapsuleCollider>().enabled = false;
        //controller.detectCollisions = false;
        // when upgrade gets added
        //_defenseAbilities.Add(gameObject.AddComponent<PoisonSpores>());
    }


    public void StopDefense()
    {
        if (_currentState.ReturnStateName()!= PlayerBaseState.PlayerState.Defense) return;
        StopCoroutine(_defenseCameraSwing);
        topDownCam.enabled = false;
        _defenseDirection.gameObject.SetActive(false);
        movingAttackInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        movingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _currentState = _states.Idle();
        _currentState.EnterState();
        _ExplorationGoo.SetActive(true);
        playerUI.HideAbilities();
        //GetComponent<CapsuleCollider>().enabled = true;
        //controller.detectCollisions = true;
        // when upgrade gets removed
        //_defenseAbilities.Remove(GetComponent<PoisonSpores>());
        //Destroy(GetComponent<PoisonSpores>());
        _defenseStep = DefenseStep.NONE;
    }

    private IEnumerator MoveToBaseCenter()
    {
        Vector3 basePos = new Vector3(0, transform.position.y, 0);
        Vector3 dir;
        while (Vector3.Distance(transform.position, basePos) > 0.1f || _currentState.ReturnStateName() != PlayerBaseState.PlayerState.Defense)
        {
            dir = (basePos - transform.position).normalized;
            controller.Move(speed * Time.deltaTime * dir);
            yield return null;
        }
    }

    public void DefenseDirection()
    {
        if (_defenseStep == DefenseStep.Aim && direction.magnitude > 0.5f)
        {
            _defenseStep = DefenseStep.Emit;
            DOTween.Kill(_playerUI.steerInfo);
            DOTween.Kill(_playerUI.steerInfoArrow);
            _playerUI.steerInfo.DOFade(0.5f, 0.5f);
            _playerUI.steerInfoArrow.DOFade(0f, 0.5f);
            _playerUI.emitInfo.DOFade(0.2f, 0.7f).SetLoops(-1, LoopType.Yoyo);
        }
        float angle = Mathf.Atan2(direction.x, direction.z);
        Vector3 _projectileRot = new Vector3(0, 0, -angle * 180 / Mathf.PI);
        _defenseDirection.localRotation = Quaternion.Euler(_projectileRot);
    }

    public void ProjectileMove()
    {
        // apply directional change based on a degree of effectiveness
        float angle = Vector3.SignedAngle(_projectileDir, direction, Vector3.up);
        if (angle > 0) angle = _projectileRotSpeed;
        else if (angle < 0) angle = -_projectileRotSpeed;
        _projectileDir = Quaternion.AngleAxis(angle, Vector3.up) * _projectileDir;
        controller.Move(_projectileDir.normalized * speed * Time.deltaTime);

        float arrowAngle = Mathf.Atan2(_projectileDir.x, _projectileDir.z);
        Vector3 _projectileRot = new Vector3(0, 0, -arrowAngle * 180 / Mathf.PI);
        _defenseDirection.localRotation = Quaternion.Euler(_projectileRot);

        if (!_baseTexManager.OnBase(transform.position)) OnActivate();
    }

    #endregion

    #region movement

    private void OnMove(InputValue inputValue)
    {
        Vector2 MoveDelta = inputValue.Get<Vector2>();
        direction = new Vector3(MoveDelta.x, 0, MoveDelta.y);
        if (direction.magnitude > 1) direction = direction.normalized;
        _movePlayer = true;
    }

    private void OnSprint()
    {
        _sprint = !sprint;
    }

    private void ApplyGravity()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    public void MoveCharacter()
    {
        float onBaseMod = speedMod * speedInBaseMod > 1 ? abilityPotencyMod * speedMod * speedInBaseMod : 1;
        float offBaseMod = speedMod * speedOutsideBaseMod > 1 ? abilityPotencyMod * speedMod * speedOutsideBaseMod : 1;
        float currentSpeed = _onBase ? speed * onBaseMod : speedOutsideBase * offBaseMod;
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0, angle, 0);
            Vector3 moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            if (!sprint) controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
            else controller.Move(moveDir.normalized * sprintSpeed * Time.deltaTime);
        }
        else
        {
            _movePlayer = false;
        }
    }

    #endregion

    public void ActivateTutorialMode(TutorialAction action)
    {
        _currentTutorialStep = true;
        _action = action;
    }

    private void TutorialStepComplete()
    {
        foreach (PlayerStateManager player in _baseManager.players)
        {
            player.tutorialManager.ActionExecuted(_action);
        }
        _currentTutorialStep = false;
    }

    private void OnStart()
    {
        _start = true;
        if (_start && _select)
        {
            if(SceneManager.GetActiveScene().buildIndex == 0)
            {
                Application.Quit();
            }
            else
            {
                SceneManager.LoadScene(2);
            }
        }
    }

    private void OnSelect()
    {
        _select = true;
        if (_start && _select)
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                Application.Quit();
            }
            else
            {
                SceneManager.LoadScene(2);
            }
        }
    }


    private void OnStartRelease()
    {
        _start = false;
    }

    private void OnSelectRelease()
    {
        _select = false;
    }
}
