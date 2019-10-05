﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathNode))]
public class PathNodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PathNode myPathNode = (PathNode)target;

        DrawDefaultInspector();


        if (GUILayout.Button("Create new connected node"))
        {
            PathNode newNode = Instantiate(Resources.Load<PathNode>("Prefabs/Road/Pathnode"), myPathNode.transform.position, myPathNode.transform.rotation);
            myPathNode.AddConnectedNode(newNode);
            newNode.transform.SetParent(myPathNode.transform.parent);
        }

        if (GUILayout.Button("Replace connected node with new node"))
        {
            PathNode newNode = Instantiate(Resources.Load<PathNode>("Prefabs/Road/Pathnode"), myPathNode.transform.position, myPathNode.transform.rotation);
            newNode.AddConnectedNode(myPathNode.GetPathNodes());
            myPathNode.ReplaceConnectedNode(newNode);
            newNode.transform.SetParent(myPathNode.transform.parent);
        }
    }
}