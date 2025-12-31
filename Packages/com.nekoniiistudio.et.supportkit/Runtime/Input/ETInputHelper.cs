using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ETInputHelper : MonoBehaviour
{
    public static ETInputHelper Instance;

    TouchPhase _touchPhase = TouchPhase.Ended;

    private void Awake()
    {
        Instance = this;
    }

    public TouchPhase GetTouchPhase()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return _touchPhase;
#else
        if(Input.touchCount > 0)
            return Input.GetTouch(0).phase;
        else
            return TouchPhase.Canceled;
#endif
    }

    public Vector2 GetTouchPosition()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Input.mousePosition;
#else
        return Input.GetTouch(0).position;
#endif
    }

#if UNITY_EDITOR || UNITY_STANDALONE
    void Update()
    {
        if (_touchPhase == TouchPhase.Ended)
        {
            if (Input.GetMouseButton(0))
                _touchPhase = TouchPhase.Began;
        }
        else if (_touchPhase == TouchPhase.Began)
        {
            if (Input.GetMouseButton(0))
                _touchPhase = TouchPhase.Moved;
        }
        else if (_touchPhase == TouchPhase.Moved)
        {
            if (!Input.GetMouseButton(0))
                _touchPhase = TouchPhase.Ended;
        }
    }
#endif
}

