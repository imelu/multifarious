using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonExplosion : MonoBehaviour
{
    [SerializeField] private float _radius;
    public float radius { get { return _radius; } set { _radius = value; } }
    [SerializeField] private float _duration;
    public float duration { get { return _duration; } set { _duration = value; } }
    [SerializeField] private Transform _visuals;
    private float _dps;
    public float dps { get { return _dps; } set { _dps = value; } }
    private float _poisonDuration;
    public float poisonDuration { get { return _poisonDuration; } set { _poisonDuration = value; } }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // poison enemy
            // periodically damage enemy until he leaves?
            other.GetComponent<NPCHealthManager>().GetPoisoned(PoisonType.spores, dps, poisonDuration);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // probably nothing
        }
    }

    public void StartLifetime()
    {
        GetComponent<SphereCollider>().radius = radius;
        StartCoroutine(Lifetime());
        _visuals.localScale = Vector3.one * 2 * radius;
    }

    private IEnumerator Lifetime()
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}
