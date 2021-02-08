using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Edge class.
/// Each node hold an edge to another.
/// </summary>
public class Edge
{
   /// <summary>
   /// The path of the edge.
   /// </summary>
    public float CostEdge { get; set; }

    /// <summary>
    /// The node connected by the edge.
    /// </summary>
    public GameObject NodeConnected { get; set; }
}
