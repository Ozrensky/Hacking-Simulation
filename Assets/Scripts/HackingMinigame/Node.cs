using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Node : MonoBehaviour
{
    public enum Type { start, treasure, firewall, data, spam }
    public enum State { locked, unlocked, hacked}
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

    public List<LineRenderer> hackLrs = new List<LineRenderer>();
    public List<Connection> connections = new List<Connection>();
    public Type type;
    public State state = State.locked;
    public bool isTrap = false;
    public int hackingDifficulty = 1;
    public int chanceToTriggerTracer = 10;

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

    public List<LineRenderer> GetLineRenderers()
    {
        List<LineRenderer> lr = new List<LineRenderer>();
        foreach (Connection c in connections)
        {
            lr.Add(c.lineRenderer);
        }
        return lr;
    }

    public List<LineRenderer> GetHackLineRenderers()
    {
        return hackLrs;
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

        if (type != Type.start)
            RandomizeDifficulty();
        else
            hackingDifficulty = 0;
    }

    public void TriggerTracer()
    {
        if (!HackingController.tracerActive)
        {
            gameObject.AddComponent<TracerController>().CalculateTracerPath(this, new TracerController.Path());
        }
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

    public void LockNode()
    {
        state = State.locked;
        SetColor(lockedColor);
        onLockEvent.Invoke();
    }

    public void HackNode()
    {
        if (state == State.unlocked)
        {
            HackingController.hackingActive = true;
            hackCoroutine = StartCoroutine(HackCoroutine());
        }
    }

    public void Nuke()
    {
        SaveController.currentSaveData.nukeCount--;
        SaveController.WriteSaveData();
        UIManager.Instance.UpdateLevelUI();
        hackCoroutine = StartCoroutine(HackCoroutine(true));
    }

    IEnumerator HackCoroutine(bool isInstant = false)
    {
        LineRenderer lr = HackingController.Instance.GetLineRenderer();
        lr.startColor = Color.green;
        lr.endColor = Color.green;
        lr.SetPosition(0, GetNeighbourHackedNode().trans.position);
        Vector3 lerpPosition = trans.position;
        lr.sortingOrder = -1;
        lr.enabled = true;
        hackLrs.Add(lr);
        if (!isInstant)
        {
            lr.SetPosition(1, lr.GetPosition(0));
            hackingObject.SetActive(true);
            float elapsed = 0;
            float duration = hackingDifficulty * HackingController.Instance.baseHackTime;
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
        SetColor(hackedColor);
        state = State.hacked;
        onHackedEvent.Invoke();
        SaveController.currentSaveData.xpAmount += hackingDifficulty * 100;
        SaveController.WriteSaveData();
        UIManager.Instance.UpdateLevelUI();
        UnlockConnectedNodes();
        if (type == Type.treasure)
        {
            HackingController.Instance.TreasureHacked();
        }
        else if (type == Type.spam)
        {
            HackingController.Instance.RandomizeNodesDifficulties();
        }
        hackCoroutine = null;
    }

    Node GetNeighbourHackedNode()
    {
        foreach (Connection c in connections)
        {
            if (c.connectedNode.state == State.hacked || c.connectedNode.type == Type.start)
                return c.connectedNode;
        }

        return null;
    }

    public void UnlockNode()
    {
        state = State.unlocked;
        SetColor(unlockedColor);
        onUnlockEvent.Invoke();
    }

    public void TrapNode()
    {
        isTrap = true;
        trapObject.SetActive(true);
    }

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

    public void ToggleSelection(bool value)
    {
        selectionIndicator.enabled = value;
    }

    public void UnlockConnectedNodes()
    {
        foreach (Connection connection in connections)
        {
            if (connection.connectedNode.state == State.locked)
                connection.connectedNode.UnlockNode();
        }
    }
}