﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNodeProgressTracker : MonoBehaviour
{
    //CarAI ai;

    public List<Vector3> waypoints;
    [SerializeField] int substeps = 15;

    Rigidbody rb;

    int pathnodesToCalculate = 2;
    float lookAheadMinDistance = 1f;
    float lookAheadMaxDistance = 10f;
    float lookAheadSpeedModifier = 0.5f;

    int curvePercentageLookAheadIndex = 5;

    //This helps us to make sure we don't target the same position again if already close enough
    public float distanceToAcceptAsPassed = 5f;
    //public int passedProgress = 0;

    public Vector3 target;
    public float curvePercentage;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (waypoints.Count > 0)
        {
            RemovePassedWaypoints();
            target = CalculateTarget();
            //curvePercentage = CalculateCurvePercentage(waypoints[waypoints.Count - 1]);

            int index = Mathf.Min(curvePercentageLookAheadIndex, waypoints.Count - 1);

            curvePercentage = CalculateCurvePercentage(waypoints[index]);
        }
    }

    //Remove waypoints we are passing, but leave at least one
    private void RemovePassedWaypoints()
    {
        for (int i = 0; i < waypoints.Count-1; i++)
        {
            if (Vector3.Distance(rb.position, waypoints[i]) < distanceToAcceptAsPassed)
            {
                waypoints.RemoveAt(i);
                i--;
            }
        }
    }

    private Vector3 CalculateTarget()
    {
        float speed = rb.velocity.magnitude * 3.6f;
        float distanceToCheck = speed * lookAheadSpeedModifier;

        distanceToCheck = Mathf.Max(lookAheadMinDistance, distanceToCheck);
        distanceToCheck = Mathf.Min(lookAheadMaxDistance, distanceToCheck);
        float testedDistance = 0;
        Vector3 a = rb.position;
        for (int i = 0; i < waypoints.Count; i++)
        {
            testedDistance += Vector3.Distance(a, waypoints[i]);
            if (testedDistance > distanceToCheck)
            {
                if (i == 0)
                {
                    return waypoints[0];
                }
                return a;
            }
            a = waypoints[i];
        }
        return waypoints[waypoints.Count - 1];
    }


    public void UpdatePath(List<PathNode> path, PathNode currentNode)
    {
        curvePercentage = 0;
        //passedProgress = 0;
        waypoints = new List<Vector3>();
        if (path.Count > 0)
        {
            List<Vector3> nodePositions = new List<Vector3>();

            int nodes = pathnodesToCalculate;
            nodes = Mathf.Min(path.Count - 1, nodes);


            Vector3 averageBackwardsNodePosition = Vector3.zero;
            for (int inNode = 0; inNode < currentNode.GetInConnections().Count; inNode++)
            {
                averageBackwardsNodePosition += currentNode.GetInConnections()[inNode].transform.position;
            }
            averageBackwardsNodePosition /= currentNode.GetInConnections().Count;

            nodePositions.Add(averageBackwardsNodePosition);
            nodePositions.Add(currentNode.transform.position);
            for (int i = 0; i < path.Count; i++)
            {
                nodePositions.Add(path[i].transform.position);
            }

            if (nodePositions.Count > 3)
            {
                for (int i = 0; i < nodes; i++)
                {
                    CreateProgressPath(nodePositions[i], nodePositions[i + 1], nodePositions[i + 2], nodePositions[i + 3]);
                }
            }
            else
            {
                CreateStraightPath(path[0].transform.position);
            }
            curvePercentage = CalculateCurvePercentage(waypoints[waypoints.Count - 1]);
        }
    }

    private void CreateStraightPath(Vector3 endTarget)
    {
        for (int step = 0; step < substeps; step++)
        {
            float progress = (float)step / (float)substeps;
            waypoints.Add(Vector3.Lerp(transform.position, endTarget, progress));
        }
    }

    private void CreateProgressPath(Vector3 positionComingFrom, Vector3 currentPosition, Vector3 targetPos, Vector3 positionAfterTargetPos)
    {
        for (int step = 0; step < substeps; step++)
        {
            float progress = (float)step / (float)substeps;
            waypoints.Add(CatmullRom(positionComingFrom, currentPosition, targetPos, positionAfterTargetPos, progress));
        }
    }

    private float CalculateCurvePercentage(Vector3 endPoint)
    {
        Vector3 relativeVector = transform.InverseTransformPoint(endPoint);
        relativeVector /= relativeVector.magnitude;
        return (relativeVector.x / relativeVector.magnitude);
    }

    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float i)
    {
        // comments are no use here... it's the catmull-rom equation.
        // Un-magic this, lord vector!
        return 0.5f *
               ((2 * p1) + (-p0 + p2) * i + (2 * p0 - 5 * p1 + 4 * p2 - p3) * i * i +
                (-p0 + 3 * p1 - 3 * p2 + p3) * i * i * i);
    }

    [Header("Editor")]
    public Color targetIndicatorColor = Color.yellow;

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            if (waypoints.Count > 0)
            {
                Gizmos.color = targetIndicatorColor;
                for (int i = 0; i < waypoints.Count - 1; i++)
                {
                    Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
                }
                Gizmos.DrawLine(transform.position, target);
                Gizmos.DrawWireSphere(target, 0.2f);

                for (int i = 0; i < waypoints.Count; i++)
                {
                    Gizmos.DrawWireSphere(waypoints[i], 0.1f);
                }
            }
        }
    }
}
