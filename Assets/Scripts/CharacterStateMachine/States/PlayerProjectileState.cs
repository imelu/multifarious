using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectileState : PlayerBaseState
{
    public PlayerProjectileState(PlayerStateManager currentContext, PlayerStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        Ctx.movingAttackInstance.start();
    }

    public override void UpdateState()
    {
        Ctx.ProjectileMove();
        CheckSwitchState();
    }

    public override void FixedUpdateState()
    {

    }

    public override void OnCollisionEnter(Collision col)
    {

    }

    public override void OnTriggerEnter(Collider col)
    {
        
    }

    public override void ExitState()
    {

    }

    public override void CheckSwitchState()
    {

    }

    public override PlayerState ReturnStateName()
    {
        PlayerState value = PlayerState.Projectile;
        return value;
    }
}
