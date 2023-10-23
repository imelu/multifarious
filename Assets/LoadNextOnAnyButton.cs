using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;

public class LoadNextOnAnyButton : MonoBehaviour
{

    private IDisposable _anyButtonListener;

    private void OnDisable()
    {
        if (_anyButtonListener != null) _anyButtonListener.Dispose();
    }

    private void Start()
    {
        _anyButtonListener = InputSystem.onAnyButtonPress.Call(ctrl => OnButtonPressed());
        //SceneManager.LoadScene(0);
        Debug.Log("entered empty scene");
    }

    // Update is called once per frame
    void Update()
    {
        if (Gamepad.all.Count > 0)
        {
            if (Gamepad.current.leftStick.ReadValue().x > 0.1f || Gamepad.current.leftStick.ReadValue().y > 0.1f) OnButtonPressed();
            if (Gamepad.current.rightStick.ReadValue().x > 0.1f || Gamepad.current.rightStick.ReadValue().y > 0.1f) OnButtonPressed();
        }
    }

    private void OnButtonPressed()
    {
        if (_anyButtonListener != null) _anyButtonListener.Dispose();
        Invoke("LoadMenu", 0.25f);
    }

    private void LoadMenu()
    {
        SceneManager.LoadScene(0);
    }


}
