using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonSpores : MonoBehaviour, IDefenseAbility
{
    private PlayerStateManager _player;

    private DefenseType _type = DefenseType.Projectile;
    public DefenseType type { get { return _type; } }
    private Sprite _sprite;
    public Sprite sprite { get { return _sprite; } set { _sprite = value; } }

    private GameObject _ExplosionPrefab;
    public GameObject ExplosionPrefab { get { return _ExplosionPrefab; } set { _ExplosionPrefab = value; } }

    private float _dps;
    public float dps { get { return _dps; } set { _dps = value; } }
    private float _poisonDuration;
    public float posionDuration { get { return _poisonDuration; } set { _poisonDuration = value; } }

    private void Start()
    {
        _player = GetComponent<PlayerStateManager>();
    }

    public void ActivateAbility(PlayerStateManager player)
    {
        GameObject Explosion = Instantiate(_ExplosionPrefab, player.transform.position, Quaternion.identity);
        PoisonExplosion poisonExplosion = Explosion.GetComponent<PoisonExplosion>();
        // TODO add upgrade modifiers here
        poisonExplosion.dps = dps * _player.abilityPotencyMod;
        poisonExplosion.poisonDuration = posionDuration;
        poisonExplosion.duration *= _player.abilityLifetimeMod;
        poisonExplosion.radius *= _player.abilitySizeMod;
        poisonExplosion.StartLifetime();
    }
}
