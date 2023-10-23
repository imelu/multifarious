using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public interface IUpgrade
{
    public void EnableUpgrade(int index, PlayerStateManager playerIndex);
    public void UpdateUpgrade(int index, PlayerStateManager playerIndex);
    public void DisableUpgrade(int index, PlayerStateManager playerIndex);
}
