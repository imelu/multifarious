using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Resource", menuName = "ScriptableObjects/Resource", order = 1)]
public class Resource : ScriptableObject
{
    public string resourceName;

    public Sprite resourceTypeSprite;
    public Sprite resourceInfoSprite;

    [TextArea(10, 15)] public string description;
}