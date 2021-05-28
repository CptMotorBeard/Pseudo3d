using UnityEditor;
using UnityEngine;

public static class DrawingHelper
{
    public static void DrawStaticGrid(Rect canvas, Texture2D texture)
    {
        var size = canvas.size;
        var center = size / 2f;

        float xOffset = -center.x / texture.width;
        float yOffset = (center.y - size.y) / texture.height;

        Vector2 tileOffset = new Vector2(xOffset, yOffset);

        float tileAmountX = Mathf.Round(size.x) / texture.width;
        float tileAmountY = Mathf.Round(size.y) / texture.height;

        Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

        GUI.DrawTextureWithTexCoords(canvas, texture, new Rect(tileOffset, tileAmount));
    }

    public static void DrawGrid(Rect canvas, Texture texture, float zoom, Vector2 pan)
    {
        var size = canvas.size;
        var center = size / 2f;

        float xOffset = -(center.x * zoom + pan.x) / texture.width;
        float yOffset = ((center.y - size.y) * zoom + pan.y) / texture.height;

        Vector2 tileOffset = new Vector2(xOffset, yOffset);

        float tileAmountX = Mathf.Round(size.x * zoom) / texture.width;
        float tileAmountY = Mathf.Round(size.y * zoom) / texture.height;

        Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

        GUI.DrawTextureWithTexCoords(canvas, texture, new Rect(tileOffset, tileAmount));
    }

    public static void DrawRectConnectionScreenSpace(Vector2 start, Vector2 end, Color color)
    {
        var originalColor = Handles.color;
        Handles.color = color;

        float halfDist = (start - end).magnitude / 2f;

        Vector2 directionToEnd = (end - start).normalized;
        Vector2 directionToStart = (start - end).normalized;

        Vector2 axisForTipAlignment = Vector3.up;

        Vector2 startTip = Vector3.Project(directionToEnd, axisForTipAlignment) * halfDist + (Vector3)start;
        Vector2 endTip = Vector3.Project(directionToStart, axisForTipAlignment) * halfDist + (Vector3)end;

        if (startTip == endTip)
        {
            Handles.DrawLine(start, end);
        }

        else
        {
            Handles.DrawLine(start, startTip);
            Handles.DrawLine(end, endTip);
            Handles.DrawLine(startTip, endTip);
        }

        Handles.color = originalColor;
    }

    // Helper method to draw textures with color tint.
    public static void DrawTexture(Rect r, Texture2D tex, Color c)
    {
        GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit, true, 0f, c, 0f, 0f);
    }

    public static void DrawLineCanvasSpace(CanvasTransform t, Vector2 start, Vector2 end, Color color)
    {
        start = t.CanvasToScreenSpace(start);
        end = t.CanvasToScreenSpace(end);
        DrawLineScreenSpace(start, end, color);
    }

    public static void DrawLineCanvasSpace(CanvasTransform t, Vector2 start, Vector2 end, Color color, float width)
    {
        start = t.CanvasToScreenSpace(start);
        end = t.CanvasToScreenSpace(end);
        if (t.IsScreenAxisLineInView(start, end))
        {
            DrawLineScreenSpace(start, end, color, width);
        }
    }

    public static void DrawLineScreenSpace(Vector2 start, Vector2 end, Color color)
    {
        var originalColor = Handles.color;
        Handles.color = color;
        Handles.DrawLine(start, end);
        Handles.color = originalColor;
    }

    public static void DrawLineScreenSpace(Vector2 start, Vector2 end, Color color, float width)
    {
        var originalColor = Handles.color;
        Handles.color = color;
        Handles.DrawAAPolyLine(width, start, end);
        Handles.color = originalColor;
    }

    public static void DrawRoad(CanvasTransform canvas, CircuitNode node, bool nodeIsSelected)
    {
        Rect screenRect = node.RectPosition;
        screenRect.position = canvas.CanvasToScreenSpace(screenRect.position);

        Vector2 fullSize = new Vector2(node.Icon.texture.width, node.Icon.texture.height);
        Vector2 size = new Vector2(node.Icon.textureRect.width, node.Icon.textureRect.height);

        Rect coords = node.Icon.textureRect;
        coords.x /= fullSize.x;
        coords.width /= fullSize.x;
        coords.y /= fullSize.y;
        coords.height /= fullSize.y;

        Vector2 ratio;
        ratio.x = screenRect.width / size.x;
        ratio.y = screenRect.height / size.y;
        float minRatio = Mathf.Min(ratio.x, ratio.y);

        Vector2 center = screenRect.center;
        screenRect.width = size.x * minRatio;
        screenRect.height = size.y * minRatio;
        screenRect.center = center;

        if (nodeIsSelected)
        {
            Color defaultColor = GUI.color;
            GUI.color = Color.blue;
            GUI.DrawTextureWithTexCoords(screenRect, node.Icon.texture, coords);
            GUI.color = defaultColor;
        }
        else
        {
            GUI.DrawTextureWithTexCoords(screenRect, node.Icon.texture, coords);
        }        
    }
}