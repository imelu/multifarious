using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimEvents : MonoBehaviour
{
    public void Decayed()
    {
        Destroy(transform.parent.parent.gameObject);
    }
}
