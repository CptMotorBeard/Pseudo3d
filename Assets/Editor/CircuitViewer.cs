using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class CircuitViewer
{
    public static float ZoomDelta => Preferences.ZoomDelta;
    public static float MinZoom => Preferences.MinZoom;
    public static float MaxZoom => Preferences.MaxZoom;
    public static float PanSpeed => Preferences.PanSpeed;

    public Vector2 Zoom = Vector2.one;
    public Vector2 PanOffset = Vector2.one;
    public float ZoomScale => Zoom.x;

    public IReadOnlySelection Selection;

    public CircuitCanvas Canvas { get; set; }

    public Action<CanvasTransform> CustomDraw;
    public Action CustomOverlayDraw;

    private static CircuitDesignerPreferences Preferences => CircuitDesignerPreferences.instance;

    private readonly GUIStyle m_statusStyle = new GUIStyle { fontSize = 36, fontStyle = FontStyle.Bold };
    private readonly Rect m_statusRect = new Rect(20f, 20f, 250f, 150f);

    private string m_emptyCircuitLabel = "No Circuit Set";

    public CircuitViewer()
    {
        m_statusStyle.normal.textColor = new Color(1f, 1f, 1f, 0.2f);
    }

    public void Draw(CanvasTransform canvas)
    {
        if (Event.current.type == EventType.Repaint)
        {
            DrawGrid(canvas);
            DrawCanvasContents(canvas);
        }        
    }

    public void DrawStaticGrid(Vector2 size)
    {
        var canvasRect = new Rect(Vector2.zero, size);
        DrawingHelper.DrawStaticGrid(canvasRect, Preferences.GridTexture);
    }

    public void DrawStatus()
    {
        GUI.Label(m_statusRect, m_emptyCircuitLabel, m_statusStyle);
    }

    private void DrawGrid(CanvasTransform canvas)
    {
        var canvasRect = new Rect(Vector2.zero, canvas.Size);
        DrawingHelper.DrawGrid(canvasRect, Preferences.GridTexture, ZoomScale, PanOffset);
    }

    private void DrawCanvasContents(CanvasTransform canvas)
    {
        var canvasRect = new Rect(Vector2.zero, canvas.Size);
        ScaleUtility.BeginScale(canvasRect, ZoomScale, CircuitWindow.kToolbarHeight);

        CustomDraw?.Invoke(canvas);
        DrawConnections(canvas);
        DrawRoads(canvas);

        ScaleUtility.EndScale(canvasRect, ZoomScale, CircuitWindow.kToolbarHeight);

        CustomOverlayDraw?.Invoke();
    }

    private void DrawConnections(CanvasTransform canvas)
    {
        foreach (var node in Canvas.CircuitData.Circuit)
        {
            var nextNode = Canvas.CircuitData.Circuit.Find(n => n.Index == node.Index + 1);
            if (nextNode != null)
            {
                DrawingHelper.DrawLineCanvasSpace(canvas, node.Center, nextNode.Center, CircuitDesignerPreferences.instance.ConnectionColor, CircuitDesignerPreferences.instance.ConnectionWidth);
            }
        }
    }

    private void DrawRoads(CanvasTransform canvas)
    {
        foreach (var node in Canvas.CircuitData.Circuit)
        {
            DrawingHelper.DrawRoad(canvas, node, Selection.IsNodeSelected(node));
        }        
    }
}