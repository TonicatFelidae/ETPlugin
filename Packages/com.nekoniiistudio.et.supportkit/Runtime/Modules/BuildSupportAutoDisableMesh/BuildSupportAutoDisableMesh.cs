using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class BuildSupportAutoDisableMesh : MonoBehaviour
{
    public bool autoDisableMeshOnAwake;
    public bool autoDisableThis;
    private MeshRenderer _meshRenderer;
    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        if (autoDisableMeshOnAwake) _meshRenderer.enabled = false;
    }
}
