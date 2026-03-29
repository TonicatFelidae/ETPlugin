using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Auto destroy after set of time
/// </summary>
public class SC_AutoDisable : MonoBehaviour
{
    public float time;
    public float _time;
    public UnityEvent onEnableEvent { get; set; } = new();
    public UnityEvent onDisableEvent { get; set; } = new();
    public void OnEnable()
    {
        _time = time;
        onEnableEvent.Invoke();
        if (time <= 0) TouchDisable();
    }
    public void Update()
    {
        if (time > 0)
        {
            time -= Time.deltaTime;
        }
        else
        {
            TouchDisable();
        }
    }
    public void TouchDisable()
    {
        gameObject.SetActive(false);
        onDisableEvent.Invoke();
    }
}
