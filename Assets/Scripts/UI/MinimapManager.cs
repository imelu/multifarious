using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapManager : MonoBehaviour
{
    private BaseTexManager _baseTexManager;
    private BaseManager _baseManager;
    [SerializeField] private RectTransform[] _players = new RectTransform[2];
    private RectTransform _miniMap;

    private void Start()
    {
        _baseTexManager = GlobalGameManager.Instance.baseTexManager;
        _baseManager = _baseTexManager.GetComponent<BaseManager>();
        _miniMap = GetComponent<RectTransform>();
    }

    private void Update()
    {
        SetPlayerPositions();
    }

    private void SetPlayerPositions()
    {
        for(int i = 0; i < _players.Length; i++)
        {
            Vector3 playerPos = _baseManager.players[i].transform.position;
            Vector2 percentPos = _baseTexManager.GetPercentPos(new Vector2(-playerPos.x, playerPos.z));
            
            Vector2 playerUIPos = percentPos * _miniMap.rect.width;

            _players[i].anchoredPosition = new Vector3(playerUIPos.x - _miniMap.rect.width/2, -(playerUIPos.y - _miniMap.rect.width / 2), 0);
        }
    }
}
