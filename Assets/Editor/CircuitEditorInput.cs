using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CircuitEditorInput : IDisposable
{
    private readonly GenericMenu m_roadSectionSelectionMenu = new GenericMenu();

    public event EventHandler<CircuitEditorInputEvent> MouseDown;
    public event EventHandler<CircuitEditorInputEvent> Click;
    public event EventHandler<CircuitEditorInputEvent> DoubleClick;
    public event EventHandler<CircuitEditorInputEvent> MouseUp;
    public event EventHandler<CircuitNode> NodeContextClick;

    public event EventHandler<IRoadSectionBase> CreateRoadSegmentRequest;
    public event EventHandler<IRoadSectionBase> SwapRoadSegmentRequest;

    public event EventHandler SaveRequest;
    public event EventHandler DeleteRequest;
    public event EventHandler CanvasContextClick;
    public event EventHandler CanvasLostFocus;

    public IReadOnlySelection Selection { get; set; }

    // Keeps track of time between mouse down and mouse up to determine if the event was a click.
    private readonly System.Timers.Timer m_clickTimer = new System.Timers.Timer(120);

    private readonly System.Timers.Timer m_doubleClickTimer = new System.Timers.Timer(400);

    // Keeps track of the number of quick clicks in succession. The time threshold is determined by clickTimer.
    private int m_quickClicksCount = 0;

    public CircuitEditorInput()
    {
        m_clickTimer.AutoReset = false;
        m_doubleClickTimer.AutoReset = false;

        m_doubleClickTimer.Elapsed += (s, e) => m_quickClicksCount = 0;

        m_roadSectionSelectionMenu.AddDisabledItem(new GUIContent("Create Piece"));
        m_roadSectionSelectionMenu.AddSeparator("");

        foreach (var item in CircuitDesignerPreferences.instance.PossibleSections)
        {
            m_roadSectionSelectionMenu.AddItem(new GUIContent(item.RoadSection.name), false, OnCreateRoadSegmentRequest, item.RoadSection);
        }
    }

    private void OnCreateRoadSegmentRequest(object o)
    {
        CreateRoadSegmentRequest?.Invoke(this, o as IRoadSectionBase);
    }

    public void HandleMouseEvents(Event e, CanvasTransform canvas, IReadOnlyList<CircuitNode> allNodes, Rect inputRect)
    {
        if (!inputRect.Contains(e.mousePosition))
        {
            CanvasLostFocus?.Invoke(this, EventArgs.Empty);
            return;
        }

        HandleClickActions(canvas, allNodes, e);
        HandleEditorShortcuts(e);

        if (e.type == EventType.ContextClick)
        {
            HandleContextInput(canvas, allNodes);
            e.Use();
        }
    }

    private void HandleClickActions(CanvasTransform canvas, IReadOnlyList<CircuitNode> allNodes, Event e)
    {
        if (IsClickAction(e))
        {
            if (m_quickClicksCount == 0)
                m_doubleClickTimer.Start();

            m_clickTimer.Start();
            MouseDown?.Invoke(this, CreateInputEvent(canvas, allNodes));
        }
        else if (IsUnlickAction(e))
        {
            CircuitEditorInputEvent inputEvent = CreateInputEvent(canvas, allNodes);
            if (m_clickTimer.Enabled)
                Click?.Invoke(this, inputEvent);

            if (m_doubleClickTimer.Enabled)
                ++m_quickClicksCount;

            if (m_quickClicksCount >= 2)
            {
                DoubleClick?.Invoke(this, inputEvent);
                m_doubleClickTimer.Stop();
                m_quickClicksCount = 0;
            }

            m_clickTimer.Stop();
            MouseUp?.Invoke(this, inputEvent);
        }
    }

    private void HandleEditorShortcuts(Event e)
    {        
        if (e.type == EventType.KeyUp)
        {
            if (e.control && e.keyCode == KeyCode.S)
            {
                e.Use();
                SaveRequest?.Invoke(this, EventArgs.Empty);
            }
            else if (e.keyCode == KeyCode.Delete)
            {
                e.Use();
                DeleteRequest?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void HandleContextInput(CanvasTransform canvas, IReadOnlyList<CircuitNode> allNodes)
    {
        var node = NodeUnderMouse(canvas, allNodes);
        if (node != null && Selection.IsSingleSelection)
        {
            NodeContextClick?.Invoke(this, node);
            CreateNodeSelectionContextMenu(node).ShowAsContext();
        }
        else
        {
            CanvasContextClick?.Invoke(this, EventArgs.Empty);
            ShowCreateRoadMenu();
        }        
    }

    private GenericMenu CreateNodeSelectionContextMenu(CircuitNode node)
    {
        var menu = new GenericMenu();
        menu.AddDisabledItem(new GUIContent("Change type"));
        menu.AddSeparator("");

        foreach (var item in CircuitDesignerPreferences.instance.PossibleSections)
        {
            menu.AddItem(new GUIContent(item.RoadSection.name), false, OnSwapRoadSegmentRequest, item.RoadSection);
        }

        return menu;
    }

    private void OnSwapRoadSegmentRequest(object o)
    {
        SwapRoadSegmentRequest?.Invoke(this, o as IRoadSectionBase);
    }

    public void ShowCreateRoadMenu()
    {
        m_roadSectionSelectionMenu.ShowAsContext();
    }

    private static bool IsClickAction(Event e)
    {
        return e.type == EventType.MouseDown && e.button == 0;
    }

    private static bool IsUnlickAction(Event e)
    {
        return e.type == EventType.MouseUp && e.button == 0;
    }

    public static bool IsPanAction(Event e)
    {
        return e.type == EventType.MouseDrag && e.button == 2;
    }

    public static bool IsZoomAction(Event e)
    {
        return e.type == EventType.ScrollWheel;
    }

    public static Vector2 MousePosition(CanvasTransform canvas)
    {
        return canvas.ScreenToCanvasSpace(Event.current.mousePosition);
    }

    public static bool IsUnderMouse(CanvasTransform transform, Rect r)
    {
        return r.Contains(MousePosition(transform));
    }

    private static CircuitNode NodeUnderMouse(CanvasTransform canvas, IReadOnlyList<CircuitNode> allNodes)
    {
        for (int i = allNodes.Count - 1; i >= 0; --i)
        {
            if (IsUnderMouse(canvas, allNodes[i].RectPosition))
            {
                return allNodes[i];
            }
        }

        return null;
    }

    private static CircuitEditorInputEvent CreateInputEvent(CanvasTransform canvas, IReadOnlyList<CircuitNode> allNodes)
    {
        return new CircuitEditorInputEvent
        {
            Transform = canvas,
            CanvasMousePosition = MousePosition(canvas),
            Node = NodeUnderMouse(canvas, allNodes)
        };
    }

    public void Dispose()
    {
        m_clickTimer.Dispose();
        m_doubleClickTimer.Dispose();
    }
}