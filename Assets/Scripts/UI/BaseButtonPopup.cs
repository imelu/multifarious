using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseButtonPopup : ButtonUIPopup
{
    
    public void EnableBaseDefenseButton()
    {
        _buttons[1].gameObject.SetActive(true);
    }


    public void DisableBaseDefenseButton()
    {
        _buttons[1].gameObject.SetActive(false);
    }
}
