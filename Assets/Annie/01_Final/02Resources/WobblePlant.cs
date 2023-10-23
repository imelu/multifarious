using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WobblePlant : MonoBehaviour
{
    public bool SugarPlant = false;
    public bool doPlantOnly = false;
    public GameObject plant;

    private Renderer plantRend;
    private Material plantMat;

    private bool doTheWobble = false;
    private bool stopTheWobble = false;
    private int beWobbly = 0;
    public float wobblyTransitionSpeed = 0.5f;

    private float multiplyNoise = 0f;

    public ParticleSystem pS;
    private Renderer pSRend;
    private Material pSMat;
    public Color newParticlesColor;
    private Color originalParticlesColor;
    private Color storeTransitionColor;

    private float lerp = 0f;
    public float lerpSpeed = 0.1f;

    void Start()
    {
        plantRend = plant.GetComponent<Renderer>();
        plantMat = plantRend.material;
        beWobbly = plantMat.GetInt("_BeWobbly");
        plantMat.SetInt("_BeWobbly", beWobbly);
        plantMat.SetFloat("_MultiplyNoise", multiplyNoise);

        // particle System
        pSRend = pS.GetComponent<Renderer>();
        pSMat = pSRend.material;
        originalParticlesColor = pSRend.material.color;
        newParticlesColor = Color.white * 4f;
    }

    void Update()
    {
        //SugarPlant---------------------------------------------------
        if (doTheWobble == true && SugarPlant == true)
        {
            WobblingSugarPlant();
        }
        //SugarPlant---------------------------------------------------
    }

    private void WobblingSugarPlant()
    {
        //set shader bool to true or false depending on player in or out of trigger (shader bools are ints)
        // 1 > do wobble 
        // 0 > don't do wobble

        if (stopTheWobble == false && beWobbly == 0)
        {
            beWobbly = 1; 
        }
        if (stopTheWobble == true && beWobbly == 1)
        {

            if (multiplyNoise > 0f)
            {
                multiplyNoise -= 0.5f * Time.deltaTime;
                plantMat.SetFloat("_MultiplyNoise", multiplyNoise);
            }

            if (multiplyNoise <= 0f)
            {
                beWobbly = 0;
            }
        }
        //if in trigger change particles
        if (beWobbly == 1 & stopTheWobble == false && pSRend.material.color != newParticlesColor)
        {
            if (doPlantOnly == false)
            {
                ChangeParticlesColor();
            }

            if (multiplyNoise < 0.1f)
            {
                multiplyNoise += wobblyTransitionSpeed * Time.deltaTime;
                plantMat.SetFloat("_MultiplyNoise", multiplyNoise);

            }

            if (pSRend.material.color == newParticlesColor)
            {
                lerp = 0f;
            }
        }

        // reset bools and particles color
        if (stopTheWobble == true && beWobbly == 0)
        {
            if (doPlantOnly == false)
            {
                RevertParticlesColor();
            }
            if (pSRend.material.color == originalParticlesColor)
            {
                doTheWobble = false;
                stopTheWobble = false;
            }
        }

        //set bool value
        plantMat.SetInt("_BeWobbly", beWobbly);
    }

    private void ChangeParticlesColor()
    {
        pSRend.material.color = Color.Lerp(storeTransitionColor, newParticlesColor, lerp);

        if (pSRend.material.color != newParticlesColor)
        {
            lerp += lerpSpeed;
        }
    }
    private void RevertParticlesColor()
    {
        pSRend.material.color = Color.Lerp(storeTransitionColor, originalParticlesColor, lerp);

        //rend.material.color = storeColor;
        if (pSRend.material.color != originalParticlesColor)
        {
            lerp += (lerpSpeed * 1f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        stopTheWobble = false;
        doTheWobble = true;

        if (doPlantOnly == false)
        {
            //in case exit transition not done
            if (pSRend.material.color != originalParticlesColor)
            {
                storeTransitionColor = pSRend.material.color;
                lerp = 0f;
            }
            if (pSRend.material.color == originalParticlesColor)
            {
                storeTransitionColor = originalParticlesColor;
                lerp = 0f;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        stopTheWobble = true;

        if (doPlantOnly == false)
        {
            //in case enter transition not done
            if (pSRend.material.color != newParticlesColor)
            {
                storeTransitionColor = pSRend.material.color;
                lerp = 0f;
            }
            if (pSRend.material.color == newParticlesColor)
            {
                storeTransitionColor = newParticlesColor;
                lerp = 0f;
            }
        }
    }
}
