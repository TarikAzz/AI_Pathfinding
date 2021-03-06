﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


//From: https://blog.nobel-joergensen.com/2010/12/25/procedural-generated-mesh-in-unity/
public class TriangularPrism : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter not found!");
            return;
        }

        Vector3 p0 = new Vector3(0, 0, 0);
        Vector3 p1 = new Vector3(1, 0, 0);
        Vector3 p2 = new Vector3(0.5f, 0, Mathf.Sqrt(0.75f));
        Vector3 p3 = new Vector3(0.5f, Mathf.Sqrt(0.75f), Mathf.Sqrt(0.75f) / 3);

        // Mesh mesh = meshFilter.sharedMesh;
        Mesh mesh = meshFilter.mesh;
        mesh.Clear();

        mesh.vertices = new Vector3[]
        {
    p0,p1,p2,
    p0,p2,p3,
    p2,p1,p3,
    p0,p3,p1
};
        mesh.triangles = new int[]
        {
    0,1,2,
    3,4,5,
    6,7,8,
    9,10,11
};

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        //MeshUtility.Optimize(mesh);

    }

    // Update is called once per frame
    void Update()
    {

    }
}
