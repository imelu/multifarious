using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateManager currentContext, PlayerStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        //Debug.Log("player idle");
    }

    public override void UpdateState()
    {
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
        if (Ctx.movePlayer) SwitchState(Factory.Walk());
        if (Ctx.inUI) SwitchState(Factory.UI());
    }

    public override PlayerState ReturnStateName()
    {
        PlayerState value = PlayerState.Idle;
        return value;
    }
}
