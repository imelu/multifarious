using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageLights : MonoBehaviour
{
    [SerializeField] private Light _lightSource;
    [SerializeField] private float _changeLightDuration = 1f;
    private float _originalLightIntensity;

    private int _playersInTrigger = 0;

    void Start()
    {
        _originalLightIntensity = _lightSource.intensity;
        if(_playersInTrigger == 0)
        {
            _lightSource.intensity = 0;
            _lightSource.enabled = false;
        }
    }

    private void TurnLightUp()
    {
        _lightSource.enabled = true;
        _lightSource.DOIntensity(_originalLightIntensity, _changeLightDuration);
    }
    private void TurnLightDown()
    {
        _lightSource.DOIntensity(0, _changeLightDuration).OnComplete(() => _lightSource.enabled = false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playersInTrigger++;
            if (_playersInTrigger == 1) TurnLightUp();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playersInTrigger--;
            if (_playersInTrigger == 0) TurnLightDown();
        }
    }
}
