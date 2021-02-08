using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIMovements : MonoBehaviour
{
    /// <summary>
    /// The game manager.
    /// </summary>
    public GameObject gameManager;

    /// <summary>
    /// The group of clusters.
    /// </summary>
    public GameObject Clusters;

    public Material mat;

    /// <summary>
    /// The start node.
    /// </summary>
    public GameObject startNode;

    /// <summary>
    /// The goal node.
    /// </summary>
    public GameObject goalNode;

    /// <summary>
    /// Value determining if steering movement should be used.
    /// </summary>
    public bool isSteering = false;

    /// <summary>
    /// The NPC's current node.
    /// </summary>
    public GameObject currentNode;

    /// <summary>
    /// The NPC's next node.
    /// </summary>
    public GameObject nextNode;

    /// <summary>
    /// The index of the pathList node currently used.
    /// </summary>
    public int indexer = 0;

    /// <summary>
    ///  Value determining if NPC is a runner.
    /// </summary>
    public bool isRunner = false;

    /// <summary>
    ///  Value determining if NPC is a chaser.
    /// </summary>
    public bool isChaser = false;

    /// <summary>
    /// The list used to retrieve the path from the chosen algorithm.
    /// </summary>
    public List<GameObject> pathList = new List<GameObject>();

    /// <summary>
    /// The list used to retrieve the path from the chosen cluster alorithm. 
    /// </summary>
    public List<GameObject> clusterPathList = new List<GameObject>();

    /// <summary>
    /// The list used wihtin the path finidng algorithms.
    /// </summary>
    public List<GameObject> openListSorted = new List<GameObject>();

    /// <summary>
    /// The look up table used to retrieve the shortest path between clusters.
    /// </summary>
    List<GameObject>[][] lookUpTable;

    /// <summary>
    /// The look up  list populated from the clustesr.
    /// </summary>
    List<GameObject> clusterTable;

    /// <summary>
    /// The velocity vector used in steering movements.
    /// </summary>
    Vector3 velDir = new Vector3();

    /// <summary>
    /// The max speed used in steering.
    /// </summary>
    public float maxSpeed = 10f;

    /// <summary>
    /// The max acceleration used in steering.
    /// </summary>
    float maxAcceleration = 5f;

    /// <summary>
    /// The animator.
    /// </summary>
    Animator animator;


    /// <summary>
    /// The Runner NPC.
    /// </summary>
    GameObject runner;

    /// <summary>
    /// The GameObject holding all lines.
    /// </summary>
    GameObject linesContainer;

    /// <summary>
    /// Turns off the layering of the NPC only once in update.
    /// </summary>
    bool layerOnce = true;

    /// <summary>
    /// Determines if steering should be used.
    /// </summary>
    bool useSteering = true;

    /// <summary>
    /// Object which retrieves information from the raycast.
    /// </summary>
    RaycastHit hitInfo;

    /// <summary>
    /// The other chaser relative to one chaser.
    /// </summary>
    GameObject otherChaser;

    /// <summary>
    /// Determines who the flanker is.
    /// </summary>
    public bool isFlanker = false;

    /// <summary>
    /// The Unity Awake method.
    /// Use this for initialization
    /// </summary>
    void Awake()
    {
        if (isRunner)
        {
            startNode = ClosestNodeCalculate();
            currentNode = startNode;
        }

    }

    /// <summary>
    /// The Unity Start method.
    /// </summary>
    void Start()
    {
        linesContainer = GameObject.Find("PathLineContainer");

        lookUpTable = Clusters.GetComponent<Cluster>().lookUpTable;

        if (gameObject.name == "Runner")
        {
            isRunner = true;
            maxSpeed = 10f;
            //  animator = GetComponent<Animator>();
        }
        else if (gameObject.name.Contains("Chaser"))
        {
            isChaser = true;
            maxSpeed = 10;
            maxAcceleration = 2f;

            runner = GameObject.Find("Runner");

            otherChaser = gameObject.name == "Chaser1" ? GameObject.Find("Chaser2") : GameObject.Find("Chaser1");
        }

        startNode = ClosestNodeCalculate();
        currentNode = startNode;

        SetGoalNode();

    }

    /// <summary>
    /// The Unity Update method.
    /// </summary>
    void Update()
    {
        if (layerOnce)
        {
            gameObject.layer = 0;
            var a = gameObject.layer;
            layerOnce = false;
        }

        // Speed up chaser if close to other chaser. Makes Super Saiyan easier to attain.

        if (isChaser && Physics.Raycast(transform.position, transform.forward, out hitInfo) && hitInfo.transform.name.Contains("Chaser"))
        {
            maxSpeed = 13f;
        }

        // Determines who the flanker is.
        if (isChaser && gameManager.GetComponent<GameManager>().strongFlankToggle == 0)
        {
            GameObject.Find("Chaser1").GetComponent<AIMovements>().isFlanker = false;
            GameObject.Find("Chaser2").GetComponent<AIMovements>().isFlanker = true;
        }
        else if (isChaser && gameManager.GetComponent<GameManager>().strongFlankToggle == 1)
        {
            if (Vector3.Distance(transform.position, GameObject.Find("Runner").transform.position) > Vector3.Distance(otherChaser.transform.position, GameObject.Find("Runner").transform.position))
            {
                isFlanker = true;
                otherChaser.GetComponent<AIMovements>().isFlanker = false;
            }
        }
        else
        {
            isFlanker = false;
        }

        SetGoalNode();

        if (!gameManager.GetComponent<GameManager>().isGameDone)
        {
            if (!gameManager.GetComponent<GameManager>().lockerAI)
            {
                if (!isSteering)
                {
                    ClearStuff();

                    // Swith the pathfinding type.
                    switch (gameManager.GetComponent<GameManager>().pathFindingType)
                    {
                        case 1:
                            pathList = DijkstraPath();
                            break;
                        case 2:
                            //pathList = ClusterPath();
                            pathList = AStartPath();
                            break;
                        case 3:
                            //pathList = ClusterPath();
                            pathList = AStartPath();

                            break;
                    }

                    RemoveLines();

                    for (int i = 0; i < pathList.Count - 1; i++)
                    {
                        if (isRunner)
                        {
                            DrawPathLines(pathList[i].transform.position, pathList[i + 1].transform.position, Color.cyan, Color.green);
                        }
                        else
                        {
                            DrawPathLines(pathList[i].transform.position, pathList[i + 1].transform.position, Color.red, new Color32(226, 111, 11, 255));
                        }
                    }


                    if ((indexer + 1) != pathList.Count)
                    {
                        nextNode = pathList[indexer + 1];
                        isSteering = true;
                    }
                }
                else //if they ARE moving.
                {
                    if (useSteering)
                    {
                        SeekSteering();
                    }
                    else
                    {
                        SeekKinematic();
                    }

                    if (Vector3.Distance(transform.position, nextNode.transform.position) <= 1)
                    {
                        currentNode = nextNode;
                        //GetComponent<Rigidbody>().velocity = Vector3.zero;
                        SetGoalNode();
                        isSteering = false;
                        indexer++;
                    }
                }

                GoSuperSaiyan();
            }
        }

        if (isRunner)
        {
            // animator.SetFloat("Blend", GetComponent<Rigidbody>().velocity.magnitude);
        }
    }

    /// <summary>
    /// Draws the lines on the pathList.
    /// </summary>
    /// <param name="nodeStartPosition">Starting node position</param>
    /// <param name="nodeEndPosition">Ending node position</param>
    /// <param name="colourStart">Start colour</param>
    /// <param name="colourEnd">End colour</param>
    public void DrawPathLines(Vector3 nodeStartPosition, Vector3 nodeEndPosition, Color colourStart, Color colourEnd)
    {
        GameObject lineTemp = new GameObject();
        lineTemp.tag = "PathLine " + gameObject.name;

        lineTemp.name = "PathLine " + gameObject.name;
        lineTemp.transform.parent = linesContainer.transform;

        // Define where it begins.
        lineTemp.transform.position = nodeStartPosition;

        lineTemp.AddComponent<LineRenderer>();
        LineRenderer lineRendererTemp = lineTemp.GetComponent<LineRenderer>();

        //Give material so the start and end node colour works.
        lineRendererTemp.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));

        // Set line parameters.
        lineRendererTemp.startWidth = 0.15f;
        lineRendererTemp.endWidth = 0.15f;
        lineRendererTemp.startColor = colourStart;
        lineRendererTemp.endColor = colourEnd;
        lineRendererTemp.SetPosition(0, nodeStartPosition);
        lineRendererTemp.SetPosition(1, nodeEndPosition);

        // Destroy if it reaches the max time limit (probably won't).
        Destroy(lineTemp, 5f);
    }

    /// <summary>
    /// Removes the drawn lines from the hierarchy.
    /// </summary>
    public void RemoveLines()
    {
        // Get all existing lines
        GameObject[] linesTemp = GameObject.FindGameObjectsWithTag("PathLine " + gameObject.name);

        foreach (GameObject temp in linesTemp)
        {
            Destroy(temp);
        }

    }

    /// <summary>
    /// When the the chasers combine, they go Super Saiyan. 
    /// Movement speed is increased and they get bigger.
    /// </summary>
    void GoSuperSaiyan()
    {
        if (gameObject.name == "Chaser1")
        {
            if (Vector3.Distance(transform.position, GameObject.Find("Chaser2").transform.position) <= 0.5 || Vector3.Distance(GameObject.Find("Chaser2").transform.position, transform.position) <= 0.5)
            {
                //if (gameManager.GetComponent<GameManager>().ssLocker)
                //{
                //    gameManager.GetComponent<GameManager>().isSuperSaiayan = true;
                //    gameManager.GetComponent<GameManager>().ssLocker = false;
                //}

                // Kinematic kicks in and moves without error.
                useSteering = false;

                transform.position = GameObject.Find("Chaser2").transform.position;

                if (!gameManager.GetComponent<GameManager>().robotsToggle)
                {
                    gameObject.transform.localScale = new Vector3(2f, 0.5f, 2f);
                    GameObject.Find("Chaser2").transform.localScale = new Vector3(0.5f, 0.1f, 0.2f);

                    gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                    GameObject.Find("Chaser2").GetComponent<Renderer>().material.color = Color.yellow;

                    maxSpeed = 15f;
                    GameObject.Find("Chaser2").GetComponent<AIMovements>().maxSpeed = 15f;
                }
                else
                {
                    gameObject.transform.localScale = new Vector3(2f, 2f, 2f);
                    GameObject.Find("Chaser2").transform.localScale = new Vector3(2f, 2f, 2f);

                    gameObject.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.yellow;
                    GameObject.Find("Chaser2").transform.GetChild(0).GetComponent<Renderer>().material.color = Color.yellow;

                    maxSpeed = 15f;
                    GameObject.Find("Chaser2").GetComponent<AIMovements>().maxSpeed = 15f;
                }
                gameManager.GetComponent<GameManager>().isSuperSaiayan = false;

            }
            else
            {
                //gameManager.GetComponent<GameManager>().ssLocker = true;
                useSteering = true;

                if (!gameManager.GetComponent<GameManager>().robotsToggle)
                {
                    gameObject.transform.localScale = new Vector3(1f, 0.2f, 1f);
                    GameObject.Find("Chaser2").transform.localScale = new Vector3(1f, 0.2f, 1f);
                    gameObject.GetComponent<Renderer>().material.color = Color.red;
                    GameObject.Find("Chaser2").GetComponent<Renderer>().material.color = Color.red;
                }
                else
                {
                    gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
                    GameObject.Find("Chaser2").transform.localScale = new Vector3(1f, 1f, 1f);
                    gameObject.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.red;
                    GameObject.Find("Chaser2").transform.GetChild(0).GetComponent<Renderer>().material.color = Color.red;
                }

                maxSpeed = 10f;
                GameObject.Find("Chaser2").GetComponent<AIMovements>().maxSpeed = 10f;
            }
        }
    }

    /// <summary>
    /// Sets the goal node for the runner and chasers.
    /// </summary>
    public void SetGoalNode()
    {
        if (isRunner)
        {
            Vector3 centreMass = (GameObject.Find("Chaser1").transform.position + GameObject.Find("Chaser2").transform.position) / 2;
            goalNode = FurthestNodeCalculate(centreMass);
        }
        else if (isChaser)
        {
            // Simpler flanking.
            if (isFlanker)
            {
                if (Vector3.Distance(GameObject.Find("Runner").transform.position, transform.position) > 15)
                {
                    //print(gameObject.name + ": Flanking.");
                    goalNode = GameObject.Find("Runner").GetComponent<AIMovements>().goalNode;
                }
                else
                {
                    //print(gameObject.name + ": Regular.");
                    goalNode = runner.GetComponent<AIMovements>().currentNode;

                    if (goalNode == currentNode) //If the runner is moving, basically, check for the new goalNode. This fixes a bug where the chaser gets stuck.
                    {
                        goalNode = runner.GetComponent<AIMovements>().nextNode;
                    }
                }
            }
            else
            {
                goalNode = runner.GetComponent<AIMovements>().currentNode;

                if (goalNode == currentNode) //If the runner is moving, basically, check for the new goalNode. This fixes a bug where the chaser gets stuck.
                {
                    goalNode = runner.GetComponent<AIMovements>().nextNode;
                }
            }

            #region Old Flanking
            //if(gameManager.GetComponent<GameManager>().strongFlankToggle == 0)
            //{

            //    if (gameObject.name == "Chaser1")
            //    {
            //        goalNode = runner.GetComponent<AIMovements>().currentNode;

            //        if (goalNode == currentNode) //If the runner is moving, basically, check for the new goalNode. This fixes a bug where the chaser gets stuck.
            //        {
            //            goalNode = runner.GetComponent<AIMovements>().nextNode;
            //        }
            //    }
            //    else if (gameObject.name == "Chaser2")
            //    {
            //        if (Vector3.Distance(GameObject.Find("Runner").transform.position, transform.position) <= 15)
            //        {

            //            //print("Regular.");
            //            goalNode = runner.GetComponent<AIMovements>().currentNode;

            //            if (goalNode == currentNode) //If the runner is moving, basically, check for the new goalNode. This fixes a bug where the chaser gets stuck.
            //            {
            //                goalNode = runner.GetComponent<AIMovements>().nextNode;
            //            }
            //        }
            //        else
            //        {
            //            //print("Flanking.");
            //            goalNode = GameObject.Find("Runner").GetComponent<AIMovements>().goalNode;

            //        }
            //    }

            //}
            //else if(gameManager.GetComponent<GameManager>().strongFlankToggle == 1)
            //{

            //    if (isFlanker)
            //    {
            //        if (Vector3.Distance(GameObject.Find("Runner").transform.position, transform.position) > 15)
            //        {
            //            //print(gameObject.name + ": Flanking.");
            //            goalNode = GameObject.Find("Runner").GetComponent<AIMovements>().goalNode;
            //        }
            //        else
            //        {
            //            //print(gameObject.name + ": Regular.");
            //            goalNode = runner.GetComponent<AIMovements>().currentNode;

            //            if (goalNode == currentNode) //If the runner is moving, basically, check for the new goalNode. This fixes a bug where the chaser gets stuck.
            //            {
            //                goalNode = runner.GetComponent<AIMovements>().nextNode;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        //print(gameObject.name + ": Regular.");
            //        goalNode = runner.GetComponent<AIMovements>().currentNode;

            //        if (goalNode == currentNode) //If the runner is moving, basically, check for the new goalNode. This fixes a bug where the chaser gets stuck.
            //        {
            //            goalNode = runner.GetComponent<AIMovements>().nextNode;
            //        }
            //    }
            //}
            //else //Regular
            //{
            //    goalNode = runner.GetComponent<AIMovements>().currentNode;

            //    if (goalNode == currentNode) //If the runner is moving, basically, check for the new goalNode. This fixes a bug where the chaser gets stuck.
            //    {
            //        goalNode = runner.GetComponent<AIMovements>().nextNode;
            //    }
            //}
            #endregion
        }
        #region Old Original

        // Original
        //else if (gameObject.name == "Chaser1")
        //{
        //    goalNode = runner.GetComponent<AIMovements>().currentNode;

        //    if (goalNode == currentNode) //If the runner is moving, basically, check for the new goalNode. This fixes a bug where the chaser gets stuck.
        //    {
        //        goalNode = runner.GetComponent<AIMovements>().nextNode;
        //    }
        //}
        //else if (gameObject.name == "Chaser2")
        //{
        //    if (Vector3.Distance(GameObject.Find("Runner").transform.position, transform.position) <= 15 || !gameManager.GetComponent<GameManager>().flankToggle)
        //    {

        //        //print("Regular.");
        //        goalNode = runner.GetComponent<AIMovements>().currentNode;

        //        if (goalNode == currentNode) //If the runner is moving, basically, check for the new goalNode. This fixes a bug where the chaser gets stuck.
        //        {
        //            goalNode = runner.GetComponent<AIMovements>().nextNode;
        //        }
        //    }
        //    else
        //    {
        //        //print("Flanking.");
        //        goalNode = GameObject.Find("Runner").GetComponent<AIMovements>().goalNode;

        //    }
        //}
        #endregion
    }

    /// <summary>
    /// Gets Furthest node.
    /// The runner uses this to run flee.
    /// </summary>
    /// <param name="centreMass">The average distance between both chasers</param>
    /// <returns></returns>
    public GameObject FurthestNodeCalculate(Vector3 centreMass)
    {
        GameObject nodeTemp = null;
        float dist = 0f;

        for (int i = 0; i < gameManager.GetComponent<GameManager>().totalNodeList.Count; i++)
        {
            float distanceTemp = Vector3.Distance(centreMass, gameManager.GetComponent<GameManager>().totalNodeList[i].transform.position);

            if (distanceTemp > dist)
            {
                dist = distanceTemp;
                nodeTemp = gameManager.GetComponent<GameManager>().totalNodeList[i];
            }
        }

        return nodeTemp;
    }

    /// <summary>
    /// Gets the node that the NPC is sitting on.
    /// </summary>
    /// <returns></returns>
    public GameObject ClosestNodeCalculate()
    {
        float dist = 9999f;
        GameObject nodeTemp = null;

        for (int i = 0; i < gameManager.GetComponent<GameManager>().totalNodeList.Count; i++)
        {
            float distanceTemp = Vector3.Distance(gameManager.GetComponent<GameManager>().totalNodeList[i].transform.position, transform.position);

            if (distanceTemp < dist)
            {
                dist = distanceTemp;
                nodeTemp = gameManager.GetComponent<GameManager>().totalNodeList[i];
            }
        }

        return nodeTemp;
    }

    /// <summary>
    /// Gets the shortest path using Dijkstra's algorithm.
    /// Uses Dijkstra.
    /// </summary>
    /// <returns>The shortest path list</returns>
    public List<GameObject> DijkstraPath()
    {
        DijkstraAlgorithm();
        var pathTemp = new List<GameObject>();
        pathTemp.Add(goalNode);
        CreatePathList(pathTemp, goalNode);
        pathTemp.Reverse();
        return pathTemp;
    }

    /// <summary>
    /// Gets the shortest path using AStar's algorithm.
    /// Uses AStar.
    /// </summary>
    /// <returns>The shortest path list</returns>
    public List<GameObject> AStartPath()
    {
        for (int i = 0; i < gameManager.GetComponent<GameManager>().totalNodeList.Count; i++)
        {
            if (gameObject.name == "Runner")
            {
                gameManager.GetComponent<GameManager>().totalNodeList[i].GetComponent<Node>().euclideanDistanceToGoal[0] = Vector3.Distance(gameManager.GetComponent<GameManager>().totalNodeList[i].transform.position, goalNode.transform.position);
            }
            else if (gameObject.name == "Chaser1")
            {
                gameManager.GetComponent<GameManager>().totalNodeList[i].GetComponent<Node>().euclideanDistanceToGoal[1] = Vector3.Distance(gameManager.GetComponent<GameManager>().totalNodeList[i].transform.position, goalNode.transform.position);
            }
            else if (gameObject.name == "Chaser2")
            {
                gameManager.GetComponent<GameManager>().totalNodeList[i].GetComponent<Node>().euclideanDistanceToGoal[2] = Vector3.Distance(gameManager.GetComponent<GameManager>().totalNodeList[i].transform.position, goalNode.transform.position);
            }
        }

        AStarAlgorithm();
        var pathTemp = new List<GameObject>();
        pathTemp.Add(goalNode);
        CreatePathList(pathTemp, goalNode);
        pathTemp.Reverse();
        return pathTemp;
    }

    /// <summary>
    /// Creates the shortest path recursively for both Dijkstra and A*.
    /// Modifies node values depending on NPC.
    /// </summary>
    /// <param name="list">The passed in list</param>
    /// <param name="node">the goal node</param>
    private void CreatePathList(List<GameObject> list, GameObject node)
    {
        if (gameObject.name == "Chaser2")
        {
            if (node.GetComponent<Node>().nearestToStart[2] == null)
            {
                return;
            }

            list.Add(node.GetComponent<Node>().nearestToStart[2]);
            CreatePathList(list, node.GetComponent<Node>().nearestToStart[2]);
        }

        if (gameObject.name == "Runner")
        {
            if (node.GetComponent<Node>().nearestToStart[0] == null)
            {
                return;
            }

            list.Add(node.GetComponent<Node>().nearestToStart[0]);
            CreatePathList(list, node.GetComponent<Node>().nearestToStart[0]);
        }

        if (gameObject.name == "Chaser1")
        {
            if (node.GetComponent<Node>().nearestToStart[1] == null)
            {
                return;
            }

            list.Add(node.GetComponent<Node>().nearestToStart[1]);
            CreatePathList(list, node.GetComponent<Node>().nearestToStart[1]);
        }

    }

    /// <summary>
    /// The Dijkstra's algorithm.
    /// </summary>
    private void DijkstraAlgorithm()
    {
        if (gameObject.name == "Chaser2")
        {
            currentNode.GetComponent<Node>().costStart[2] = 0;

            openListSorted = new List<GameObject>();
            openListSorted.Add(currentNode);

            do
            {
                openListSorted = openListSorted.OrderBy(x => x.GetComponent<Node>().costStart[2]).ToList();
                GameObject node = openListSorted.First();
                openListSorted.Remove(node);

                node.GetComponent<Node>().EvaluateClosestNeighbour();
                node.GetComponent<Node>().EvaluateNodeConnections();

                GameObject childNode = null;
                foreach (var connection in node.GetComponent<Node>().connections.OrderBy(x => x.CostEdge))
                {
                    childNode = connection.NodeConnected;
                    // Debug.DrawLine(node.transform.position, childNode.transform.position, Color.red, 2f);

                    if (childNode.GetComponent<Node>().hasBeenVisisted[2])
                    {
                        continue;
                    }

                    if (childNode.GetComponent<Node>().costStart[2] == 0 || node.GetComponent<Node>().costStart[2] + connection.CostEdge < childNode.GetComponent<Node>().costStart[2])
                    {
                        childNode.GetComponent<Node>().costStart[2] = node.GetComponent<Node>().costStart[2] + connection.CostEdge;
                        childNode.GetComponent<Node>().nearestToStart[2] = node;

                        if (!openListSorted.Contains(childNode))
                        {
                            openListSorted.Add(childNode);
                        }
                    }
                }

                //The actual path it takes.
                node.GetComponent<Node>().hasBeenVisisted[2] = true;

                if (node == goalNode)
                {
                    return;
                }

            } while (openListSorted.Any());
        }
        else if (gameObject.name == "Runner")
        {
            currentNode.GetComponent<Node>().costStart[0] = 0;

            openListSorted = new List<GameObject>();
            openListSorted.Add(currentNode);

            do
            {
                openListSorted = openListSorted.OrderBy(x => x.GetComponent<Node>().costStart[0]).ToList();
                GameObject node = openListSorted.First();
                openListSorted.Remove(node);

                node.GetComponent<Node>().EvaluateClosestNeighbour();
                node.GetComponent<Node>().EvaluateNodeConnections();

                GameObject childNode = null;
                foreach (var connection in node.GetComponent<Node>().connections.OrderBy(x => x.CostEdge))
                {
                    childNode = connection.NodeConnected;
                    //  Debug.DrawLine(node.transform.position, childNode.transform.position, Color.red, 333f);
                    if (childNode.GetComponent<Node>().hasBeenVisisted[0])
                    {
                        continue;
                    }

                    if (childNode.GetComponent<Node>().costStart[0] == 0 || node.GetComponent<Node>().costStart[0] + connection.CostEdge < childNode.GetComponent<Node>().costStart[0])
                    {
                        childNode.GetComponent<Node>().costStart[0] = node.GetComponent<Node>().costStart[0] + connection.CostEdge;
                        childNode.GetComponent<Node>().nearestToStart[0] = node;

                        if (!openListSorted.Contains(childNode))
                        {
                            openListSorted.Add(childNode);
                        }
                    }
                }

                //The actual path it takes.
                node.GetComponent<Node>().hasBeenVisisted[0] = true;

                if (node == goalNode)
                {
                    return;
                }

            } while (openListSorted.Any());
        }
        else if (gameObject.name == "Chaser1")
        {
            currentNode.GetComponent<Node>().costStart[1] = 0;

            openListSorted = new List<GameObject>();
            openListSorted.Add(currentNode);

            do
            {
                openListSorted = openListSorted.OrderBy(x => x.GetComponent<Node>().costStart[1]).ToList();
                GameObject node = openListSorted.First();
                openListSorted.Remove(node);

                node.GetComponent<Node>().EvaluateClosestNeighbour();
                node.GetComponent<Node>().EvaluateNodeConnections();

                GameObject childNode = null;
                foreach (var connection in node.GetComponent<Node>().connections.OrderBy(x => x.CostEdge))
                {
                    childNode = connection.NodeConnected;
                    //  Debug.DrawLine(node.transform.position, childNode.transform.position, Color.red, 2f);

                    if (childNode.GetComponent<Node>().hasBeenVisisted[1])
                    {
                        continue;
                    }

                    if (childNode.GetComponent<Node>().costStart[1] == 0 || node.GetComponent<Node>().costStart[1] + connection.CostEdge < childNode.GetComponent<Node>().costStart[1])
                    {
                        childNode.GetComponent<Node>().costStart[1] = node.GetComponent<Node>().costStart[1] + connection.CostEdge;
                        childNode.GetComponent<Node>().nearestToStart[1] = node;

                        if (!openListSorted.Contains(childNode))
                        {
                            openListSorted.Add(childNode);
                        }
                    }
                }

                //The actual path it takes.
                node.GetComponent<Node>().hasBeenVisisted[1] = true;

                if (node == goalNode)
                {
                    return;
                }

            } while (openListSorted.Any());
        }

    }

    /// <summary>
    /// The A* algorithm. Uses the euclidean distance to goal node.
    /// </summary>
    private void AStarAlgorithm()
    {
        if (gameObject.name == "Runner")
        {
            currentNode.GetComponent<Node>().costStart[0] = 0;

            openListSorted = new List<GameObject>();
            openListSorted.Add(currentNode);

            do
            {
                openListSorted = openListSorted.OrderBy(x => x.GetComponent<Node>().costStart[0] + x.GetComponent<Node>().euclideanDistanceToGoal[0]).ToList();
                GameObject node = openListSorted.First();
                openListSorted.Remove(node);

                node.GetComponent<Node>().EvaluateClosestNeighbour();
                node.GetComponent<Node>().EvaluateNodeConnections();

                GameObject childNode = null;

                foreach (var connection in node.GetComponent<Node>().connections.OrderBy(x => x.CostEdge))
                {
                    childNode = connection.NodeConnected;
                    //Debug.DrawLine(node.transform.position, childNode.transform.position, Color.yellow, 2);

                    if (childNode.GetComponent<Node>().hasBeenVisisted[0])
                    {
                        continue;
                    }

                    if (childNode.GetComponent<Node>().costStart[0] == 0 || node.GetComponent<Node>().costStart[0] + connection.CostEdge < childNode.GetComponent<Node>().costStart[0])
                    {
                        childNode.GetComponent<Node>().costStart[0] = node.GetComponent<Node>().costStart[0] + connection.CostEdge;
                        childNode.GetComponent<Node>().nearestToStart[0] = node;

                        if (!openListSorted.Contains(childNode))
                        {
                            openListSorted.Add(childNode);
                        }
                    }
                }

                node.GetComponent<Node>().hasBeenVisisted[0] = true;

                if (node == goalNode)
                {
                    return;
                }

            } while (openListSorted.Any());
        }
        else if (gameObject.name == "Chaser1")
        {
            currentNode.GetComponent<Node>().costStart[1] = 0;

            openListSorted = new List<GameObject>();
            openListSorted.Add(currentNode);

            do
            {
                openListSorted = openListSorted.OrderBy(x => x.GetComponent<Node>().costStart[1] + x.GetComponent<Node>().euclideanDistanceToGoal[1]).ToList();
                GameObject node = openListSorted.First();
                openListSorted.Remove(node);

                node.GetComponent<Node>().EvaluateClosestNeighbour();
                node.GetComponent<Node>().EvaluateNodeConnections();


                GameObject childNode = null;

                foreach (var connection in node.GetComponent<Node>().connections.OrderBy(x => x.CostEdge))
                {
                    childNode = connection.NodeConnected;
                    // Debug.DrawLine(node.transform.position, childNode.transform.position, Color.yellow, 2);

                    if (childNode.GetComponent<Node>().hasBeenVisisted[1])
                    {
                        continue;
                    }

                    if (childNode.GetComponent<Node>().costStart[1] == 0 || node.GetComponent<Node>().costStart[1] + connection.CostEdge < childNode.GetComponent<Node>().costStart[1])
                    {
                        childNode.GetComponent<Node>().costStart[1] = node.GetComponent<Node>().costStart[1] + connection.CostEdge;
                        childNode.GetComponent<Node>().nearestToStart[1] = node;

                        if (!openListSorted.Contains(childNode))
                        {
                            openListSorted.Add(childNode);
                        }
                    }
                }

                node.GetComponent<Node>().hasBeenVisisted[1] = true;

                if (node == goalNode)
                {
                    return;
                }

            } while (openListSorted.Any());
        }
        else if (gameObject.name == "Chaser2")
        {
            currentNode.GetComponent<Node>().costStart[2] = 0;

            openListSorted = new List<GameObject>();
            openListSorted.Add(currentNode);

            do
            {
                openListSorted = openListSorted.OrderBy(x => x.GetComponent<Node>().costStart[2] + x.GetComponent<Node>().euclideanDistanceToGoal[2]).ToList();
                GameObject node = openListSorted.First();
                openListSorted.Remove(node);

                node.GetComponent<Node>().EvaluateClosestNeighbour();
                node.GetComponent<Node>().EvaluateNodeConnections();


                GameObject childNode = null;

                foreach (var connection in node.GetComponent<Node>().connections.OrderBy(x => x.CostEdge))
                {
                    childNode = connection.NodeConnected;

                    // Debug.DrawLine(node.transform.position, childNode.transform.position, Color.yellow, 2);

                    if (childNode.GetComponent<Node>().hasBeenVisisted[2])
                    {
                        continue;
                    }

                    if (childNode.GetComponent<Node>().costStart[2] == 0 /*null*/ || node.GetComponent<Node>().costStart[2] + connection.CostEdge < childNode.GetComponent<Node>().costStart[2])
                    {
                        childNode.GetComponent<Node>().costStart[2] = node.GetComponent<Node>().costStart[2] + connection.CostEdge;
                        childNode.GetComponent<Node>().nearestToStart[2] = node;

                        if (!openListSorted.Contains(childNode))
                        {
                            openListSorted.Add(childNode);
                        }
                    }
                }

                node.GetComponent<Node>().hasBeenVisisted[2] = true;

                if (node == goalNode)
                {
                    //////print("DONE");
                    return;
                }

            } while (openListSorted.Any());
        }
    }


    /// <summary>
    /// Gets the shortest cluster using Dijkstra if in the same cluster
    /// or A* for outside clusters.
    /// </summary>
    /// <returns></returns>
    private List<GameObject> ClusterPath()
    {
        clusterPathList.Clear();

        if (currentNode.CompareTag(goalNode.tag))
        {
            return DijkstraPath();
        }
        else
        {
            clusterTable = lookUpTable[currentNode.GetComponent<Node>().clusterNum - 1][goalNode.GetComponent<Node>().clusterNum - 1];

            clusterPathList.Add(currentNode);
            goalNode = clusterTable.First();

            clusterPathList.AddRange(AStartPath());
            clusterPathList.AddRange(clusterTable);

            currentNode = clusterTable[clusterTable.Count - 1];

            SetGoalNode();

            clusterPathList.AddRange(AStartPath());

            clusterPathList = clusterPathList.Distinct().ToList();

            return clusterPathList;
        }
    }

    /// <summary>
    /// Clears the nodes for a new path finding algorithm run.
    /// </summary>
    public void ClearStuff()
    {
        indexer = 0;
        pathList.Clear();
        openListSorted.Clear();

        // Clear node values depening on the NPC.
        for (int i = 0; i < gameManager.GetComponent<GameManager>().totalNodeList.Count; i++)
        {
            if (gameObject.name == "Runner")
            {
                gameManager.GetComponent<GameManager>().totalNodeList[i].GetComponent<Node>().costStart[0] = 0;
                gameManager.GetComponent<GameManager>().totalNodeList[i].GetComponent<Node>().hasBeenVisisted[0] = false;
                gameManager.GetComponent<GameManager>().totalNodeList[i].GetComponent<Node>().nearestToStart[0] = null;
            }
            else if (gameObject.name == "Chaser1")
            {
                gameManager.GetComponent<GameManager>().totalNodeList[i].GetComponent<Node>().costStart[1] = 0;
                gameManager.GetComponent<GameManager>().totalNodeList[i].GetComponent<Node>().hasBeenVisisted[1] = false;
                gameManager.GetComponent<GameManager>().totalNodeList[i].GetComponent<Node>().nearestToStart[1] = null;
            }
            else if (gameObject.name == "Chaser2")
            {
                gameManager.GetComponent<GameManager>().totalNodeList[i].GetComponent<Node>().costStart[2] = 0;
                gameManager.GetComponent<GameManager>().totalNodeList[i].GetComponent<Node>().hasBeenVisisted[2] = false;
                gameManager.GetComponent<GameManager>().totalNodeList[i].GetComponent<Node>().nearestToStart[2] = null;
            }
        }
    }

    /// <summary>
    /// Kinematic flee.
    /// </summary>
    void SeekKinematic()
    {
        velDir = (nextNode.transform.position - transform.position);
        velDir = velDir.normalized;

        // Rotation
        Quaternion targetRotation = Quaternion.LookRotation(velDir, Vector3.up);
        transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, Time.deltaTime * 5f);

        velDir *= maxSpeed;

        GetComponent<Rigidbody>().velocity = velDir;
    }

    /// <summary>
    /// Steering flee.
    /// </summary>
    void SeekSteering()
    {
        velDir = (nextNode.transform.position - transform.position);

        GetComponent<Rigidbody>().velocity += velDir.normalized * maxAcceleration;

        if (GetComponent<Rigidbody>().velocity.magnitude > maxSpeed)
        {
            GetComponent<Rigidbody>().velocity = velDir.normalized * maxSpeed;
        }

        // Rotation
        Quaternion targetRotation = Quaternion.LookRotation(velDir, Vector3.up);
        transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, Time.deltaTime * 5);

    }

    /// <summary>
    /// The OnTriggerEnter method.
    /// </summary>
    /// <param name="col">The collider it touches</param>
    void OnTriggerEnter(Collider col)
    {
        // The game end check.
        if (isChaser && col.gameObject.name == "Runner")
        {
            // //print("END GAME");
            gameManager.GetComponent<GameManager>().isGameDone = true;
            Time.timeScale = 0;
        }

        // Make the runner stop if in a corner.
        if (isRunner && col.CompareTag("OutsideCollider"))
        {
            transform.position = currentNode.transform.position;
            gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }

}


