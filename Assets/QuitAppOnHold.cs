using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitAppOnHold : MonoBehaviour
{
    private bool _start;
    private bool _select;

    private void OnStart()
    {
        _start = true;
        if (_start && _select)
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                Application.Quit();
            }
            else
            {
                SceneManager.LoadScene(2);
            }
        }
    }

    private void OnSelect()
    {
        _select = true;
        if (_start && _select)
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                Application.Quit();
            }
            else
            {
                SceneManager.LoadScene(2);
            }
        }
    }


    private void OnStartRelease()
    {
        _start = false;
    }

    private void OnSelectRelease()
    {
        _select = false;
    }
}
