using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateFactory
{
    private PlayerStateManager _ctx;
    public PlayerStateFactory(PlayerStateManager currentContext)
    {
        _ctx = currentContext;
    }
    
    public PlayerIdleState Idle()
    {
        return new PlayerIdleState(_ctx, this);
    }
    
    public PlayerWalkState Walk()
    {
        return new PlayerWalkState(_ctx, this);
    }

    public PlayerUIState UI()
    {
        return new PlayerUIState(_ctx, this);
    }

    public PlayerDefenseState Defense()
    {
        return new PlayerDefenseState(_ctx, this);
    }

    public PlayerProjectileState Projectile()
    {
        return new PlayerProjectileState(_ctx, this);
    }

    public PlayerBlockedState Blocked()
    {
        return new PlayerBlockedState(_ctx, this);
    }
}
