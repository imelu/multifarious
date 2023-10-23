using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameIntroManager : MonoBehaviour
{
    private int _particleSystemsDestroyed;
    public int particleSystemsDestroyed { get { return _particleSystemsDestroyed; } 
        set 
        {
            _particleSystemsDestroyed = value;
            if(_particleSystemsDestroyed == 2)
            {
                GlobalGameManager.Instance.StartGame(_tutorialCam);
            }
        } 
    }

    [SerializeField] private Camera _tutorialCam;

    public void StopIntro()
    {
        _tutorialCam.enabled = false;
    }
}
