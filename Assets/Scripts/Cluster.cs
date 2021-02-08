using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cluster : MonoBehaviour
{
    /// <summary>
    /// The lookUpTable used for AStar Cluster.
    /// </summary>
    public List<GameObject>[][] lookUpTable = new List<GameObject>[6][];

   /// <summary>
   /// The Unity Awake method.
   /// </summary>
    void Awake()
    {
        // Add all nodes to the look up table.
        // The shortest path from one cluster to another are all contained here.
        lookUpTable[0] = new List<GameObject>[6];
        lookUpTable[1] = new List<GameObject>[6];
        lookUpTable[2] = new List<GameObject>[6];
        lookUpTable[3] = new List<GameObject>[6];
        lookUpTable[4] = new List<GameObject>[6];
        lookUpTable[5] = new List<GameObject>[6];

        // Yellow 1
        lookUpTable[0][0] = null; // Empty for this one.
        lookUpTable[0][1] = new List<GameObject> { GameObject.Find("Node (45)"), GameObject.Find("Node (42)") };
        lookUpTable[0][2] = new List<GameObject> { GameObject.Find("Node (45)"), GameObject.Find("Node (42)"), GameObject.Find("Node (36)"), GameObject.Find("Node (38)") };
        lookUpTable[0][3] = new List<GameObject> { GameObject.Find("Node (45)"), GameObject.Find("Node (42)"), GameObject.Find("Node (36)"), GameObject.Find("Node (38)"), GameObject.Find("Node (33)") };
        lookUpTable[0][4] = new List<GameObject> { GameObject.Find("Node (45)"), GameObject.Find("Node (42)"), GameObject.Find("Node (36)"), GameObject.Find("Node (69)"), GameObject.Find("Node (67)"), GameObject.Find("Node (72)"), GameObject.Find("Node (4)") };
        lookUpTable[0][5] = new List<GameObject> { GameObject.Find("Node (62)"), GameObject.Find("Node (64)") };

        // Blue
        lookUpTable[1][0] = new List<GameObject> { GameObject.Find("Node (42)"), GameObject.Find("Node (45)") };
        lookUpTable[1][1] = null; //Empty for this one.
        lookUpTable[1][2] = new List<GameObject> { GameObject.Find("Node (36)"), GameObject.Find("Node (38)") };
        lookUpTable[1][3] = new List<GameObject> { GameObject.Find("Node (36)"), GameObject.Find("Node (33)") };
        lookUpTable[1][4] = new List<GameObject> { GameObject.Find("Node (36)"), GameObject.Find("Node (69)"), GameObject.Find("Node (67)"), GameObject.Find("Node (72)"), GameObject.Find("Node (4)") };
        lookUpTable[1][5] = new List<GameObject> { GameObject.Find("Node (36)"), GameObject.Find("Node (69)"), GameObject.Find("Node (67)"), GameObject.Find("Node (78)") };

        // Green
        lookUpTable[2][0] = new List<GameObject> { GameObject.Find("Node (38)"), GameObject.Find("Node (36)"), GameObject.Find("Node (42)"), GameObject.Find("Node (45)") };
        lookUpTable[2][1] = new List<GameObject> { GameObject.Find("Node (38)"), GameObject.Find("Node (36)") };
        lookUpTable[2][2] = null;
        lookUpTable[2][3] = new List<GameObject> { GameObject.Find("Node (79)"), GameObject.Find("Node (20)") };
        lookUpTable[2][4] = new List<GameObject> { GameObject.Find("Node (19)"), GameObject.Find("Node (4)") };
        lookUpTable[2][5] = new List<GameObject> { GameObject.Find("Node (67)"), GameObject.Find("Node (78)") };

        // Purple
        lookUpTable[3][0] = new List<GameObject> { GameObject.Find("Node (33)"), GameObject.Find("Node (38)"), GameObject.Find("Node (36)"), GameObject.Find("Node (42)"), GameObject.Find("Node (45)") };
        lookUpTable[3][1] = new List<GameObject> { GameObject.Find("Node (33)"), GameObject.Find("Node (36)") };
        lookUpTable[3][2] = new List<GameObject> { GameObject.Find("Node (20)"), GameObject.Find("Node (79)") };
        lookUpTable[3][3] = null;
        lookUpTable[3][4] = new List<GameObject> { GameObject.Find("Node (20)"), GameObject.Find("Node (4)") };
        lookUpTable[3][5] = new List<GameObject> { GameObject.Find("Node (20)"), GameObject.Find("Node (78)") };

        // Red
        lookUpTable[4][0] = new List<GameObject> { GameObject.Find("Node"),GameObject.Find("Node (66)"), GameObject.Find("Node (65)"), GameObject.Find("Node (83)"), GameObject.Find("Node (64)"), GameObject.Find("Node (62)") };
        lookUpTable[4][1] = new List<GameObject> { GameObject.Find("Node (4)"), GameObject.Find("Node (72)"), GameObject.Find("Node (67)"), GameObject.Find("Node (69)"), GameObject.Find("Node (82)"), GameObject.Find("Node (36)") };
        lookUpTable[4][2] = new List<GameObject> { GameObject.Find("Node (4)"), GameObject.Find("Node (19)") };
        lookUpTable[4][3] = new List<GameObject> { GameObject.Find("Node (4)"), GameObject.Find("Node (20)") };
        lookUpTable[4][4] = null;
        lookUpTable[4][5] = new List<GameObject> { GameObject.Find("Node"), GameObject.Find("Node (68)") };

        // Cyan
        lookUpTable[5][0] = new List<GameObject> { GameObject.Find("Node (64)"), GameObject.Find("Node (62)") };
        lookUpTable[5][1] = new List<GameObject> { GameObject.Find("Node (78)"), GameObject.Find("Node (67)"), GameObject.Find("Node (69)"), GameObject.Find("Node (82)"), GameObject.Find("Node (36)") };
        lookUpTable[5][2] = new List<GameObject> { GameObject.Find("Node (78)"), GameObject.Find("Node (67)") };
        lookUpTable[5][3] = new List<GameObject> { GameObject.Find("Node (78)"), GameObject.Find("Node (79)"), GameObject.Find("Node (20)") };
        lookUpTable[5][4] = new List<GameObject> { GameObject.Find("Node (68)"), GameObject.Find("Node") };
        lookUpTable[5][5] = null;
    }
}


