using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Node : MonoBehaviour
{
    public enum Type { start, treasure, firewall, data, spam }
    public enum State { locked, unlocked, hacked}
    [Header("Prefab Objects")]
    public GameObject startNodeObject;
    public GameObject treasureNodeObject;
    public GameObject firewallNodeObject;
    public GameObject dataNodeObject;
    public GameObject spamNodeObject;
    public GameObject trapObject;
    public GameObject hackingObject;
    public TextMeshPro hackingCounterText;
    public SpriteRenderer selectionIndicator;
    public List<SpriteRenderer> nodeSpriteRenderers = new List<SpriteRenderer>();
    public Color lockedColor;
    public Color unlockedColor;
    public Color hackedColor;
    [SerializeField] UnityEngine.Events.UnityEvent onLockEvent;
    [SerializeField] UnityEngine.Events.UnityEvent onUnlockEvent;
    [SerializeField] UnityEngine.Events.UnityEvent onHackedEvent;
    [SerializeField] UnityEngine.Events.UnityEvent onResetEvent;

    [HideInInspector] public List<LineRenderer> hackLrs = new List<LineRenderer>();
    [HideInInspector] public List<Connection> connections = new List<Connection>();
    [HideInInspector] public Type type;
    [HideInInspector] public State state = State.locked;
    [HideInInspector] public bool isTrap = false;
    [HideInInspector] public int hackingDifficulty = 1;
    [HideInInspector]public int chanceToTriggerTracer = 10;

    [HideInInspector] public Transform trans;
    [HideInInspector] public SpriteRenderer sRenderer;
    Collider2D coll;
    Coroutine hackCoroutine;
    Coroutine tracerCoroutine;
    Coroutine spamCoroutine;

    [System.Serializable]
    public class Connection
    {
        public Node connectedNode;
        public LineRenderer lineRenderer;
    }

    //resets node
    public void Reset()
    {
        LockNode();
        ResetTrap();
        onResetEvent.Invoke();
        StopCoroutines();
        hackingObject.SetActive(false);
        ToggleSelection(false);
        foreach (LineRenderer lr in hackLrs)
        {
            HackingController.Instance.ReturnLineRenderer(lr);
        }
        hackLrs.Clear();
    }

    public void StopCoroutines()
    {
        if (hackCoroutine != null)
        {
            StopCoroutine(hackCoroutine);
            hackCoroutine = null;
        }
        if (tracerCoroutine != null)
        {
            StopCoroutine(tracerCoroutine);
            tracerCoroutine = null;
        }
        if (spamCoroutine != null)
        {
            StopCoroutine(spamCoroutine);
            spamCoroutine = null;
        }
    }

    //randomizes node's difficulty
    public void RandomizeDifficulty()
    {
        float minDiff = HackingController.Instance.difficultyRange.x;
        float maxDiff = HackingController.Instance.difficultyRange.y;
        hackingDifficulty = Random.Range((int)minDiff, (int)maxDiff);
        chanceToTriggerTracer = hackingDifficulty * 10;
    }

    private void Awake()
    {
        trans = transform;
        sRenderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
        HackingController.OnLost += StopCoroutines;
        HackingController.OnWon += StopCoroutines;
    }

    //returns connection to a given node
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

    //returns connection lrs
    public List<LineRenderer> GetLineRenderers()
    {
        List<LineRenderer> lr = new List<LineRenderer>();
        foreach (Connection c in connections)
        {
            lr.Add(c.lineRenderer);
        }
        return lr;
    }

    //returns hack lrs
    public List<LineRenderer> GetHackLineRenderers()
    {
        return hackLrs;
    }

    //sets up node
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

        if (type != Type.start)
            RandomizeDifficulty();
        else
            hackingDifficulty = 0;
    }

    //triggers tracers
    public void TriggerTracers()
    {
        HackingController.Instance.ActivateTracers();
    }

    //removes connection and returns it's lr
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

    //removes connection by given lr
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

    //removes disabled connections
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

    //sets locked state
    public void LockNode()
    {
        state = State.locked;
        SetColor(lockedColor);
        onLockEvent.Invoke();
    }

    //instant hack
    public void Nuke()
    {
        if (!IsBeingHacked())
        {
            FirebaseController.Instance.NukeCount--;
            UIManager.Instance.UpdateLevelUI();
            hackCoroutine = StartCoroutine(HackCoroutine(true));
        }
    }

    //hack
    public void HackNode()
    {
        if (state == State.unlocked && !IsBeingHacked())
        {
            HackingController.hackingActive = true;
            hackCoroutine = StartCoroutine(HackCoroutine());
        }
    }

    //tracer hack
    public void TracerHack(Node previousNode)
    {
        if (type == Type.start)
        {
            hackCoroutine = StartCoroutine(HackCoroutine(true, previousNode));
        }
        else
        {
            hackCoroutine = StartCoroutine(HackCoroutine(false, previousNode));
        }
    }

    //hack coroutine for regular hack, nuke (instant) hack and tracer hack
    IEnumerator HackCoroutine(bool isInstant = false, Node previousNode = null)
    {
        float timeMultiplier = 1f;
        //setting up hack line
        LineRenderer lr = HackingController.Instance.GetLineRenderer();
        if (previousNode != null)
        {
            lr.startColor = Color.red;
            lr.endColor = Color.red;
            lr.SetPosition(0, previousNode.trans.position);
            lr.sortingOrder = -1;
            if (HackingController.spamActive)
                timeMultiplier += HackingController.Instance.settings.spamDecrease / 100;
        }
        else
        {
            lr.startColor = Color.green;
            lr.endColor = Color.green;
            lr.SetPosition(0, GetNeighbourHackedNode().trans.position);
            lr.sortingOrder = -2;
        }
        Vector3 lerpPosition = trans.position;
        lr.enabled = true;
        hackLrs.Add(lr);
        if (!isInstant)
        {
            lr.SetPosition(1, lr.GetPosition(0));
            hackingObject.SetActive(true);
            //lerping hack line
            float elapsed = 0;
            float duration = hackingDifficulty * HackingController.Instance.baseHackTime * timeMultiplier;
            {
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    hackingCounterText.text = (elapsed / duration * 100).ToString("0.0") + "%";
                    lr.SetPosition(1, Vector3.Lerp(lr.GetPosition(0), lerpPosition, elapsed / duration));
                    yield return null;
                }
            }
            hackingObject.SetActive(false);
            HackingController.hackingActive = false;
        }
        else
        {
            lr.SetPosition(1, lerpPosition);
        }
        if (previousNode == null)
        {
            //setting hacked state and adding hack reward, treasure reward etc.
            SetColor(hackedColor);
            state = State.hacked;
            onHackedEvent.Invoke();
            FirebaseController.Instance.XpAmount += hackingDifficulty * 100;
            UIManager.Instance.UpdateLevelUI();
            UnlockConnectedNodes();
            if (type == Type.treasure)
            {
                HackingController.Instance.TreasureHacked();
            }
            else if (type == Type.spam)
            {
                HackingController.Instance.RandomizeNodesDifficulties();
                HackingController.spamActive = true;
                UIManager.Instance.UpdateLevelUI();
                TriggerTracers();
            }
            else if (type == Type.firewall && !HackingController.tracersActive)
            {
                HackingController.Instance.FirewallHacked();
            }
            //calculate if a node is going to trigger tracers
            if (type != Type.spam && !HackingController.tracersActive)
                IsTriggeringTracers();
        }
        //if tracer is hacking and this node is start node, trigger lose
        else if (type == Type.start)
        {
            HackingController.Instance.MinigameFailed();
        }

        hackCoroutine = null;
    }

    //calculate if a node is going to trigger tracers
    void IsTriggeringTracers()
    {
        int random = Random.Range(1, 101);
        if (random > 0 && random < chanceToTriggerTracer)
        {
            TriggerTracers();
        }
    }

    //gets random hacked neighbour
    Node GetNeighbourHackedNode()
    {
        foreach (Connection c in connections)
        {
            if (c.connectedNode.state == State.hacked || c.connectedNode.type == Type.start)
                return c.connectedNode;
        }

        return null;
    }

    //sets unlocked state
    public void UnlockNode()
    {
        state = State.unlocked;
        SetColor(unlockedColor);
        onUnlockEvent.Invoke();
    }

    //sets a trap
    public void TrapNode()
    {
        isTrap = true;
        trapObject.SetActive(true);
    }

    //removes a trap
    public void ResetTrap()
    {
        isTrap = false;
        trapObject.SetActive(false);
    }

    void SetColor(Color color)
    {
        foreach (SpriteRenderer sr in nodeSpriteRenderers)
        {
            sr.color = color;
        }
    }

    //shows action panel on node click or disables it
    public void OnClick()
    {
        if (state == State.unlocked && state != State.hacked && !HackingController.hackingActive && type != Type.start)
        {
            UIManager.Instance.CloseActionPanel();
            UIManager.Instance.ShowActionPanel(this);
        }
        else
        {
            UIManager.Instance.CloseActionPanel();
        }
    }

    //toggles selection indicator
    public void ToggleSelection(bool value)
    {
        selectionIndicator.enabled = value;
    }

    //unlocks connected nodes
    public void UnlockConnectedNodes()
    {
        foreach (Connection connection in connections)
        {
            if (connection.connectedNode.state == State.locked)
                connection.connectedNode.UnlockNode();
        }
    }

    //returns if node is currently being hacked
    public bool IsBeingHacked()
    {
        if (hackCoroutine != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}