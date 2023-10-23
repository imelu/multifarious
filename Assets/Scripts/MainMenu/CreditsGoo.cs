using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CreditsGoo : MonoBehaviour
{
    [SerializeField] private Transform _goal;
    private ParticleSystem _particleSystem;

    private void OnEnable()
    {
        MoveToPosition();
        _particleSystem = GetComponent<ParticleSystem>();
    }

    public void MoveToPosition()
    {
        transform.DOMove(_goal.position, 1.5f).OnComplete(() => StartCoroutine(FreezeAndDestroy()));
    }

    private IEnumerator FreezeAndDestroy()
    {
        //_particleSystem.Stop();

        yield return new WaitForSeconds(_particleSystem.main.startLifetime.constant);

        //Destroy(gameObject);
    }
}
