using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DefenseType
{
    Projectile,
    Immediate
}

public interface IDefenseAbility
{
    public DefenseType type { get; }
    public Sprite sprite { get; set; }
    public void ActivateAbility(PlayerStateManager player);
}
