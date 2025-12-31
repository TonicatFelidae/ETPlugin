using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Auto destroy after set of time
/// </summary>
public class SC_AutoDisable : MonoBehaviour
{
    public float time;
    public void Start()
    {
        if (time <= 0) TouchDisable();  
    }
    public void Update()
    {
        if (time > 0 ) {
            time -= Time.deltaTime;
        }
        else
        {
            TouchDisable();
        }
    }
    public void TouchDisable()
    {
        gameObject .SetActive(false);
    }
}
