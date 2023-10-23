using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCHealthManager : MonoBehaviour
{
    [SerializeField] private Animator _anim;

    [SerializeField] private float _maxHealth;
    private float _currentHealth;

    private Coroutine _sporesCor;
    private Coroutine _trailCor;
    private Coroutine _toxicCor;

    private bool _dead;

    private NPCAIStateManager _npc;

    private void Start()
    {
        _npc = GetComponent<NPCAIStateManager>();
        _currentHealth = _maxHealth;
    }

    public void GetPoisoned(PoisonType type, float DoT, float duration)
    {
        _anim.SetBool("Intoxicated", true);
        if(_sporesCor == null && _trailCor == null && _toxicCor == null) _anim.SetTrigger("Hit");
        switch (type)
        {
            case PoisonType.spores:
                if (_sporesCor != null) StopCoroutine(_sporesCor);
                _sporesCor = StartCoroutine(PoisonDOT(type, DoT, duration));
                break;

            case PoisonType.trail:
                if (_trailCor != null) StopCoroutine(_trailCor);
                _trailCor = StartCoroutine(PoisonDOT(type, DoT, duration));
                break;

            case PoisonType.toxic:
                if (_toxicCor != null) StopCoroutine(_toxicCor);
                _toxicCor = StartCoroutine(PoisonDOT(type, DoT, duration));
                break;
        }
    }

    private IEnumerator PoisonDOT(PoisonType type, float DoT, float duration)
    {
        _npc.poisonedInstance.start();
        for(float t = 0; t < duration; t += Time.deltaTime)
        {
            _currentHealth -= DoT * Time.deltaTime;
            if(_currentHealth <= 0)
            {
                Die();
                break;
            }
            yield return null;
        }
        if(type == PoisonType.toxic && _npc.currentState.ReturnStateName() == NPCBaseState.NPCStates.Eat) _toxicCor = StartCoroutine(PoisonDOT(type, DoT, duration));
        else
        {
            if (type == PoisonType.spores) _sporesCor = null;
            else if (type == PoisonType.trail) _trailCor = null;
            else if (type == PoisonType.toxic) _toxicCor = null;
            _anim.SetBool("Intoxicated", false);
            _npc.poisonedInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        } 
    }

    private void Die()
    {
        if (_dead) return;
        _dead = true;
        _anim.SetTrigger("Die");
        _anim.SetBool("Dead", true);
        _npc.Die();
        //Destroy(gameObject);
        // play death animation
        // destroy after completed anim
    }
}
