using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMenu
{
    public void Left();
    public void Right();
    public void Up();
    public void Down();
    public void Confirm();
    public void Cancel();
}
