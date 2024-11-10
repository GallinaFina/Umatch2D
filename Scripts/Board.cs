using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public List<Node> nodes = new List<Node>();

    void Start()
    {
        foreach (Node node in nodes)
        {
            Debug.Log("Node found: " + node.nodeName);
        }
    }

    public Node GetNodeByName(string name)
    {
        return nodes.Find(node => node.nodeName == name);
    }
}
