using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonTrail : MonoBehaviour
{
    private float _dps;
    public float dps { get { return _dps; } set { _dps = value; } }
    private float _duration;
    public float duration { get { return _duration; } set { _duration = value; } }

    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<NPCHealthManager>().GetPoisoned(PoisonType.trail, dps, duration);
        }
    }
}
