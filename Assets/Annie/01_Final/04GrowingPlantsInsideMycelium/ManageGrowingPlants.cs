using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageGrowingPlants : MonoBehaviour
{
    private bool playAnimation = true;

    public bool growingPlant1 = false;
    public bool growingPlant2 = false;
    public bool growingPlant3 = false;
    public bool growingPlant4 = false;
    public bool growingPlant5 = false;

    private float PlantTransitionSpeed = 150f;

    //plant1 shape keys
    private float StemsStraighten = 100f;
    private float StemsGrow = 100f;
    private float LeavesGrow = 100f;
    private float LeavesBounceDown = 100f;
    private float LeavesBounceUp = 100f;
    private bool Plant1BounceDown = true;
    private bool Plant1BounceUp = false;

    //plant2 shape keys
    private float stemsGrow2 = 100f;
    private float growHeads2 = 100f;
    private float wiggle2 = 100f;

    //plant3 shape keys
    private float stemsGrow3 = 100f;
    private float bounceUp3 = 100f;
    private float bounceDown3 = 100f;
    private bool Plant3BounceDown = true;
    private bool Plant3BounceUp = false;

    //plant4 shape keys
    private float stemsGrow4 = 100f;
    private float leavesGrowWide = 100f;
    private float leavesGrowSlim = 100f;
    private bool leaveGrowSlimDone = false;

    //plant5 shape keys
    private float stemsGrow5 = 100f;
    private float bounceUp5 = 0f;
    private float bounceDown5 = 0f;
    private bool Plant5BounceDown = true;
    private bool Plant5BounceUp = false;
    private bool plant5DoBounce = true;

    private SkinnedMeshRenderer meshRend;

    void Start()
    {
        meshRend = GetComponent<SkinnedMeshRenderer>();

        //plant1
        LeavesBounceDown = 0f;
        LeavesBounceUp = 0f;

        //plant3
        bounceUp3 = 0f;
        bounceDown3 = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (growingPlant1 == true && playAnimation == true)
        {
            GrowingPlant1();
        }

        if (growingPlant2 == true && playAnimation == true)
        {
            GrowingPlant2();
        }

        if (growingPlant3 == true && playAnimation == true)
        {
            GrowingPlant3();
        }

        if (growingPlant4 == true && playAnimation == true)
        {
            GrowingPlant4();
        }
        if (growingPlant5 == true && playAnimation == true)
        {
            GrowingPlant5();
        }
    }
    private void GrowingPlant1()
    {
        //Assign blend shapes to floats
        meshRend.SetBlendShapeWeight(0, StemsStraighten);
        meshRend.SetBlendShapeWeight(1, StemsGrow);
        meshRend.SetBlendShapeWeight(2, LeavesGrow);
        meshRend.SetBlendShapeWeight(3, LeavesBounceDown);
        meshRend.SetBlendShapeWeight(4, LeavesBounceUp);

        //make sure function doesn't get called again when animation cycle over
        if (LeavesBounceUp <= 0f && Plant1BounceUp == false && Plant1BounceDown == false)
        {
            playAnimation = false;
        }

        //grow stems
        if (StemsGrow > 22.5f)
        {
            StemsGrow -= PlantTransitionSpeed;
        }
        if (StemsGrow <= 22.5f && StemsGrow > 0f)
        {
            StemsGrow -= PlantTransitionSpeed * 0.25f * Time.deltaTime;
        }
        if (StemsGrow < 0f)
        {
            StemsGrow = 0f;
        }
        //straighten
        if (StemsStraighten > 22.5f)
        {
            StemsStraighten -= PlantTransitionSpeed;
        }
        if (StemsStraighten <= 22.5f && StemsStraighten > 0f)
        {
            StemsStraighten -= PlantTransitionSpeed * 0.25f * Time.deltaTime;
        }
        if (StemsStraighten < 0f)
        {
            StemsStraighten = 0f;
        }

        //grow leaves
        if (StemsGrow <= 22.5f && StemsStraighten <= 22.5f && LeavesGrow > 0f)
        {
            LeavesGrow -= PlantTransitionSpeed * Time.deltaTime;
        }
        if (LeavesGrow < 0f)
        {
            LeavesGrow = 0f;
        }

        //bounce leaves after growing -----------------------------------------------------------------
        // bounce down
        if (LeavesGrow == 0f && Plant1BounceDown == true)
        {
            if (LeavesBounceDown < 50f)
            {
                LeavesBounceDown += PlantTransitionSpeed * 1.7f * Time.deltaTime;
            }
            if (LeavesBounceDown >= 50f)
            {
                Plant1BounceDown = false;
                Plant1BounceUp = true;
            }

        }
        //revert bounce down
        if (Plant1BounceDown == false && Plant1BounceUp == true && LeavesBounceDown > 0f)
        {
            LeavesBounceDown -= PlantTransitionSpeed * 2f * Time.deltaTime;
        }
        if (LeavesBounceDown <= 0f)
        {
            LeavesBounceDown = 0f;
        }
        //------------------------------------------------------------------------------------------------
        // bounce up
        if (LeavesGrow == 0f && Plant1BounceUp == true && LeavesBounceDown <= 0f)
        {
            if (LeavesBounceUp < 80f)
            {
                LeavesBounceUp += PlantTransitionSpeed * 5f * Time.deltaTime;
            }
            if (LeavesBounceUp >= 80f)
            {
                Plant1BounceUp = false;
            }

        }
        //revert bounce up
        if (Plant1BounceUp == false && Plant1BounceDown == false && LeavesBounceUp > 0f)
        {
            LeavesBounceUp -= PlantTransitionSpeed * 2f * Time.deltaTime;
        }
        if (LeavesBounceUp <= 0f)
        {
            LeavesBounceUp = 0f;
        }
        //END bounce leaves after growing --------------------------------------------------------------
    }

    private void GrowingPlant2()
    {
        //Assign blend shapes to floats
        meshRend.SetBlendShapeWeight(0, stemsGrow2);
        meshRend.SetBlendShapeWeight(1, growHeads2);
        meshRend.SetBlendShapeWeight(2, wiggle2);

        //make sure function doesn't get called again when animation cycle over
        if (growHeads2 <= 0f)
        {
            playAnimation = false;
        }

        //grow stems
        if (stemsGrow2 > 0f)
        {
            stemsGrow2 -= PlantTransitionSpeed * 0.5f * Time.deltaTime;
        }
        if (stemsGrow2 < 0f)
        {
            stemsGrow2 = 0f;
        }

        //grow heads as stems are done 
        if (stemsGrow2 <= 0f)
        {
            growHeads2 -= PlantTransitionSpeed * 0.5f * Time.deltaTime;
        }
        if (growHeads2 < 0f)
        {
            growHeads2 = 0f;
        }
    }

    private void GrowingPlant3()
    {
        //Assign blend shapes to floats
        meshRend.SetBlendShapeWeight(0, stemsGrow3);
        meshRend.SetBlendShapeWeight(1, bounceUp3);
        meshRend.SetBlendShapeWeight(2, bounceDown3);

        //make sure function doesn't get called again when animation cycle over
        if (bounceUp3 <= 0f && Plant3BounceUp == false && Plant3BounceDown == false)
        {
            playAnimation = false;
        }

        //grow stems
        if (stemsGrow3 > 0f)
        {
            stemsGrow3 -= PlantTransitionSpeed * 0.5f * Time.deltaTime;
        }
        if (stemsGrow3 < 0f)
        {
            stemsGrow3 = 0f;
        }

        // bounce down
        if (stemsGrow3 == 0f && Plant3BounceDown == true)
        {
            if (bounceDown3 < 30f)
            {
                bounceDown3 += PlantTransitionSpeed * 0.7f * Time.deltaTime;
            }
            if (bounceDown3 >= 30f)
            {
                Plant3BounceDown = false;
                Plant3BounceUp = true;
            }
        }
        if (bounceDown3 > 0f && Plant3BounceDown == false)
        {
            bounceDown3 -= PlantTransitionSpeed * 0.6f * Time.deltaTime;
        }
        if (bounceDown3 < 0f)
        {
            bounceDown3 = 0f;
        }

        // bounce up
        if (stemsGrow3 == 0f && Plant3BounceDown == false && Plant3BounceUp == true)
        {
            if (bounceUp3 < 30f)
            {
                bounceUp3 += PlantTransitionSpeed * 0.7f * Time.deltaTime;
            }
            if (bounceUp3 >= 30f)
            {
                Plant3BounceUp = false;
            }
        }
        if (bounceUp3 > 0f && Plant3BounceUp == false && Plant3BounceDown == false)
        {
            bounceUp3 -= PlantTransitionSpeed * 0.6f * Time.deltaTime;
        }
        if (bounceDown3 < 0f)
        {
            bounceUp3 = 0f;
        }
    }

    private void GrowingPlant4()
    {
        //Assign blend shapes to floats
        meshRend.SetBlendShapeWeight(0, stemsGrow4);
        meshRend.SetBlendShapeWeight(1, leavesGrowSlim);
        meshRend.SetBlendShapeWeight(2, leavesGrowWide);

        //make sure function doesn't get called again when animation cycle over
        if (leavesGrowWide <= 0f && leaveGrowSlimDone == true)
        {
            playAnimation = false;
        }

        //grow stems
        if (stemsGrow4 > 0f)
        {
            stemsGrow4 -= PlantTransitionSpeed * Time.deltaTime;
        }
        if (stemsGrow4 < 0f)
        {
            stemsGrow4 = 0f;
        }

        //grow leaves slim
        if (stemsGrow4 <= 10f && leaveGrowSlimDone == false)
        {
            leavesGrowSlim -= PlantTransitionSpeed * 0.8f * Time.deltaTime;
        }
        if (leavesGrowSlim < 0f)
        {
            leavesGrowSlim = 0f;
        }
        if (leavesGrowSlim <= 0f && leaveGrowSlimDone == false)
        {
            leaveGrowSlimDone = true;
        }

        //grow leaves wide
        if (leavesGrowSlim <= 0f && leavesGrowWide > 0f && leaveGrowSlimDone == true)
        {
            leavesGrowWide -= PlantTransitionSpeed * 0.4f * Time.deltaTime;
        }
        if (leavesGrowWide < 0f)
        {
            leavesGrowWide = 0f;
        }
    }

    private void GrowingPlant5()
    {
        //Assign blend shapes to floats
        meshRend.SetBlendShapeWeight(0, stemsGrow5);
        meshRend.SetBlendShapeWeight(1, bounceUp5);
        meshRend.SetBlendShapeWeight(2, bounceDown5);

        //make sure function doesn't get called again when animation cycle over
        if (bounceUp5 <= 0f && bounceDown5 <= 0f && Plant5BounceUp == false && Plant5BounceDown == false && plant5DoBounce == false)
        {
            playAnimation = false;
        }

        //grow stems
        if (stemsGrow5 > 0f)
        {
            stemsGrow5 -= PlantTransitionSpeed * 1f * Time.deltaTime;
        }
        if (stemsGrow5 < 0f)
        {
            stemsGrow5 = 0f;
        }
        if (stemsGrow5 <= 10f && plant5DoBounce == true)
        {
            Plant5BounceUp = true;
            Plant5BounceDown = false;
        }

        //bounce up 
        if (stemsGrow5 <= 10f && Plant5BounceUp == true)
        {
            if (bounceUp5 < 100f)
            {
                bounceUp5 += PlantTransitionSpeed * 3f * Time.deltaTime;
            }
            if (bounceUp5 >= 100f)
            {
                Plant5BounceUp = false;
                plant5DoBounce = false;
            }
        }
        if (bounceUp5 > 0f && Plant5BounceUp == false && stemsGrow5 == 0f)
        {
            bounceUp5 -= PlantTransitionSpeed *3f * Time.deltaTime;
        }
        if (bounceUp5 < 0f)
        {
            bounceUp5 = 0f;
        }
        if (bounceUp5 <= 0f && Plant5BounceUp == false && stemsGrow5 == 0f && bounceDown5 <= 0f)
        {
            Plant5BounceDown = true;
        }

        //bounce down
        if (stemsGrow5 == 0f && bounceUp5 <= 0f && Plant5BounceDown == true)
        {
            if (bounceDown5 < 100f)
            {
                bounceDown5 += PlantTransitionSpeed * 3f * Time.deltaTime;
            }
            if (bounceDown5 >= 100f)
            {
                Plant5BounceDown = false;
            }
        }
        if (bounceDown5 > 0f && Plant5BounceDown == false && bounceUp5 == 0f && stemsGrow5 == 0f)
        {
            bounceDown5 -= PlantTransitionSpeed * 3f * Time.deltaTime;
        }
        if (bounceDown5 < 0f)
        {
            bounceDown5 = 0f;
        }

    }
}
