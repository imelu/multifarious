using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopAndDestroySelf : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    [SerializeField] private bool _isSmoke;

    // Start is called before the first frame update
    void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        StartCoroutine(ControlParticleSystem());
    }

    private IEnumerator ControlParticleSystem()
    {
        if(!_isSmoke) yield return new WaitForSeconds(2);

        _particleSystem.Stop();

        yield return new WaitForSeconds(_particleSystem.main.startLifetime.constant);

        if(GetComponentInParent<GameIntroManager>() != null) GetComponentInParent<GameIntroManager>().particleSystemsDestroyed++;
        Destroy(gameObject);
    }
}
