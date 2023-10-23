using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using FMODUnity;

public class ChangeColor : MonoBehaviour
{
    private BaseTexManager _baseTexManager;
    public Color white;
    public Color black;
    private Renderer rend;

    private float _colorSwitchTime = 1;

    private Renderer[] _renderers;

    [SerializeField] private bool _isInStartBase;
    [SerializeField] private bool _isSmallPlant;
    [SerializeField] private bool _isParticleRoots;

    private bool _isBlack;

    void Start()
    {
        DOTween.SetTweensCapacity(500,50);

        rend = GetComponent<Renderer>();
        _renderers = GetComponentsInChildren<Renderer>();

        if (_isInStartBase) ChangeColorToBlack();

        if (!_isSmallPlant) return;

        _baseTexManager = GlobalGameManager.Instance.baseTexManager;
        _baseTexManager.OnMapChange += OnMapChanged;
    }

    [ContextMenu("Change to Black")]
    public void ChangeColorToBlack()
    {
        if (_isBlack) return;
        _isBlack = true;
        if (rend != null)
        {
            if (_isParticleRoots)
            {
                DOTween.Kill(rend.materials[1]);
                rend.materials[1].DOColor(black, _colorSwitchTime);
            }
            else
            {
                DOTween.Kill(rend.material);
                rend.material.DOColor(black, _colorSwitchTime);
            }
            
        }
        if (_renderers == null) return;
        foreach(Renderer renderer in _renderers)
        {
            if(renderer.gameObject.layer != 10)
            {
                DOTween.Kill(renderer.material);
                renderer.material.DOColor(black, _colorSwitchTime);
            }
        }
    }

    [ContextMenu("Change to White")]
    public void ChangeColorToWhite()
    {
        if (!_isBlack) return;
        _isBlack = false;
        if (rend != null)
        {
            if (_isParticleRoots)
            {
                DOTween.Kill(rend.materials[1]);
                rend.materials[1].DOColor(white, _colorSwitchTime);
            }
            else
            {
                DOTween.Kill(rend.material);
                rend.material.DOColor(white, _colorSwitchTime);
            }
        }
        if (_renderers == null) return;
        foreach (Renderer renderer in _renderers)
        {
            if (renderer.gameObject.layer != 10)
            {
                DOTween.Kill(renderer.material);
                renderer.material.DOColor(white, _colorSwitchTime);
            }
        }
    }

    private void OnMapChanged()
    {
        if (_baseTexManager.OnBase(transform.position))
        {
            ChangeColorToBlack();
        }
    }
}
