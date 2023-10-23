using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DepthOfFieldBehaviour : MonoBehaviour
{
    //Camera and Volume
    public GameObject cam;
    private DepthOfField dof;
    public VolumeProfile volume;
    public float maxFocalDistance = 45f;
    public float focusSpeed = 5f;
    public float noHitFocusDistance = 25f;

    //Raycast
    Ray raycast;
    RaycastHit hit;
    bool isHit;
    float hitDistance;

    [SerializeField] private LayerMask rayMask;

    void Start()
    {
        // volume settings
        volume.TryGet<DepthOfField>(out dof);
    }
    void Update()
    {
        //raycast
        raycast = new Ray(transform.position, transform.forward * maxFocalDistance);

        isHit = false;

        if (Physics.Raycast(raycast, out hit, maxFocalDistance, rayMask))
        {
            isHit = true;
            hitDistance = Vector3.Distance(transform.position, hit.point);
        }
        else
        {
            if (hitDistance < maxFocalDistance)
            {
                hitDistance++;
            }
        }
        //camera focus
        SetFocus();
    }

    //Raycast
    private void OnDrawGizmos()
    {
        if (isHit)
        {
            Gizmos.DrawSphere(hit.point, 0.05f);
            Debug.DrawRay(transform.position, transform.forward * Vector3.Distance(transform.position, hit.point));
        }
        else
        {
            Debug.DrawRay(transform.position, transform.forward * maxFocalDistance);
        }
    }

    //set camera focus
    public void SetFocus()
    {
        //dof.focusDistance.value = hitDistance;

        if (hitDistance < noHitFocusDistance)
        {
            dof.focusDistance.value = Mathf.Lerp(dof.focusDistance.value, hitDistance, Time.deltaTime * focusSpeed);
        }
        if (hitDistance >= noHitFocusDistance)
        {
            dof.focusDistance.value = noHitFocusDistance;
        }

    }
}
