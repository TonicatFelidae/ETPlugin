using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Auto destroy after set of time
/// </summary>
public class SC_AutoDestroy : MonoBehaviour
{
    public float time;
    public UnityEvent onEnableEvent { get; set; } = new();
    public UnityEvent onDisableEvent { get; set; } = new();
    public void Start()
    {
        onEnableEvent.Invoke();
        if (time <= 0) TouchDestroy();
    }
    public void Update()
    {
        if (time > 0)
        {
            time -= Time.deltaTime;
        }
        else
        {
            TouchDestroy();
        }
    }
    public void TouchDestroy()
    {
        onDisableEvent.Invoke();
        Destroy(gameObject);

    }
}
