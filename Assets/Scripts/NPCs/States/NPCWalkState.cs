using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static NPCAIStateManager;

public class NPCWalkState : NPCBaseState
{
    public NPCWalkState(NPCAIStateManager currentContext, NPCStateFactory factory) : base(currentContext, factory)
    {
    }

    public override void EnterState()
    {
        Ctx.agent.speed = Random.Range(Ctx.speedMin, Ctx.speedMax);
        Ctx.selectedAction = NPCAction.NONE;
        Ctx.agent.SetDestination(Ctx.targetFoodPos);
        //Ctx.agent.SetDestination(GlobalGameManager.Instance.npcManager.GetRandomLocationInRoom(Ctx.currentRoom));
        //Debug.Log("Walk");
        Ctx.anim.SetBool("OnBase", false);

        Ctx.movingInstance.start();
    }

    public override void UpdateState()
    {
        CheckSwitchState();
        if (Ctx.agent.isPathStale) Ctx.currentState.EnterState();
        //Ctx.baseTexManager.DrawOnPos(Ctx.transform.position);
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
        // TODO check if standing on food so it doesnt walk halfway into the connection
        if (Ctx.baseTexManager.OnBase(Ctx.eatingPoint.position))
        {
            Ctx.movingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            Ctx.agent.ResetPath();
            SwitchState(Factory.Eat());
        }

        if ((Vector3.Distance(Ctx.eatingPoint.position, Ctx.agent.destination) < Ctx.stopProximity || Ctx.agent.remainingDistance <= Ctx.stopProximity) && !Ctx.agent.pathPending && Ctx.agent.hasPath)
        {
            Ctx.movingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            Ctx.agent.ResetPath();
            SwitchState(Factory.Eat());
        }


        /*if ((Ctx.agent.remainingDistance <= Ctx.stopProximity) && !Ctx.agent.pathPending && Ctx.agent.hasPath)
        {
            Ctx.agent.ResetPath();
            SwitchState(Factory.Eat());
        }*/
    }

    public override NPCStates ReturnStateName()
    {
        return NPCStates.Walk;
    }
}
