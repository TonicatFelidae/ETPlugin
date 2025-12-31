using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Auto destroy after set of time
/// </summary>
public class SC_AutoDestroy : MonoBehaviour
{
    public float time;
    public void Start()
    {
        if (time <= 0) TouchDestroy();
    }
    public void Update()
    {
        if (time > 0 ) {
            time -= Time.deltaTime;
        }
        else
        {
            TouchDestroy();
        }
    }
    public void  TouchDestroy()
    {
        Destroy(gameObject);

    }
}
