using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveGrowth : MonoBehaviour, IDefenseAbility
{
    private DefenseType _type = DefenseType.Projectile;
    public DefenseType type { get { return _type; } }
    private Sprite _sprite;
    public Sprite sprite { get { return _sprite; } set { _sprite = value; } }
    public void ActivateAbility(PlayerStateManager player)
    {
        Debug.Log("chhhschhh");
    }
}
