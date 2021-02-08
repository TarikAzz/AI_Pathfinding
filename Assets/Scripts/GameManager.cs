using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// The goal node.
    /// </summary>
    public GameObject goalNode;

    /// <summary>
    /// The node container.
    /// </summary>
    public GameObject nodeContainer;

    /// <summary>
    /// The runner.
    /// </summary>
    public GameObject runner;

    /// <summary>
    /// Chaser1.
    /// </summary>
    public GameObject chaser1;

    /// <summary>
    /// Chaser2
    /// </summary>
    public GameObject chaser2;

    /// <summary>
    /// The list of Npcs for randomization.
    /// </summary>
    public List<GameObject> npcList;

    /// <summary>
    /// All nodes in the game area.
    /// </summary>
    public List<GameObject> totalNodeList = new List<GameObject>();

    /// <summary>
    /// The clusters sorted by number.
    /// </summary>
    public List<GameObject> Cluster1 = new List<GameObject>();
    public List<GameObject> Cluster2 = new List<GameObject>();
    public List<GameObject> Cluster3 = new List<GameObject>();
    public List<GameObject> Cluster4 = new List<GameObject>();
    public List<GameObject> Cluster5 = new List<GameObject>();
    public List<GameObject> Cluster6 = new List<GameObject>();

    /// <summary>
    /// The algorithm type.
    /// </summary>
    public int pathFindingType;

    /// <summary>
    /// The text associated pathfinding type.
    /// </summary>
    public Text pathFindingText;

    /// <summary>
    /// Text determining the type of flanking.
    /// </summary>
    public Text flankText;

    /// <summary>
    /// Locks the ai after a restart.
    /// </summary>
    public bool lockerAI = true;

    /// <summary>
    /// Locks the randomization.
    /// </summary>
    public bool lockerRandom = true;

    /// <summary>
    /// Checks if the game is finsihed in order to restart.
    /// </summary>
    public bool isGameDone = false;

    /// <summary>
    /// Checks if the ranomization did not give the same number.
    /// </summary>
    public bool randomWorked = false;

    /// <summary>
    /// Changes flank modes.
    /// </summary>
    public int strongFlankToggle = 0;

    public bool isSuperSaiayan = false;

    /// <summary>
    /// The audioSource.
    /// </summary>
    AudioSource audioSource;

    /// <summary>
    /// The super saiyan sound.
    /// </summary>
    public AudioClip superSaiyanSound;

    public bool ssLocker = true;

    /// <summary>
    /// Toggle robots.
    /// </summary>
    public bool robotsToggle = false;

    /// <summary>
    /// The Unity Awake method.
    /// </summary>
    void Awake()
    {
        strongFlankToggle = 0;
        npcList = new List<GameObject>();

        runner = GameObject.Find("Runner");
        chaser1 = GameObject.Find("Chaser1");
        chaser2 = GameObject.Find("Chaser2");

        npcList.Add(runner);
        npcList.Add(chaser1);
        npcList.Add(chaser2);

        // Populate the node list from its parent container.
        nodeContainer = GameObject.Find("NodeContainerAll");

        for (int i = 0; i < nodeContainer.transform.childCount; i++)
        {
            totalNodeList.Add(nodeContainer.transform.GetChild(i).gameObject);
        }

        ApplyClusterColour();

        //Make it always run Dijkstra first.
        pathFindingType = 1;
        pathFindingText.text = "Dijkstra";

        // Choose a random starting node.
        while (!lockerRandom)
        {
            RandomStartingNode();
        }

        lockerRandom = false;

        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// The Unity Update method,
    /// </summary>
    void Update()
    {
        if (Input.GetButtonDown("One"))
        {
            pathFindingType = 1;
            pathFindingText.text = "Dijkstra";
        }
        else if (Input.GetButtonDown("Two"))
        {
            pathFindingType = 2;
            pathFindingText.text = "A*";
        }
        else if (Input.GetButtonDown("Three"))
        {
            pathFindingType = 3;
            pathFindingText.text = "Cluster";
        }

        if (Input.GetButtonDown("Space"))
        {
            Restart();
        }

        if (Input.GetButtonDown("Five"))
        {
            strongFlankToggle++;

            if (strongFlankToggle > 2)
            {
                strongFlankToggle = 0;
            }
        }

        switch (strongFlankToggle)
        {
            case 0:
                flankText.text = "Flank: Normal";
                break;
            case 1:
                flankText.text = "Flank: Strong";
                break;
            case 2:
                flankText.text = "Flank: None";
                break;
        }

        if (isSuperSaiayan)
        {
            audioSource.PlayOneShot(superSaiyanSound);
            isSuperSaiayan = false;
        }

        // Restart game is finishec.
        if (isGameDone)
        {
            Restart();
        }
    }

    /// <summary>
    /// Restarts the game when finished.
    /// </summary>
    void Restart()
    {
        while (!lockerRandom)
        {
            RandomStartingNode();
        }

        lockerRandom = false;

        for (int i = 0; i < npcList.Count; i++)
        {
            npcList[i].GetComponent<AIMovements>().ClearStuff();
            npcList[i].GetComponent<Rigidbody>().velocity = Vector3.zero;
            npcList[i].GetComponent<AIMovements>().isSteering = false;
            npcList[i].GetComponent<AIMovements>().startNode = npcList[i].GetComponent<AIMovements>().ClosestNodeCalculate();
            npcList[i].GetComponent<AIMovements>().currentNode = npcList[i].GetComponent<AIMovements>().startNode;
            npcList[i].GetComponent<AIMovements>().SetGoalNode();
        }

        isGameDone = false;
        Time.timeScale = 1;
    }

    /// <summary>
    /// Gets a random starting node for the Npcs.
    /// </summary>
    public bool RandomStartingNode()
    {
        System.Random rand = new System.Random();

        List<int> tempList = new List<int>();

        tempList.Add(rand.Next(totalNodeList.Count - 1));
        tempList.Add(rand.Next(totalNodeList.Count - 1));
        tempList.Add(rand.Next(totalNodeList.Count - 1));

        tempList = tempList.Distinct().ToList();

        if (tempList.Count == 3)
        {
            runner.transform.position = (totalNodeList[tempList[0]].transform.position);
            chaser1.transform.position = (totalNodeList[tempList[1]].transform.position);
            chaser2.transform.position = (totalNodeList[tempList[2]].transform.position);

            tempList.Clear();
            lockerRandom = true;
            lockerAI = false;
            return true;
        }
        else
        {
            tempList.Clear();
            return false;
        }
    }

    /// <summary>
    /// Applies a colour to a node based on their cluster number.
    /// </summary>
    void ApplyClusterColour()
    {
        for (int i = 0; i < totalNodeList.Count; i++)
        {
            int clusterNumTemp = totalNodeList[i].GetComponent<Node>().getClusterNum();

            // Colour the nodes depending on their cluster number.
            switch (clusterNumTemp)
            {
                case 1:
                    totalNodeList[i].GetComponent<Renderer>().material.color = Color.yellow;
                    Cluster1.Add(totalNodeList[i]);
                    break;
                case 2:
                    totalNodeList[i].GetComponent<Renderer>().material.color = Color.blue;
                    Cluster2.Add(totalNodeList[i]);
                    break;
                case 3:
                    totalNodeList[i].GetComponent<Renderer>().material.color = Color.green;
                    Cluster3.Add(totalNodeList[i]);
                    break;
                case 4:
                    totalNodeList[i].GetComponent<Renderer>().material.color = Color.magenta;
                    Cluster4.Add(totalNodeList[i]);
                    break;
                case 5:
                    totalNodeList[i].GetComponent<Renderer>().material.color = Color.red;
                    Cluster5.Add(totalNodeList[i]);
                    break;
                case 6:
                    totalNodeList[i].GetComponent<Renderer>().material.color = Color.cyan;
                    Cluster6.Add(totalNodeList[i]);
                    break;
            }
        }
    }
}
