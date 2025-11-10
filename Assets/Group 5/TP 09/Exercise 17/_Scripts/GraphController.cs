using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GraphController : MonoBehaviour
{
    [Header("Prefabs & Parents")]
    public GameObject nodePrefab;
    public GameObject edgePrefab;
    public GameObject edgeWeightPrefab;
    public Transform nodeParent;
    public Transform edgeParent;

    [Header("UI References")]
    public Button addNodeButton;
    public Button connectButton;
    public Button deleteButton;
    public Button checkPathButton;
    public Toggle pathModeToggle;
    public TMP_Text resultText;
    public RectTransform nodeSpawnRect;

    private MyALGraph<GameObject> _graph = new();
    private List<GraphNode> _nodes = new();

    private readonly Dictionary<(GameObject from, GameObject to), GameObject> _edgeObjects
        = new();

    private GraphNode selectedNodeA;
    private GraphNode selectedNodeB;

    // For path mode
    private List<GraphNode> pathSelection = new();
    private bool PathMode => pathModeToggle != null && pathModeToggle.isOn;

    private void Start()
    {
        addNodeButton.onClick.AddListener(CreateNode);
        connectButton.onClick.AddListener(ConnectSelected);
        deleteButton.onClick.AddListener(DeleteSelected);
        checkPathButton.onClick.AddListener(CheckPath);

        if (resultText != null)
            resultText.text = "";
    }

    void CreateNode()
    {
        Vector3 pos = new Vector3();
        bool valid = true;
        do
        {
            pos = Camera.main != null ? (Vector3)GetRandomPositionInRect(nodeSpawnRect) : Vector3.zero;
            if (Physics2D.OverlapCircleAll(pos, 0.5f).Length > 0)
            {
                //Colliding with something
                valid = false;
            }
            else
            {
                valid = true;
            }
        } while (!valid);
        
        GameObject nodeObj = Instantiate(nodePrefab, pos, Quaternion.identity, nodeParent);
        var node = nodeObj.GetComponent<GraphNode>();
        node.Init(this);
        _nodes.Add(node);
        _graph.AddVertex(nodeObj);
    }

    public void SelectNode(GraphNode node)
    {
        if (PathMode)
        {
            // Path selection mode
            pathSelection.Add(node);
            node.Highlight(true);
            return;
        }

        // Normal selection (for connecting/deleting)
        if (selectedNodeA == null)
        {
            selectedNodeA = node;
            node.Highlight(true);
            return;
        }

        if (selectedNodeA == node)
        {
            ClearSelection();
            return;
        }

        if (selectedNodeB == null)
        {
            selectedNodeB = node;
            node.Highlight(true);
            return;
        }

        ClearSelection();
    }

    void ConnectSelected()
    {
        if (selectedNodeA == null || selectedNodeB == null) return;

        GameObject from = selectedNodeA.gameObject;
        GameObject to = selectedNodeB.gameObject;

        // Calculate weight as distance between nodes
        float weight = Vector3.Distance(from.transform.position, to.transform.position);

        bool added = _graph.AddEdge(from, to, weight);
        if (added)
            CreateVisualEdge(from, to, weight);
        else
            Debug.Log("Edge already exists.");

        ClearSelection();
    }

    void CreateVisualEdge(GameObject from, GameObject to, float weight)
    {
        var key = (from, to);
        if (_edgeObjects.ContainsKey(key)) return;

        GameObject edgeObj = Instantiate(edgePrefab, edgeParent);
        var edgeComp = edgeObj.GetComponent<GraphEdge>();
        edgeComp.Connect(from.transform, to.transform, directed: true);

        // Optionally, display the weight value in the scene
        var text = edgeObj.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = weight.ToString("F2");
            //Location would be half point between 2 nodes.
            //text.gameObject.transform.position += pos / 2;
            Vector2 pos = to.transform.position - from.transform.position;
            text.gameObject.transform.position = new Vector2(pos.x, text.gameObject.transform.position.y + pos.y / 2);
        }

        _edgeObjects[key] = edgeObj;
    }

    void DeleteSelected()
    {
        if (selectedNodeA != null)
        {
            GameObject nodeObj = selectedNodeA.gameObject;

            _graph.RemoveVertex(nodeObj);
            RemoveAllVisualEdgesFor(nodeObj);

            _nodes.Remove(selectedNodeA);
            Destroy(nodeObj);
        }

        ClearSelection();
    }

    private void RemoveAllVisualEdgesFor(GameObject nodeObj)
    {
        var toRemove = new List<(GameObject, GameObject)>();
        foreach (var kv in _edgeObjects)
        {
            if (kv.Key.from == nodeObj || kv.Key.to == nodeObj)
                toRemove.Add(kv.Key);
        }

        foreach (var k in toRemove)
        {
            if (_edgeObjects.TryGetValue(k, out var go) && go != null)
                Destroy(go);
            _edgeObjects.Remove(k);
        }
    }

    public void RemoveEdge(GameObject from, GameObject to)
    {
        _graph.RemoveEdge(from, to);
        var key = (from, to);
        if (_edgeObjects.TryGetValue(key, out var go) && go != null) Destroy(go);
        _edgeObjects.Remove(key);
    }

    void ClearSelection()
    {
        if (selectedNodeA) selectedNodeA.Highlight(false);
        if (selectedNodeB) selectedNodeB.Highlight(false);
        selectedNodeA = selectedNodeB = null;
    }
    
    void CheckPath()
    {
        if (pathSelection.Count < 2)
        {
            resultText.text = "Select at least 2 nodes for a path.";
            return;
        }

        double totalCost = 0;
        bool valid = true;

        for (int i = 0; i < pathSelection.Count - 1; i++)
        {
            var from = pathSelection[i].gameObject;
            var to = pathSelection[i + 1].gameObject;

            if (_graph.ContainsEdge(from, to))
            {
                totalCost += _graph.GetWeight(from, to);
            }
            else
            {
                valid = false;
                break;
            }
        }

        resultText.text = valid
            ? $"Valid path. Total cost: {totalCost:F2}"
            : "Invalid path: missing edge(s).";

        // Clear highlights after check
        foreach (var n in pathSelection)
            n.Highlight(false);
        pathSelection.Clear();
    }

    private Vector2 GetRandomPositionInRect(RectTransform rect)
    {
        Rect r = rect.rect;
        Vector2 localPos = new Vector2(
            Random.Range(r.xMin, r.xMax),
            Random.Range(r.yMin, r.yMax)
        );

        // position in world space of that rect-local point (on canvas plane)
        Vector3 worldOnCanvas = rect.TransformPoint(localPos);

        // convert that world pos to screen coords (works with overlay if cam == null)
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, worldOnCanvas);

        // convert screen -> world using your game camera; supply z distance from camera
        Vector3 screenPosWithZ = new Vector3(screenPos.x, screenPos.y, -10);
        return Camera.main.ScreenToWorldPoint(screenPosWithZ);
    }
}
