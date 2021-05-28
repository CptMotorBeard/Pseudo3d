using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EditorAreaSelect
{
    public static IEnumerable<CircuitNode> NodesWithinArea(IEnumerable<CircuitNode> allNodes, Vector2 startPosition, Vector2 endPosition)
    {
        Rect selectionArea = SelectionArea(startPosition, endPosition);
        return allNodes.Where(node => selectionArea.Overlaps(node.RectPosition));
    }

    public static Rect SelectionArea(Vector2 start, Vector2 end)
    {
        float xMin, xMax, yMin, yMax;

        if (start.x < end.x)
        {
            xMin = start.x;
            xMax = end.x;
        }
        else
        {
            xMax = start.x;
            xMin = end.x;
        }

        if (start.y < end.y)
        {
            yMin = start.y;
            yMax = end.y;
        }
        else
        {
            yMax = start.y;
            yMin = end.y;
        }

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }
}

public struct DraggingNode
{
    public CircuitNode Node;
    public Vector2 Offset;
}

public static class EditorDrag
{
    public static Vector2 StartDrag(CircuitNode node, Vector2 startDragPosition)
    {
        return startDragPosition - node.Center;
    }

    public static IReadOnlyList<DraggingNode> StartDrag(IReadOnlyList<CircuitNode> nodes, Vector2 startDragPosition)
    {
        var draggingNodes = new List<DraggingNode>();
        foreach (var node in nodes)
        {
            var draggingNode = new DraggingNode
            {
                Node = node,
                Offset = StartDrag(node, startDragPosition)
            };

            draggingNodes.Add(draggingNode);
        }

        return draggingNodes;
    }

    public static void Drag(Vector2 dragPosition, CircuitNode node, Vector2 offset)
    {
        Vector2 newPosition = dragPosition - offset;
        float snapStep = CircuitDesignerPreferences.instance.SnapStep;
        node.Center = MathHelpers.SnapPosition(newPosition, snapStep);
    }

    public static void Drag(Vector2 dragPosition, IReadOnlyList<DraggingNode> nodes)
    {
        foreach (DraggingNode node in nodes)
        {
            Drag(dragPosition, node.Node, node.Offset);
        }
    }
}

public static class MathHelpers
{
    public static Vector2 SnapPosition(Vector2 point, float snapStep)
    {
        return SnapPosition(point.x, point.y, snapStep);
    }

    public static Vector2 SnapPosition(float x, float y, float snapStep)
    {
        return new Vector2(Snap(x, snapStep), Snap(y, snapStep));
    }

    public static float Snap(float val, float snapStep)
    {
        return Mathf.Round(val / snapStep) * snapStep;
    }
}