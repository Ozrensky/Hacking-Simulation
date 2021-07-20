using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public enum Type { start, treasure, firewall, data, spam }
    public GameObject startNodeObject;
    public GameObject treasureNodeObject;
    public GameObject firewallNodeObject;
    public GameObject dataNodeObject;
    public GameObject spamNodeObject;

    public List<Connection> connections = new List<Connection>();
    public Type type;
    [HideInInspector] public Transform trans;
    [HideInInspector] public SpriteRenderer sRenderer;
    Collider2D coll;

    [System.Serializable]
    public class Connection
    {
        public Node connectedNode;
        public LineRenderer lineRenderer;
    }

    private void Awake()
    {
        trans = transform;
        sRenderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
    }

    public Connection FindConnection(Node node)
    {
        foreach (Connection c in connections)
        {
            if (c.connectedNode == node)
                return c;
        }
        return null;
    }

    public void ToggleCollider(bool value)
    {
        if (coll)
            coll.enabled = value;
    }

    public List<LineRenderer> ReturnLineRenderers()
    {
        List<LineRenderer> lr = new List<LineRenderer>();
        foreach (Connection c in connections)
        {
            lr.Add(c.lineRenderer);
        }

        return lr;
    }

    public void SetupNode()
    {
        switch (type)
        {
            case Type.data:
                startNodeObject.SetActive(false); treasureNodeObject.SetActive(false); firewallNodeObject.SetActive(false); spamNodeObject.SetActive(false); dataNodeObject.SetActive(true);
                break;
            case Type.firewall:
                startNodeObject.SetActive(false); treasureNodeObject.SetActive(false); dataNodeObject.SetActive(false); spamNodeObject.SetActive(false); firewallNodeObject.SetActive(true);
                break;
            case Type.spam:
                startNodeObject.SetActive(false); treasureNodeObject.SetActive(false); dataNodeObject.SetActive(false); firewallNodeObject.SetActive(false); spamNodeObject.SetActive(true);
                break;
            case Type.start:
                spamNodeObject.SetActive(false); treasureNodeObject.SetActive(false); dataNodeObject.SetActive(false); firewallNodeObject.SetActive(false); startNodeObject.SetActive(true);
                break;
            case Type.treasure:
                spamNodeObject.SetActive(false); startNodeObject.SetActive(false); dataNodeObject.SetActive(false); firewallNodeObject.SetActive(false); treasureNodeObject.SetActive(true);
                break;
        }
    }

    public bool IsIntersecting(LineRenderer lr)
    {
        foreach (Connection c in connections)
        {
            if (lr.bounds.Intersects(c.lineRenderer.bounds))
                return true;
        }
        return false;
    }

    public void RemoveConnection(Node connectionNode)
    {
        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i].connectedNode == connectionNode)
            {
                HackingController.Instance.ReturnLineRenderer(connections[i].lineRenderer);
                connections.RemoveAt(i);
                break;
            }
        }
    }

    public void RemoveConnection(LineRenderer lr)
    {
        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i].lineRenderer == lr)
            {
                connections.RemoveAt(i);
                break;
            }
        }
    }

    public void RemoveEmptyConnections()
    {
        for (int i = 0; i < connections.Count; i++)
        {
            if (!connections[i].lineRenderer.enabled)
            {
                connections.RemoveAt(i);
                i--;
            }
        }
    }
}