using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDefenseState : PlayerBaseState
{
    public PlayerDefenseState(PlayerStateManager currentContext, PlayerStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        //Debug.Log("Defense");
    }

    public override void UpdateState()
    {
        Ctx.DefenseDirection();
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
        PlayerState value = PlayerState.Defense;
        return value;
    }
}
