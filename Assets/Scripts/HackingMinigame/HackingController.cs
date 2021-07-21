using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HackingController : MonoBehaviour
{
    [SerializeField] Transform nodesHolder;
    [Header("Prefabs")]
    [SerializeField] GameObject nodePrefab;
    [SerializeField] GameObject lrPrefab;
    public Vector2 difficultyRange;
    public float baseHackTime;
    [Header("Treasure Rewards")]
    public Vector2 xpRewardRange;
    public Vector2 nukeRewardRange;
    public Vector2 trapRewardRange;

    SpriteRenderer spawnRenderer;
    Node startNode;
    Node clickedNode;
    int treasuresLeft = 0;

    //Pools
    public List<Node> nodesPool = new List<Node>();
    public List<LineRenderer> lrPool = new List<LineRenderer>();

    public List<Node> spawnedNodes = new List<Node>();
    public List<Node> firewallNodes = new List<Node>();
    public List<ConnectionData> spawnedLr = new List<ConnectionData>();

    public Coroutine spawnCoroutine = null;
    public static bool setupDone = false;
    public static bool tracerActive = false;
    public static bool hackingActive = false;
    public GameSettings settings = new GameSettings();
    public static HackingController Instance;

    [System.Serializable]
    public class GameSettings
    {
        public int nodeCount;
        public int treasureCount;
        public int firewallCount;
        public int spamCount;
        public float spamDecrease;
        public float trapDelay;

        public GameSettings Copy()
        {
            return (GameSettings)MemberwiseClone();
        }
    }

    [System.Serializable]
    public class ConnectionData
    {
        public LineRenderer lineRenderer;
        public List<Node> nodes = new List<Node>();
    }

    private void Awake()
    {
        Instance = this;
        if (nodesHolder != null)
            spawnRenderer = nodesHolder.GetComponent<SpriteRenderer>();
    }

    public void SetupLevel(bool restart)
    {
        setupDone = false;
        RestartLevel();
        if (!restart)
        {
            startNode = null;
            foreach (Node n in spawnedNodes)
            {
                foreach (LineRenderer lr in n.GetLineRenderers())
                {
                    ReturnLineRenderer(lr);
                }
                n.connections.Clear();
                n.gameObject.SetActive(false);
            }
            spawnedNodes.Clear();
            spawnedLr.Clear();
            CreateNodes();
            SpawnNodes();
        }  
    }

    void RestartLevel()
    {
        foreach (Node n in spawnedNodes)
        {
            n.Reset();
        }
        treasuresLeft = settings.treasureCount;
    }

    void SpawnNodes()
    {
        StartCoroutine(Spawn());
    }

    IEnumerator Spawn()
    {
        int dataNodesCount = settings.nodeCount - settings.treasureCount - settings.spamCount - settings.firewallCount;
        GameSettings countsCopy = settings.Copy();
        for (int i = 0; i < settings.nodeCount + 1; i++)
        {
            Node node = nodesPool[nodesPool.Count - 1 - i];
            node.Reset();
            node.gameObject.SetActive(true);
            do
            {
                yield return null;
                node.trans.position = GetSpawnPosition(node);
            }
            while (IsNodeOverlapping(node));
            if (startNode == null)
            {
                node.type = Node.Type.start;
                node.UnlockNode();
                startNode = node;
            }
            else if (countsCopy.firewallCount > 0)
            {
                countsCopy.firewallCount--;
                node.type = Node.Type.firewall;
                firewallNodes.Add(node);
            }
            else if (countsCopy.spamCount > 0)
            {
                countsCopy.spamCount--;
                node.type = Node.Type.spam;
            }
            else if (countsCopy.treasureCount > 0)
            {
                countsCopy.treasureCount--;
                node.type = Node.Type.treasure;
            }
            else if (dataNodesCount > 0)
            {
                dataNodesCount--;
                node.type = Node.Type.data;
            }
            node.SetupNode();
            spawnedNodes.Add(node);
        }

        yield return new WaitForSeconds(0.1f);

        foreach (Node node in spawnedNodes)
        {
            node.ToggleCollider(false);
            foreach (Node n in spawnedNodes)
            {
                if (n != node && node.FindConnection(n) == null)
                {
                    n.ToggleCollider(false);
                    RaycastHit2D hit = Physics2D.Linecast(node.trans.position, n.trans.position);
                    if (!hit || hit.collider.tag != "Node")
                    {
                        LineRenderer lr = GetLineRenderer();
                        lr.enabled = true;
                        lr.SetPosition(0, node.trans.position);
                        lr.SetPosition(1, n.trans.position);
                        lr.startColor = Color.cyan;
                        lr.endColor = Color.cyan;
                        lr.sortingOrder = -2;
                        node.connections.Add(new Node.Connection() { connectedNode = n, lineRenderer = lr });
                        n.connections.Add(new Node.Connection() { connectedNode = node, lineRenderer = lr });
                        AddSpawnedLine(lr, new List<Node>() { node, n });
                    }
                    n.ToggleCollider(true);
                }
            }
            node.ToggleCollider(true);
        }
        FilterConnections();
        yield return new WaitForSeconds(0.1f);
        foreach (Node f in firewallNodes)
        {
            f.TriggerTracer();
        }
        setupDone = true;
    }

    void AddSpawnedLine(LineRenderer lr, List<Node> nodes)
    {
        float distance = Vector2.Distance(lr.GetPosition(0), lr.GetPosition(1));

        if (spawnedLr.Count == 0)
        {
            spawnedLr.Add(new ConnectionData() { lineRenderer = lr, nodes = nodes});
        }
        else
        {
            for (int i = spawnedLr.Count - 1; i >= 0; i--)
            {
                if (distance > Vector2.Distance(spawnedLr[i].lineRenderer.GetPosition(0), spawnedLr[i].lineRenderer.GetPosition(1)))
                {
                    if (i == 0)
                    {
                        spawnedLr.Insert(i, new ConnectionData() { lineRenderer = lr, nodes = nodes });
                        return;
                    }
                }
                else
                {
                    spawnedLr.Insert(i, new ConnectionData() { lineRenderer = lr, nodes = nodes });
                    return;
                }
            }
        }  
    }

    void FilterConnections()
    {
        for (int i = 0; i < spawnedLr.Count; i++)
        {
            spawnedLr[i].lineRenderer.name = i.ToString();
        }
        List <ConnectionData> removedLr = new List<ConnectionData>();
        for (int i = 0; i < spawnedLr.Count - 1; i++)
        {
            for (int j = i + 1; j < spawnedLr.Count; j++)
            {
                if (spawnedLr[i].lineRenderer.GetPosition(0) != spawnedLr[j].lineRenderer.GetPosition(0) && spawnedLr[i].lineRenderer.GetPosition(0) != spawnedLr[j].lineRenderer.GetPosition(1)
                    && spawnedLr[i].lineRenderer.GetPosition(1) != spawnedLr[j].lineRenderer.GetPosition(0) && spawnedLr[i].lineRenderer.GetPosition(1) != spawnedLr[j].lineRenderer.GetPosition(1))
                {
                    if (spawnedLr[i].nodes[0].connections.Count > 1 && spawnedLr[i].nodes[1].connections.Count > 1 && DoIntersect(spawnedLr[i].lineRenderer.GetPosition(0), spawnedLr[i].lineRenderer.GetPosition(1), spawnedLr[j].lineRenderer.GetPosition(0), spawnedLr[j].lineRenderer.GetPosition(1))/*spawnedLr[i].lineRenderer.bounds.Intersects(spawnedLr[j].lineRenderer.bounds)*/)
                    {
                        ReturnLineRenderer(spawnedLr[i].lineRenderer);
                        removedLr.Add(spawnedLr[i]);
                        spawnedLr[i].lineRenderer.name += "!!!!!" + spawnedLr[j].lineRenderer.name;
                        spawnedLr[i].nodes[0].RemoveConnection(spawnedLr[i].lineRenderer);
                        spawnedLr[i].nodes[1].RemoveConnection(spawnedLr[i].lineRenderer);
                        break;
                    }
                }
            }
        }
        foreach (ConnectionData lr in removedLr)
        {
            spawnedLr.Remove(lr);
        }
        startNode.UnlockConnectedNodes();
    }

    #region Line Intersection
    bool IsOnSegment(Vector2 p, Vector2 q, Vector2 r)
    {
        if (q.x <= Mathf.Max(p.x, r.x) && q.x >= Mathf.Min(p.x, r.x) &&
            q.y <= Mathf.Max(p.y, r.y) && q.y >= Mathf.Min(p.y, r.y))
            return true;

        return false;
    }

    int LineOrientation(Vector2 p, Vector2 q, Vector2 r)
    {
        float val = (q.y - p.y) * (r.x - q.x) -
                  (q.x - p.x) * (r.y - q.y);

        if (val == 0) return 0;

        return (val > 0) ? 1 : 2;
    }

    bool DoIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
    {
        int o1 = LineOrientation(p1, q1, p2);
        int o2 = LineOrientation(p1, q1, q2);
        int o3 = LineOrientation(p2, q2, p1);
        int o4 = LineOrientation(p2, q2, q1);

        if (o1 != o2 && o3 != o4)
            return true;

        if (o1 == 0 && IsOnSegment(p1, p2, q1)) return true;

        if (o2 == 0 && IsOnSegment(p1, q2, q1)) return true;

        if (o3 == 0 && IsOnSegment(p2, p1, q2)) return true;

        if (o4 == 0 && IsOnSegment(p2, q1, q2)) return true;

        return false;
    }
    #endregion

    public LineRenderer GetLineRenderer()
    {
        LineRenderer lr;
        if (lrPool.Count > 0)
        {
            lr = lrPool[lrPool.Count - 1];
            lrPool.RemoveAt(lrPool.Count - 1);
        }
        else
        {
            GameObject lrObject = Instantiate(lrPrefab, nodesHolder);
            lr = lrObject.GetComponent<LineRenderer>();
        }
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        return lr;
    }

    public void ReturnLineRenderer(LineRenderer lr)
    {
        if (!lrPool.Contains(lr))
        {
            lr.enabled = false;
            lrPool.Add(lr);
        }
    }

    bool IsNodeOverlapping(Node checkNode)
    {
        foreach (Node node in spawnedNodes)
        {
            if (checkNode.sRenderer.bounds.max.x < node.sRenderer.bounds.min.x)
                continue;
            if (checkNode.sRenderer.bounds.min.x > node.sRenderer.bounds.max.x)
                continue;
            if (checkNode.sRenderer.bounds.max.y < node.sRenderer.bounds.min.y)
                continue;
            if (checkNode.sRenderer.bounds.min.y > node.sRenderer.bounds.max.y)
                continue;
            return true;
        }
        return false;
    }

    Vector2 GetSpawnPosition(Node node)
    {
        float nodeWidth = node.sRenderer.bounds.size.x;
        float nodeHeight = node.sRenderer.bounds.size.y;
        return new Vector2(Random.Range(spawnRenderer.bounds.min.x + nodeHeight / 2, spawnRenderer.bounds.max.x - nodeHeight / 2), Random.Range(spawnRenderer.bounds.min.y + nodeWidth / 2, spawnRenderer.bounds.max.y - nodeWidth / 2));
    }

    void CreateNodes()
    {
        if (settings.nodeCount + 1 > nodesPool.Count)
        {
            int nodesToSpawn = settings.nodeCount + 1 - nodesPool.Count;
            for (int i = 0; i < nodesToSpawn; i++)
            {
                Node node = Instantiate(nodePrefab, nodesHolder, true).GetComponent<Node>();
                node.tag = "Node";
                nodesPool.Add(node);
            }
        }

        for (int i = 0; i < nodesPool.Count; i++)
        {
            nodesPool[i].name = i.ToString();
        }
    }

    public void ActivateTracer()
    {
        if (!tracerActive)
        {
            tracerActive = true;
            foreach (Node n in firewallNodes)
            {

            }
        }
    }

    public void LoadData()
    {
        settings.firewallCount = SaveController.currentSaveData.firewallCount;
        settings.spamCount = SaveController.currentSaveData.spamCount;
        settings.spamDecrease = SaveController.currentSaveData.spamDecrease;
        settings.nodeCount = SaveController.currentSaveData.nodeCount;
        settings.trapDelay = SaveController.currentSaveData.trapDelay;
        settings.treasureCount = SaveController.currentSaveData.treasureCount;
    }

    public void RandomizeNodesDifficulties()
    {
        foreach (Node n in spawnedNodes)
        {
            if (n.state != Node.State.hacked)
            {
                n.RandomizeDifficulty();
            }
        }
    }

    public void SetClickedNode(Node n)
    {
        if (clickedNode != null)
            clickedNode.ToggleSelection(false);
        else if (n != null)
        {
            n.ToggleSelection(true);
        }
        clickedNode = n;
    }

    public Node GetClickedNode()
    {
        return clickedNode;
    }

    public void TreasureHacked()
    {
        int randomReward = Random.Range(0, 3);
        switch (randomReward)
        {
            case 0: SaveController.currentSaveData.xpAmount += Random.Range((int)xpRewardRange.x, (int)xpRewardRange.y + 1); break;
            case 1: SaveController.currentSaveData.nukeCount += Random.Range((int)nukeRewardRange.x, (int)nukeRewardRange.y + 1); break;
            case 2: SaveController.currentSaveData.trapCount += Random.Range((int)trapRewardRange.x, (int)trapRewardRange.y + 1); break;
        }
        SaveController.WriteSaveData();
        UIManager.Instance.UpdateLevelUI();
        treasuresLeft--;
        if (treasuresLeft == 0)
        {
            MinigameWon();
        }
    }

    void MinigameWon()
    {
        foreach (Node n in spawnedNodes)
        {
            n.StopCoroutines();
        }

        UIManager.Instance.ShowWinPanel();
    }
}