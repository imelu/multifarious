using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseManager : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerStateManager[] _players = new PlayerStateManager[2];
    public PlayerStateManager[] players { get { return _players; } }

    private UpgradeManager _upgradeManager;
    private bool _baseUIOpen;
    public bool baseUIOpen { get { return _baseUIOpen; } set { _baseUIOpen = value; } }

    private ResourceManager _resourceManager;
    private BaseUIManager _baseUIManager;

    [SerializeField] private GameIntroManager _gameIntroManager;
    public GameIntroManager gameIntroManager { get { return _gameIntroManager; } }
    public BaseUIManager baseUIManager { get { return _baseUIManager; } }

    private int _playersOutsideBase;
    public int playersOutsideBase { get { return _playersOutsideBase; } 
        set 
        { 
            _playersOutsideBase = value;
            if (_playersOutsideBase < 0) _playersOutsideBase = 0;
            if (_playersOutsideBase == 0) GlobalGameManager.Instance.soundManager.SetMusicState(MusicStates.inside);
            else GlobalGameManager.Instance.soundManager.SetMusicState(MusicStates.outside);
        } 
    }

    #region Health

    [Header("Health")]
    [SerializeField] private GameObject _healthUI;
    [SerializeField] private int _startHealth;
    private float _currentHealth;
    [SerializeField] private float _healthLossPerSecondPerResourceMissing;
    [SerializeField] private float _healthRegenPerSecond;
    private Coroutine _surviveCor;
    private Coroutine _regenerateCor;

    #endregion

    #region Upgrades

    private float _connectParticleSizeMod = 1;
    public float connectParticleSizeMod { get { return _connectParticleSizeMod; } set { _connectParticleSizeMod = value; } }

    private bool _toxicMycelium = false;
    public bool toxicMycelium { get { return _toxicMycelium; } set { _toxicMycelium = value; } }
    private float _toxicDOT;
    public float toxicDOT { get { return _toxicDOT; } set { _toxicDOT = value; } }
    private float _toxicDuration;
    public float toxicDuration { get { return _toxicDuration; } set { _toxicDuration = value; } }

    #endregion

    [SerializeField] private EventReference _warning;
    private EventInstance _warningInstance;

    [Header("Tutorial")]
    [SerializeField] private UpgradeNode _tutorialUpgradeNode;
    public UpgradeNode tutorialUpgradeNode { get { return _tutorialUpgradeNode; } }
    [SerializeField] private ResourceNode _tutorialResourceNode;
    public ResourceNode tutorialResourceNode { get { return _tutorialResourceNode; } }

    private void Start()
    {
        _upgradeManager = GetComponent<UpgradeManager>();
        _resourceManager = GetComponent<ResourceManager>();
        _baseUIManager = GetComponent<BaseUIManager>();
        _currentHealth = _startHealth;
        _healthUI.SetActive(false);

        _warningInstance = RuntimeManager.CreateInstance(_warning);
        _warningInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
    }

    public void Interact(PlayerStateManager player)
    {
        if (!_baseUIOpen)
        {
            
            player.playerUI.OpenBaseWindow(_upgradeManager.upgrades);
            _baseUIManager.EnableBarMask(player.isPlayerOne);
            _baseUIOpen = true;
        }
    }

    public void StartSurvival()
    {
        if (_regenerateCor != null) StopCoroutine(_regenerateCor);
        if (_surviveCor == null) _surviveCor = StartCoroutine(LoseHealth());
        _regenerateCor = null;
    }

    public void StopSurvival()
    {
        if (_surviveCor != null) StopCoroutine(_surviveCor);
        if (_regenerateCor == null) _regenerateCor = StartCoroutine(RegenerateHealth());
        _surviveCor = null;
    }

    private IEnumerator LoseHealth()
    {
        _warningInstance.start();
        _healthUI.SetActive(true);
        while (_currentHealth > 0)
        {
            _currentHealth -= _resourceManager.missingResourcesAmount * _healthLossPerSecondPerResourceMissing * Time.deltaTime;
            _baseUIManager.UpdateHP(_currentHealth / (float)_startHealth);
            GlobalGameManager.Instance.soundManager.SetMusicIntensity((int)_currentHealth);
            yield return null;
        }
        // TODO ded
        _healthUI.SetActive(false);
        GlobalGameManager.Instance.PlayersDied();
        _warningInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        foreach (PlayerStateManager player in players)
        {
            player.BlockPlayer();
        }
    }

    private IEnumerator RegenerateHealth()
    {
        _warningInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _healthUI.SetActive(true);
        while (_currentHealth < _startHealth)
        {
            _currentHealth += _healthRegenPerSecond * Time.deltaTime;
            _baseUIManager.UpdateHP(_currentHealth / _startHealth);
            GlobalGameManager.Instance.soundManager.SetMusicIntensity((int)_currentHealth);
            yield return null;
        }
        if(_currentHealth > _startHealth) _currentHealth = _startHealth;
        GlobalGameManager.Instance.soundManager.SetMusicIntensity((int)_currentHealth);
        _healthUI.SetActive(false);
    }
}
