using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineParticle : MonoBehaviour
{
    private List<Vector2> _followPath;
    private bool _isMoving;
    private Vector3 _nextPos;
    private Vector3 _direction;
    [SerializeField] private float _speed;
    private ParticleSystem _ParticleSystem;

    public INode nodeToConnect;

    private void Start()
    {
        nodeToConnect.vineParticle = this;
        _ParticleSystem = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (_isMoving) Move();
    }

    private void Move()
    {
        _direction = (_nextPos - transform.position).normalized;
        transform.Translate(_direction * _speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, _nextPos) < 0.2f)
        {
            GetNextPosition();
        }
    }

    public void FollowPath(List<Vector2> path)
    {
        _followPath = path;
        GetNextPosition();
        _isMoving = true;
    }

    private void GetNextPosition()
    {
        if (_followPath.Count > 0)
        {
            Vector2 pos = _followPath[_followPath.Count - 1];
            _nextPos = new Vector3(pos.x, transform.position.y, pos.y);
            _followPath.RemoveAt(_followPath.Count - 1);
        }
        else
        {
            DestinationReached();
        }
    }

    private void DestinationReached()
    {
        _isMoving = false;
        StartCoroutine(DestReachedDelay());
    }

    private IEnumerator DestReachedDelay()
    {
        yield return new WaitForSeconds(0.8f);
        // freeze particle
        _ParticleSystem.Pause();
    }

    public void Die()
    {
        Destroy(gameObject);
        /*_ParticleSystem.Play();
        _ParticleSystem.Stop();
        StartCoroutine(DeathAfterDelay());*/
    }

    private IEnumerator DeathAfterDelay()
    {
        yield return new WaitForSeconds(20);
        Destroy(gameObject);
    }
}

