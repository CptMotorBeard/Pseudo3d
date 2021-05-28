using UnityEngine;

public class CircuitEditorInputEvent
{
    public CanvasTransform Transform;
    public Vector2 CanvasMousePosition;
    public CircuitNode Node;

    public bool IsNodeFocused => Node != null;
}