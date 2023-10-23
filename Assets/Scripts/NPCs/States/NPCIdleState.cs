using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NPCIdleState : NPCBaseState
{
    public NPCIdleState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        Ctx.targetFoodPos = Ctx.FindFood();
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

    public override void OnTriggerExit(Collider col)
    {

    }

    public override void ExitState()
    {
    }

    public override void CheckSwitchState()
    {
        if (Ctx.selectedAction == NPCAIStateManager.NPCAction.walk)
        {
            SwitchState(Factory.Walk());
        }
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.Idle;
    }
}
