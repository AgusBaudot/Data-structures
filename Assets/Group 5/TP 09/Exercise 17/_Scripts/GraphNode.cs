using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GraphNode : MonoBehaviour
{
    private GraphController controller;
    private SpriteRenderer sr;
    private Color defaultColor;

    public void Init(GraphController controller)
    {
        this.controller = controller;
        sr = GetComponent<SpriteRenderer>();
        defaultColor = sr != null ? sr.color : Color.white;
    }

    public void Highlight(bool active)
    {
        if (sr != null) sr.color = active ? Color.yellow : defaultColor;
    }

    public void OnMouseDown()
    {
        controller.SelectNode(this);
    }
}