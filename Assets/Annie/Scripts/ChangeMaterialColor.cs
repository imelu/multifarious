using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMaterialColor : MonoBehaviour
{
    private Color black;
    private Color white;
    private Color storeColor;
    public bool changeToBlack = false;
    private Renderer rend;
    private Material mat;
    private Color[] colorArray;
    private int currentColorIndex = 0;
    private int targetColorIndex = 1;
    private float targetPoint;
    private float time;

    void Start()
    {
        black = new Color(0f,0f,0f);
        white = new Color(0.9f, 0.9f, 0.9f);
        rend = this.GetComponent<Renderer>();
        mat = rend.material;
    }

    void Update()
    {
        if (changeToBlack == true)
        {
            storeColor = black;
            Transition();
        }
        if (changeToBlack == false)
        {
            storeColor = white;
            rend.material.SetColor("_Color", storeColor);
        }
    }

    private void Transition()
    {
        targetPoint += Time.deltaTime / time;
        mat.color = Color.Lerp(colorArray[currentColorIndex], colorArray[targetColorIndex], targetPoint);
        if (targetPoint >= 1f)
        {
            targetPoint = 0f;
            currentColorIndex = targetColorIndex;
            targetColorIndex++;
            if (targetColorIndex == colorArray.Length)
            {
                targetColorIndex = 0;
            }
        }
    }
}
