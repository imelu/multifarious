using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static NPCAIStateManager;

public class NPCEatState : NPCBaseState
{
    public NPCEatState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        Ctx.agent.speed = Ctx.eatingSpeed;
        Ctx.selectedAction = NPCAction.NONE;
        Ctx.agent.SetDestination(Ctx.targetFoodPos);
        if (Ctx.baseManager.toxicMycelium) Ctx.npcHealth.GetPoisoned(PoisonType.toxic, Ctx.baseManager.toxicDOT, Ctx.baseManager.toxicDuration);
        Ctx.anim.SetBool("OnBase", true);

        Ctx.eatingInstance.start();
    }

    public override void UpdateState()
    {
        CheckSwitchState();
        if (Ctx.agent.isPathStale) Ctx.currentState.EnterState();
        // TODO make this distance based
        if(Time.frameCount % 35 == 0) Ctx.baseTexManager.RemoveOnPos(Ctx.eatingPoint.position, BaseTexManager.DrawSize.medium);
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
        if ((Ctx.agent.remainingDistance <= Ctx.stopProximity) && !Ctx.agent.pathPending && Ctx.agent.hasPath)
        {
            Ctx.agent.ResetPath();
            //SwitchState(Factory.Idle());

            if(Ctx.selectedAction == NPCAction.walk)
            {
                Ctx.eatingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                Ctx.targetFoodPos = Ctx.FindFood();
                SwitchState(Factory.Walk());
            }
            else
            {
                // check in look direction in a small distance if there is still food, if yes go eat that, if no, check for new nearest food
                Vector3 checkPos = Ctx.eatingPoint.position + Ctx.transform.TransformDirection(Vector3.forward) * 0.3f;
                if (!Ctx.baseTexManager.OnBase(checkPos))
                {
                    Ctx.selectedAction = NPCAction.walk;
                }
                Ctx.agent.SetDestination(checkPos);
            }
        }
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.Eat;
    }
}
