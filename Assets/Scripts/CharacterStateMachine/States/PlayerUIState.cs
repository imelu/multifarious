using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUIState : PlayerBaseState
{
    public PlayerUIState(PlayerStateManager currentContext, PlayerStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {

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
        if (!Ctx.inUI) SwitchState(Factory.Idle());
    }

    public override PlayerState ReturnStateName()
    {
        PlayerState value = PlayerState.UI;
        return value;
    }
}
