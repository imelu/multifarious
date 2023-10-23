using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ManageAirDistortion : MonoBehaviour
{
    [Header("Air Distortion")]
    [SerializeField] private Renderer airDis;
    [SerializeField] private GameObject airDisOBJ;
    [SerializeField] private float transitionDurationMagnification;
    [SerializeField] private float transitionDurationAlpha;
    private float initialValueMagnification;
    private float initialValueAlpha = 1f;
 

    private void Start()
    {
        initialValueMagnification = airDis.material.GetFloat("_Magnification");
        initialValueAlpha = airDis.material.GetFloat("_alpha");
        changeDisValueMag();
        StartCoroutine(AwaitMag());
        changeDisValueAlpha();
    }
    private void Update()
    {

        if (airDis.material.GetFloat("_alpha") <= 0f)
        {
            Destroy(airDisOBJ);
            Destroy(this);
        }
    }
    private void changeDisValueMag()
    {
        airDis.material.DOFloat(0, "_Magnification", transitionDurationMagnification / 2);
    }
    private void changeDisValueAlpha()
    {
        airDis.material.DOFloat(0, "_alpha", transitionDurationAlpha / 2);
    }
    IEnumerator AwaitMag()
    {
        yield return new WaitForSeconds(2);

    }
}
