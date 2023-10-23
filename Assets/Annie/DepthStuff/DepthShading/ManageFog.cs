using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ManageFog : MonoBehaviour
{
    //fog stuff
    public float fogDensityInBase = 0.03f;
    public float fogDensityOutOfBase = 1;
    public float lerpSpeed = 0.3f;
    private float fogDensity;

    private Vector2 minMaxDis;
    private Vector3 playerPos;
    private Vector3 basePos;
    private float distancePlayerToBase;

    //cam and volume
    public GameObject cam;
    public VolumeProfile volume;
    //reference to basTexManager script for onBase 
    public BaseTexManager baseTexManagerScript;

    private void Start()
    {
        fogDensity = RenderSettings.fogDensity;
        minMaxDis = new Vector2(fogDensityInBase, fogDensityOutOfBase);
    }
    void Update()
    {
        // calculate distance between player and base
        distancePlayerToBase = Vector3.Distance(basePos, playerPos);
        //clamp with min and max fog values
        float newDistancePlayerToBase = Mathf.Clamp(distancePlayerToBase, fogDensityInBase, fogDensityOutOfBase);


        //in base > make fog 0.03
        if (baseTexManagerScript.OnBase(transform.position))
        {
            //lerp if fog number bigger than standard inside fog number
            if (fogDensity > fogDensityInBase)
            {
                fogDensity = fogDensity - lerpSpeed;
                RenderSettings.fogDensity = fogDensity;
            }
            //if inside Fog value reached stop lerping
            if (fogDensity <= fogDensityInBase)
            {
                fogDensity = fogDensityInBase;
                RenderSettings.fogDensity = fogDensity;
            }
        }
        // outside of base > make fog 1 
        if (!baseTexManagerScript.OnBase(transform.position))
        {
            /*
            fogDensity = newDistancePlayerToBase;
            RenderSettings.fogDensity = fogDensity;
            */

            //lerp if fog number smaller than standard outside fog number
            if (fogDensity < fogDensityOutOfBase)
            {
                fogDensity = fogDensity + lerpSpeed;
                RenderSettings.fogDensity = fogDensity;
            }
            //if outside Fog value reached stop lerping
            if (fogDensity >= fogDensityOutOfBase)
            {
                fogDensity = 1f;
                RenderSettings.fogDensity = fogDensity;
            }
        }
    }
}
