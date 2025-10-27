using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GraphEdge : MonoBehaviour
{
    [Header("Visual Settings")]
    [Range(0.001f, 0.2f)] public float lineWidth = 0.03f;
    public float nodeRadius = 0.3f;
    public float arrowOffset = 0.15f;

    private LineRenderer _lr;
    private Transform _from;
    private Transform _to;
    private bool _directed;
    private Transform _arrowHead;

    private void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        _lr.positionCount = 2;
        _lr.useWorldSpace = true;
        _lr.startWidth = _lr.endWidth = lineWidth;
        _arrowHead = transform.Find("ArrowHead");
    }

    public void Connect(Transform from, Transform to, bool directed = true)
    {
        _from = from;
        _to = to;
        _directed = directed;
        UpdatePositions();
    }

    private void Update()
    {
        if (_from == null || _to == null) return;
        UpdatePositions();
    }

    private void UpdatePositions()
    {
        Vector3 a = _from.position;
        Vector3 b = _to.position;

        Vector3 dir = (b - a).normalized;
        Vector3 start = a + dir * nodeRadius;
        Vector3 end = b - dir * nodeRadius;

        _lr.startWidth = _lr.endWidth = lineWidth;
        _lr.SetPosition(0, start);
        _lr.SetPosition(1, end);

        if (_directed && _arrowHead != null)
        {
            // place arrowhead a bit before the end
            Vector3 arrowPos = b - dir * (arrowOffset + nodeRadius);
            _arrowHead.position = arrowPos;

            // rotate so the arrow points *towards* 'to' node
            // assumes arrow sprite faces UP by default
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            _arrowHead.rotation = Quaternion.Euler(0, 0, angle - 90f);

            _arrowHead.gameObject.SetActive(true);
        }
        else if (_arrowHead != null)
        {
            _arrowHead.gameObject.SetActive(false);
        }
    }
}