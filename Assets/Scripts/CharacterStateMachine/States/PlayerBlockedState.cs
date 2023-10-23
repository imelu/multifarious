using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBlockedState : PlayerBaseState
{
    public PlayerBlockedState(PlayerStateManager currentContext, PlayerStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
    }

    public override void UpdateState()
    {
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
        PlayerState value = PlayerState.Blocked;
        return value;
    }
}
