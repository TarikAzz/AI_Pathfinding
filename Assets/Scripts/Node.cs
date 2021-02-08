using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Node : MonoBehaviour
{

    public GameObject gameManager;

    /// <summary>
    /// Used to determine the starting cost in pathfidning algorithms.
    /// </summary>
    public float[] costStart = new float[3];

    /// <summary>
    /// Used to determine if the NPC has been visited in pathfidning algorithms.
    /// </summary>
    public bool[] hasBeenVisisted = new bool[3];

    /// <summary>
    /// Used to determine the euclidean distance to the goal node in AStar algorithm.
    /// </summary>
    public float[] euclideanDistanceToGoal = new float[3];

    /// <summary>
    /// Used to determine the closest node in pathfidning algorithms.
    /// </summary>
    public GameObject[] nearestToStart = new GameObject[3];

    /// <summary>
    /// The cluster identification number.
    /// </summary>
    public int clusterNum;

    /// <summary>
    /// The cost (distance) of the closest node.
    /// </summary>
    public float costClosest;

    /// <summary>
    /// The cost (distance) of the closest node. 
    /// Used in another method to not conflict with costClosest.
    /// </summary>
    float distanceToBestNode = 0;

    /// <summary>
    /// GameObect holding all nodes.
    /// </summary>
    public GameObject nodeContainer;

    /// <summary>
    /// The closest node to a given node.
    /// </summary>
    public GameObject nodeClosest = null;

    /// <summary>
    /// The list of all nodes a particular node can see (through raycasting).
    /// </summary>
    public List<GameObject> neighbourNodeList = new List<GameObject>();

    /// <summary>
    /// The connections of a particular node.
    /// </summary>
    public List<Edge> connections = new List<Edge>();

    /// <summary>
    /// The directio used to find all neighbours.
    /// </summary>
    Vector3 direction = new Vector3();

    /// <summary>
    /// The UNity Start method.
    /// </summary>
    void Start()
    {
        nodeContainer = GameObject.Find("NodeContainerAll");

        gameManager = GameObject.Find("GameManager");

        //for (int i = 0; i < nodeContainer.transform.childCount; i++)
        //{
        //    totalNodeList.Add(nodeContainer.transform.GetChild(i).gameObject);
        //}

        GetNeighboursAndRayCast();
    }
    
    /// <summary>
    ///  Evalutes connection costs between neighbouring nodes.
    /// </summary>
    public void EvaluateNodeConnections()
    {
        for (int i = 0; i < neighbourNodeList.Count; i++)
        {
            connections.Add(new Edge
            {
                NodeConnected = neighbourNodeList[i],
                CostEdge = Vector3.Distance(transform.position, neighbourNodeList[i].transform.position)
            });
        }
    }

    /// <summary>
    /// Finds the best neighbouring node (one with lowest cost).
    /// </summary>
    public void EvaluateClosestNeighbour()
    {
        neighbourNodeList = neighbourNodeList.OrderBy(x => x.GetComponent<Node>().costClosest).ToList();
        nodeClosest = neighbourNodeList[0];
        costClosest = Vector3.Distance(transform.position, neighbourNodeList[0].transform.position);
    }

    /// <summary>
    /// Adds all neighbours and makes a raycast.
    /// </summary>
    void GetNeighboursAndRayCast()
    {
        for (int i = 0; i < gameManager.GetComponent<GameManager>().totalNodeList.Count; i++)
        {
            if (gameObject != gameManager.GetComponent<GameManager>().totalNodeList[i])
            {
                direction = gameManager.GetComponent<GameManager>().totalNodeList[i].transform.position - transform.position;
                distanceToBestNode = Vector3.Distance(transform.position, gameManager.GetComponent<GameManager>().totalNodeList[i].transform.position);

                if (Physics.Raycast(transform.position, direction, distanceToBestNode) == false)
                {
                    neighbourNodeList.Add(gameManager.GetComponent<GameManager>().totalNodeList[i]);
                    //Debug.DrawLine(transform.position, totalNodeList[i].transform.position, Color.green, 9999999f);
                }
            }

        }
    }

    /// <summary>
    /// Gets the cluster number.
    /// </summary>
    /// <returns>The cluster tag number</returns>
    public int getClusterNum()
    {
        clusterNum = int.Parse(gameObject.tag.Substring(gameObject.tag.Length - 1));
        return clusterNum;
    }
}