//Cluster Table:

//Yellow1
//Yellow1 to Yellow1= = --- *******
//Yellow1 to Blue2 = 45, 42 *******
//Yellow1 to Green3 = 45, 42, 36, 38 ****
//Yellow1 to Purple4 = 45, 42, 36, 38, 33 **
//Yellow1 to Red5 = 45, 42, 36
//Yellow1 to Cyan6 = 62, 64



//Blue2
//Blue2 to Yellow1 = 42, 45
//Blue2 to Blue2 = ---
//Blue2 to Green3 = 36, 38
//Blue2 to Purple4 = 36, 33
//Blue2 to Red5 = 36blue, 67green, 72green, 4red
//Blue2 to Cyan6 = 78, 67, 69, 36

//Green3
//Green3 to Yellow1= = 36, 42, 45
//Green3 to Blue2 = 38, 36
//Green3 to Green3 = ----
//Green3 to Purple4 = 79, 20
//Green3 to Red5 = 19, 4
//Green3 to Cyan6  = 67, 78

//Purple4
//Purple4 to Yellow1 =  33, 38, 36,42,45
//Purple4 to Blue2 = 33, 36
//Purple4 to Green3 = 20, 79
//Purple4 to Purple4 = ----
//Purple4 to Red5 = 20, 4
//Purple4 to Cyan6 = 20, 78

//Red5
//Red5 to Yellow1= 66, 65, 64, 62
//Red5 to Blue2 = 4red, 72green, 67green, 36blue.
//Red5 to Green3 = node4 to node19
//Red5 to Purple4 = node4 to node20
//Red5 to Red5 =--
//Red5 to Cyan6 = Node to node68


//Cyan6
//Cyan6 to Yellow1= 64, 62
//Cyan6 to Blue2 = 36, 69, 67, 78
//Cyan6 to Green3 = node78 to node67
//Cyan6 to Purple4 = node78, node79, node20
//Cyan6 to Red5 = node68, node
//Cyan6 to Cyan6  =--






