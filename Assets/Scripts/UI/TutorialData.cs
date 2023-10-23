using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct TutorialStep
{
    public TutorialAction action;
    public string upperText;
    public string lowerText;
}

public enum TutorialAction
{
    findResource,
    connectResource,
    findUpgrade,
    connectUpgrade,
    chooseUpgrade,
    returnToBase,
    activateUpgrade
}

[CreateAssetMenu(fileName = "TutorialData", menuName = "ScriptableObjects/TutorialData", order = 1)]
public class TutorialData : ScriptableObject
{
    public List<TutorialStep> steps = new List<TutorialStep>();
}
