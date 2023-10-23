using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    resource,
    upgrade
}

public interface INode
{
    public ConnectParticle connectParticle { get; set; }
    public VineParticle vineParticle { get; set; }
    public NodeType type { get; }
    public List<Vector2> paintPositions { get; set; }
    public bool wasDiscovered { get; }
    public void PlayUpgradeNodeSound();
}
