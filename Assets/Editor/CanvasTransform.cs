using UnityEngine;

public struct CanvasTransform
{
    public Vector2 Pan;
    public float Zoom;
    public Vector2 Size;
    
    public bool InView(Rect rectInCanvasSpace)
    {
        var rectInScreenSpace = new Rect(CanvasToScreenSpace(rectInCanvasSpace.position), rectInCanvasSpace.size);
        var viewRect = new Rect(Vector2.zero, Size * Zoom);
        return viewRect.Overlaps(rectInScreenSpace);
    }

    public bool IsScreenAxisLineInView(Vector2 start, Vector2 end)
    {
        var lineBox = new Rect { position = start, max = end };
        Rect viewRect = new Rect(Vector2.zero, Size * Zoom);
        return viewRect.Overlaps(lineBox);
    }

    public Vector2 CanvasToScreenSpace(Vector2 canvasPosition)
    {
        return (0.5f * Size * Zoom) + Pan + canvasPosition;
    }

    public Vector2 ScreenToCanvasSpace(Vector2 screenPosition)
    {
        return (screenPosition - 0.5f * Size) * Zoom - Pan;
    }
}