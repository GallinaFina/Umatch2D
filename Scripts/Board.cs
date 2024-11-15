using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    public List<Node> GetNodesInZone(string zoneName)
    {
        return nodes.Where(node => node.zones.Contains(zoneName)).ToList();
    }

    public bool IsNodeInZone(Node node, string zoneName)
    {
        return node.zones.Contains(zoneName);
    }
}
