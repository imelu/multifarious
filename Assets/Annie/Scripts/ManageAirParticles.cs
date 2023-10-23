using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageAirParticles : MonoBehaviour
{
    [SerializeField] private List<Transform> _particles;
    //[SerializeField] private float buttonHeight = 3;
    public GameObject particles;
    private ParticleSystem _particleSystem;

    private int _playersInTrigger = 0;

    private bool _isEnabled = true;
    public bool isEnabled
    {
        get { return _isEnabled; }
        set
        {
            _isEnabled = value;
            if (value && _playersInTrigger > 0) ShowParticles();
            if (!value) HideParticles();
        }
    }

    private void Start()
    {
        _particleSystem = particles.GetComponent<ParticleSystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        _playersInTrigger++;
        if (!isEnabled) return;
        if (_playersInTrigger == 1)
        {
            ShowParticles();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        _playersInTrigger--;
        if (!isEnabled) return;
        if (_playersInTrigger > 0) return;
        HideParticles();
    }

    private void ShowParticles()
    {
        foreach (Transform airParticles in _particles)
        {
            //particles.SetActive(true);
            
        }
        //if (_particleSystem != null) _particleSystem.Stop();
        //else particles.SetActive(false);
    }

    private void HideParticles()
    {
        foreach (Transform airParticles in _particles)
        {
            //particles.SetActive(false);
            
        }
        //if (_particleSystem != null) _particleSystem.Play();
        //else particles.SetActive(true);
    }
}
