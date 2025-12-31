using ET;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_AutoRandomRotation : MonoBehaviour
{
    Rigidbody rb;
    public ETAxis axis;
    public bool isUseRandom = true;
    [Header("Random value")]
    public float minIncluded;
    public float maxIncluded;
    [Header("Define value")]
    public float value;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        ApplyForceSetting();
    }
    public void ApplyForceSetting()
    {

        Vector3 randomRotation = new();
        float localValue = value;
        if (isUseRandom) { localValue = Random.Range(minIncluded, maxIncluded); }
        // Apply a random rotation
        switch (axis)
        {
            case ETAxis.X:
                randomRotation = new Vector3(localValue, 0, 0);
                break;
            case ETAxis.Y:
                randomRotation = new Vector3(0, localValue, 0);
                break;
            case ETAxis.Z:
                randomRotation = new Vector3(0, 0, localValue);
                break;
            case ETAxis.XY:
                randomRotation = new Vector3(localValue, localValue, 0);
                break;
            case ETAxis.YZ:
                randomRotation = new Vector3(0, localValue, localValue);
                break;
            case ETAxis.XZ:
                randomRotation = new Vector3(localValue, 0, localValue);
                break;
            case ETAxis.XYZ:
                randomRotation = new Vector3(localValue, localValue, localValue);
                break;
        }
        rb.angularVelocity = randomRotation;
    }
}
