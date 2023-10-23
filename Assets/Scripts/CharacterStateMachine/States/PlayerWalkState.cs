using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{
    public PlayerWalkState(PlayerStateManager currentContext, PlayerStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        //Debug.Log("player walk");
        Ctx.movingInstance.start();
    }

    public override void UpdateState()
    {
        Ctx.MoveCharacter();
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

        if (!Ctx.movePlayer)
        {
            Ctx.movingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            SwitchState(Factory.Idle());
        }
        if (Ctx.inUI) 
        {
            Ctx.movingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            SwitchState(Factory.UI()); 
        }
    }

    public override PlayerState ReturnStateName()
    {
        PlayerState value = PlayerState.Walk;
        return value;
    }
}
