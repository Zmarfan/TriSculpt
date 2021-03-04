using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayMesh : MonoBehaviour
{
    MeshFilter _meshFilter;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }

    public void DisplayMeshNow(Mesh mesh)
    {
        _meshFilter.mesh = mesh;
    }
}
