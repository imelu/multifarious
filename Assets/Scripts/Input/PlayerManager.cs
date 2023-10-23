using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;

public class PlayerManager : MonoBehaviour
{
    //[SerializeField] private List<Shader> shaders = new List<Shader>();
    [SerializeField] private List<PlayerInput> players = new List<PlayerInput>();
    [SerializeField] private List<Transform> startingPoints;
    [SerializeField] private List<LayerMask> playerLayers;

    [SerializeField] private GameObject StartCam1;
    [SerializeField] private GameObject StartCam2;

    [SerializeField] private GameObject SingleKeyboard;
    [SerializeField] private GameObject SingleKeyboardText;
    // Wait for opponent pulsating
    [SerializeField] private TMP_Text PlayerOneText;
    [SerializeField] private TMP_Text PlayerTwoText;

    [SerializeField] private float textPulseDuration;

    private PlayerInputManager playerInputManager;

    private void Awake()
    {
        playerInputManager = GetComponent<PlayerInputManager>();
    }

    private void OnEnable()
    {
        playerInputManager.onPlayerJoined += AddPlayer;
    }

    private void OnDisable()
    {
        playerInputManager.onPlayerJoined -= AddPlayer;
    }

    private void Start()
    {
        PlayerOneText.DOColor(new Color(1, 1, 1, 0), textPulseDuration).SetLoops(-1, LoopType.Yoyo);
        PlayerTwoText.DOColor(new Color(1, 1, 1, 0), textPulseDuration).SetLoops(-1, LoopType.Yoyo);
        SingleKeyboardText.GetComponent<TMP_Text>().DOColor(new Color(1, 1, 1, 0), textPulseDuration).SetLoops(-1, LoopType.Yoyo);
    }

    public void AddPlayer(PlayerInput player)
    {
        /*players.Add(player);

        SetPlayerToSpawnpoint(player.transform);

        //Spawnpositions
        Transform playerParent = player.transform.parent;
        
        //playerParent.position = startingPoints[players.Count - 1].position;
        

        
        playerParent.GetComponentInChildren<InputHandler>().horizontal = player.actions.FindAction("Look");


        if (players.Count == 1)
        {
            player.GetComponent<PlayerStateManager>().isPlayerOne = true;
            GlobalGameManager.Instance.Player1 = player.GetComponent<PlayerStateManager>();
            playerParent.GetComponentInChildren<Camera>().rect = new Rect(0,0,0.5f,1);
            //StartCam1.SetActive(false);
            PlayerOneText.text = "Wait for Opponent";
            if(!player.GetComponent<PlayerInput>().currentControlScheme.Equals("Gamepad")) PlayerTwoText.text = "Press Start";
        }
        else
        {
            player.GetComponent<PlayerStateManager>().isPlayerOne = false;
            GlobalGameManager.Instance.Player2 = player.GetComponent<PlayerStateManager>();
            StartCam1.SetActive(false); // new start
            StartCam2.SetActive(false);
            GlobalGameManager.Instance.Player1.cam.GetComponent<Camera>().enabled = true;
            GlobalGameManager.Instance.Player2.cam.GetComponent<Camera>().enabled = true;
        }

        GlobalGameManager.Instance.soundManager.AddPlayer(player.GetComponent<PlayerStateManager>());*/
    }

    private void Update()
    {
        
    }

    private void SetPlayerToSpawnpoint(Transform player)
    {
        player.GetComponent<CharacterController>().enabled = false;
        // set to spawn
        player.GetComponent<CharacterController>().enabled = true;
    }
}
