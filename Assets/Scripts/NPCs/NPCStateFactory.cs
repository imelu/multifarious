using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStateFactory
{
    private NPCAIStateManager _ctx;
    public NPCStateFactory(NPCAIStateManager currentContext)
    {
        _ctx = currentContext;
    }

    #region general

    public NPCIdleState Idle()
    {
        return new NPCIdleState(_ctx, this);
    }

    public NPCWalkState Walk()
    {
        return new NPCWalkState(_ctx, this);
    }

    public NPCEatState Eat()
    {
        return new NPCEatState(_ctx, this);
    }

    public NPCDeadState Dead()
    {
        return new NPCDeadState(_ctx, this);
    }

    #endregion
}
