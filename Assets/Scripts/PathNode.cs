﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode : MonoBehaviour
{
    public static PathNode selectedNodeForConnection; //used with hotkeys for quick connection between nodes

    [SerializeField] bool allowedToPass = true;
    [SerializeField] float roadSpeedLimit = 30;

    public bool isPartOfIntersection = false; //Used to enable cars to check for other cars in the intersection
    [SerializeField] List<PathNode> nodesToWaitFor = new List<PathNode>();
    public List<CarAI> carsOnThisNode = new List<CarAI>(); //debug public

    [SerializeField] List<PathNode> possibleNextNodes = new List<PathNode>();
    //public List<DirectionChoice> outChoices = new List<DirectionChoice>();
    public List<PathNode> backwardNodes = new List<PathNode>(); //used for catmull-rom (curved path)


    private void Start()
    {
        if (possibleNextNodes.Count < 1)
        {
            Debug.LogError("You have not set up the path correctly, this node is missing a nodeconnection " + transform.name);
        }
    }


    /// <summary>
    /// Returns true when green light is on, or if there is no traffic light present
    /// </summary>
    public bool IsAllowedToPass()
    {
        if (!allowedToPass)
        {
            return false;
        }
        else
        {
            if (isPartOfIntersection)
            {
                for (int i = 0; i < nodesToWaitFor.Count; i++)
                {
                    if (nodesToWaitFor[i].carsOnThisNode.Count != 0)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return true;
            }
        }
    }

    /// <summary>
    /// Sets flags the node as allowed to pass or not.
    /// </summary>
    public void SetAllowedToPass(bool input)
    {
        allowedToPass = input;
    }

    /// <summary>
    /// Switches the bool of allowed to pass
    /// </summary>
    public void SwitchAllowedToPass()
    {
        allowedToPass = allowedToPass ? false : true;
    }

    /// <summary>
    /// Returns the speed limit at this node
    /// </summary>
    public float GetRoadSpeedLimit()
    {
        return roadSpeedLimit;
    }

    /// <summary>
    /// Sets the speed limit at this node
    /// </summary>
    public void SetRoadSpeedLimit(float input)
    {
        roadSpeedLimit = input;
    }

    /// <summary>
    /// Returns the nodes possible to go to next from this position
    /// </summary>
    public List<PathNode> GetNextPossibleNodes()
    {
        return possibleNextNodes;
    }

    /// <summary>
    /// Adds a new node to the list of connected nodes, mainly used in editor too quickly create new nodes
    /// </summary>
    public void AddPossibleNextNode(PathNode input)
    {
        if (input != this && !possibleNextNodes.Contains(input))
        {
            possibleNextNodes.Add(input);
            input.AddBackwardsNodeConnection(this);
        }
    }
    public void AddConnectedNode(List<PathNode> input)
    {
        for (int i = 0; i < input.Count; i++)
        {
            AddPossibleNextNode(input[i]);
        }
    }

    /// <summary>
    /// Adds a new node to the list of previous or incoming nodes, used for catmull-rom calculation
    /// </summary>
    public void AddBackwardsNodeConnection(PathNode input)
    {
        if (input != this && !backwardNodes.Contains(input))
        {
            backwardNodes.Add(input);
        }
    }

    /// <summary>
    /// Returns all backward connections of this node
    /// </summary>
    public List<PathNode> GetBackWardConnections()
    {
        return backwardNodes;
    }

    /// <summary>
    /// Replaces the connected node, mainly used in editor too quickly create new nodes between already made nodes
    /// </summary>
    public void ReplaceConnectedNode(PathNode input)
    {
        if (possibleNextNodes.Count == 0)
        {
            possibleNextNodes.Add(input);
        }
        else
        {
            possibleNextNodes[0] = input;
        }
        input.AddBackwardsNodeConnection(this);
    }

    public void ClearForwardConnections()
    {
        for (int i = 0; i < possibleNextNodes.Count; i++)
        {
            possibleNextNodes[i].backwardNodes.Remove(this);
        }
        possibleNextNodes.Clear();
    }



    public void AddCarToNode(CarAI car)
    {
        if (carsOnThisNode.Contains(car))
        {
            Debug.LogWarning("Tried to att the an already existing car to this node" + transform.name);
        }
        else
        {
            carsOnThisNode.Add(car);
        }
    }

    public void RemoveCarFromNode(CarAI car)
    {
        if (carsOnThisNode.Contains(car))
        {
            carsOnThisNode.Remove(car);
        }
        else
        {
            Debug.LogWarning("Tried to remove a car from this node" + transform.name);
        }
    }

    public List<CarAI> GetCarsOnThisNode()
    {
        return carsOnThisNode;
    }




    [Header("Editor")]
    Color lineColor = Color.green;
    Color allowedToPassColor = Color.green;
    Color notAllowedToPassColor = Color.red;
    float nodeSize = 1f;
    int visualPathSubsteps = 15; //substeps for catmull-rom curve

    private void OnDrawGizmos()
    {
        //Draw sphere
        Gizmos.color = allowedToPass ? allowedToPassColor : notAllowedToPassColor;
        Gizmos.DrawWireSphere(this.transform.position, nodeSize);

        #region DrawLinesAndCheckConnectivity
        bool catmullCurveAllowed = true;
        int visualizationSubsteps = visualPathSubsteps;

        ValidateConnections();

        //Lines and curves
        Gizmos.color = lineColor;
        for (int outNode = 0; outNode < possibleNextNodes.Count; outNode++)
        {
            Vector3 currentNode = this.transform.position;
            Vector3 nextNode = Vector3.zero;
            nextNode = possibleNextNodes[outNode].transform.position;

            Vector3 direction = (nextNode - currentNode).normalized;
            Vector3 arrowPosition = currentNode + direction;
            DrawArrow.ForGizmo(arrowPosition, direction, lineColor, 0.4f, 30);

            //Safety check before catmull-rom to make sure connections can be checked both ways
            if (!possibleNextNodes[outNode].GetBackWardConnections().Contains(this))
            {
                possibleNextNodes[outNode].AddBackwardsNodeConnection(this);
            }

            //If we are missing any connections anywhere here we cannot make a proper catmull-curve
            if (backwardNodes.Count < 1 || possibleNextNodes.Count < 1 || possibleNextNodes[outNode].possibleNextNodes.Count < 1)
            {
                catmullCurveAllowed = false;
                Color temp = Gizmos.color;
                Gizmos.color = Color.red;
                Gizmos.DrawLine(this.transform.position, possibleNextNodes[outNode].transform.position);
                Gizmos.color = temp;
            }

            if (catmullCurveAllowed)
            {
                //instead of showing many paths for all different kind of combinations of inputs and outputs, we take the average of position 1 and 4 of the catmull-rom if there are multiple
                Vector3 averageBackwardsNodePosition = Vector3.zero;
                for (int inNode = 0; inNode < backwardNodes.Count; inNode++)
                {
                    averageBackwardsNodePosition += backwardNodes[inNode].transform.position;
                }
                averageBackwardsNodePosition /= backwardNodes.Count;

                Vector3 averageOutNodeOutNodesPosition = Vector3.zero;
                for (int outNodeOutNode = 0; outNodeOutNode < possibleNextNodes[outNode].possibleNextNodes.Count; outNodeOutNode++)
                {
                    averageOutNodeOutNodesPosition += possibleNextNodes[outNode].possibleNextNodes[outNodeOutNode].transform.position;
                }
                averageOutNodeOutNodesPosition /= possibleNextNodes[outNode].possibleNextNodes.Count;

                //use catmull rom to draw a curved path
                for (int step = 0; step < visualizationSubsteps; step++)
                {
                    float progress = (float)step / (float)visualizationSubsteps;
                    Vector3 a = CatmullRom(averageBackwardsNodePosition, this.transform.position, possibleNextNodes[outNode].transform.position, averageOutNodeOutNodesPosition, progress);
                    progress = ((float)step + 1) / (float)visualizationSubsteps;
                    Vector3 b = CatmullRom(averageBackwardsNodePosition, this.transform.position, possibleNextNodes[outNode].transform.position, averageOutNodeOutNodesPosition, progress);
                    Gizmos.DrawLine(a, b);
                }
            }
        }
        #endregion

        //Show waiting nodes
        Gizmos.color = Color.red;
        for (int i = 0; i < nodesToWaitFor.Count; i++)
        {
            if (nodesToWaitFor[i].carsOnThisNode.Count != 0)
            {
                Gizmos.DrawLine(this.transform.position, nodesToWaitFor[i].transform.position);
            }
        }
    }

    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float i)
    {
        // comments are no use here... it's the catmull-rom equation.
        // Un-magic this, lord vector!
        return 0.5f *
               ((2 * p1) + (-p0 + p2) * i + (2 * p0 - 5 * p1 + 4 * p2 - p3) * i * i +
                (-p0 + 3 * p1 - 3 * p2 + p3) * i * i * i);
    }

    private void ValidateConnections()
    {
        //Safety check, delete inactive nodes
        for (int i = 0; i < backwardNodes.Count; i++)
        {
            if (backwardNodes[i] == null)
            {
                backwardNodes.Remove(backwardNodes[i]);
                Debug.Log("Removed null-node");
            }
        }
        for (int i = 0; i < possibleNextNodes.Count; i++)
        {
            if (possibleNextNodes[i] == null)
            {
                possibleNextNodes.Remove(possibleNextNodes[i]);
                Debug.Log("Removed null-node");
            }
        }

        //Add if this is missing in connected nodes backward nodes
        for (int i = 0; i < possibleNextNodes.Count; i++)
        {
            if (!possibleNextNodes[i].GetBackWardConnections().Contains(this))
            {
                possibleNextNodes[i].AddBackwardsNodeConnection(this);
                Debug.Log("Added missing node connection");
            }
        }

        //If this node has backward - connections which no longer is connected to this node, remove the backward - connection
        for (int i = 0; i < backwardNodes.Count; i++)
        {
            if (!backwardNodes[i].possibleNextNodes.Contains(this))
            {
                backwardNodes.Remove(backwardNodes[i]);
                Debug.Log("Removed backward node (" + backwardNodes[i].transform.name + ") to a no longer connected node from: " + transform.name + transform.position);
            }
        }
    }
}



public static class DrawArrow
{
    public static void ForGizmo(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.DrawRay(pos, direction);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
    }

    public static void ForGizmo(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.color = color;
        Gizmos.DrawRay(pos, direction);
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
    }

    public static void ForDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Debug.DrawRay(pos, direction);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Debug.DrawRay(pos + direction, right * arrowHeadLength);
        Debug.DrawRay(pos + direction, left * arrowHeadLength);
    }
    public static void ForDebug(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Debug.DrawRay(pos, direction, color);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
        Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
    }
}

public enum Turn { NotSet, Straight, Left, Right }

[System.Serializable]
public struct DirectionChoice
{
    public PathNode outNode;
    public Turn turnDirection;
}