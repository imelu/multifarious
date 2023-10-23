using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllMyceliumShader : MonoBehaviour
{
    private Renderer rend;
    private Material myceliumMaterial;

    //shader references
    private Color insideColor;
    private Color outsideColor;
    private Color backgroundExplorationColor;
    private float bumpScale;

    private float lerp = 0f;
    public float lerpSpeed = 0.1f;
    private Color storeColor;

    private bool lerpColors = false;

    void Start()
    {
        //Renderer and material
        rend = this.GetComponent<Renderer>();
        myceliumMaterial = rend.material;

        // shader value references
        insideColor = myceliumMaterial.GetVector("_BgCol");
        outsideColor = myceliumMaterial.GetVector("_OutCol");
        bumpScale = myceliumMaterial.GetFloat("_OutsideBumpScale");
        storeColor = backgroundExplorationColor;
        myceliumMaterial.SetVector("_BgColExploration", insideColor);

    }

    void Update()
    {
        if (Input.GetKeyDown("k"))
        {
            lerpColors = true;
        }
        if (Input.GetKeyDown("l"))
        {
            lerpColors = false;
        }

        if (lerpColors == true)
        {
            // make outlines fat and blue
            backgroundExplorationColor = Color.Lerp(backgroundExplorationColor, outsideColor, lerp);
            myceliumMaterial.SetVector("_BgColExploration", backgroundExplorationColor); 
            storeColor = myceliumMaterial.GetVector("_BgColExploration");

            //rend.material.color = storeColor;
            Debug.Log("ddsmcldskmcls " + backgroundExplorationColor);

            if (backgroundExplorationColor != outsideColor)
            {
                lerp += lerpSpeed;
            }
        }
            /*
            myceliumMaterial.SetVector("_OutFungiCol", fadeColor);
            myceliumMaterial.SetFloat("_OutsideBumpScale", newBumpScale);
            */
        //revert width and color to original
        if (Input.GetKeyDown("l"))
        {
            myceliumMaterial.SetVector("_OutFungiCol", insideColor);
            myceliumMaterial.SetFloat("_OutsideBumpScale", bumpScale);
        }
    }

    /*
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Scanbox")
        {
            HighlightColor();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Scanbox")
        {
            RevertHighlightColor();
        }
    }

    private void HighlightColor()
    {
        myceliumMaterial.SetVector("_OutlineColor", fadeColor);
        myceliumMaterial.SetFloat("_OutlineWidth", newBumpScale);
    }

    private void RevertHighlightColor()
    {
        myceliumMaterial.SetVector("_OutlineColor", originalColor);
        myceliumMaterial.SetFloat("_OutlineWidth", bumpScale);
    }
    */
}
