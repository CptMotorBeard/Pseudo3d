using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class CircuitEditor
{
    public CircuitCanvas Canvas { get; private set; }    
    public CircuitViewer Viewer { get; set; }

    public CircuitEditorInput Input { get; } = new CircuitEditorInput();
    public EditorSelection NodeSelection { get; } = new EditorSelection();


    public EventHandler CanvasChanged;

    private Action<CanvasTransform> m_motionAction;
    private Action<CircuitEditorInputEvent> m_applyAction;

    public CircuitEditor()
    {        
        NodeSelection.SingleSelected += OnSingleSelected;
        
        Input.Selection = NodeSelection;
        Input.MouseDown += OnMouseDown;
        Input.MouseUp += OnMouseUp;
        Input.Click += OnClick;
        Input.DoubleClick += OnDoubleClick;
        Input.CanvasLostFocus += OnCanvasLostFocus;
        Input.NodeContextClick += OnNodeContextClicked;
        Input.CanvasContextClick += OnCanvasContextClicked;

        Input.SwapRoadSegmentRequest += SwapRoadSegment;

        Input.DeleteRequest += OnDelete;

        Input.CreateRoadSegmentRequest += CreateSegment;
    }

    public void SetCircuit(RoadSectionGroup circuit)
    {
        Canvas = new CircuitCanvas(circuit);
        Viewer.Canvas = Canvas;
        Viewer.Selection = NodeSelection;
    }

    private void CreateSegment(object sender, IRoadSectionBase roadSegment)
    {
        Canvas.CreateSection(roadSegment);
    }

    private void OnDelete(object sender, EventArgs e)
    {
        if (NodeSelection.IsEmpty)
            return;

        foreach (var node in NodeSelection.SelectedNodes)
        {
            Canvas.RemoveSection(node.Segment);
        }
    }

    public void PollInput(Event e, CanvasTransform canvas, Rect inputRect)
    {
        if (e.type == EventType.MouseDrag)
        {
            if (m_motionAction != null)
            {
                m_motionAction(canvas);
                OnCanvasChanged();
            }
        }

        if (CircuitEditorInput.IsPanAction(e))
        {
            Pan(e.delta);
            OnCanvasChanged();
        }

        if (CircuitEditorInput.IsZoomAction(e))
        {
            if (m_motionAction != null)
            {
                RotateSelection(e.delta.y);
            }
            else
            {
                Zoom(e.delta.y);                
            }

            OnCanvasChanged();
        }

        Input.HandleMouseEvents(e, canvas, Canvas.CircuitData.Circuit, inputRect);
    }

    public void Pan(Vector2 delta)
    {
        Viewer.PanOffset += delta * Viewer.ZoomScale * CircuitViewer.PanSpeed;

        Viewer.PanOffset.x = Mathf.Round(Viewer.PanOffset.x);
        Viewer.PanOffset.y = Mathf.Round(Viewer.PanOffset.y);
    }

    public void Zoom(float zoomDirection)
    {
        float scale = (zoomDirection < 0) ? (1f - CircuitViewer.ZoomDelta) : (1f + CircuitViewer.ZoomDelta);
        Viewer.Zoom *= scale;

        float cap = Mathf.Clamp(Viewer.Zoom.x, CircuitViewer.MinZoom, CircuitViewer.MaxZoom);
        Viewer.Zoom.Set(cap, cap);
    }

    public void RotateSelection(float delta)
    {
        float scale = (delta < 0) ? (1f - CircuitDesignerPreferences.instance.RotateDelta) : (1f + CircuitDesignerPreferences.instance.RotateDelta);
        foreach (var node in NodeSelection.SelectedNodes)
        {
            node.Rotation += 1;
        }
    }

    private void ClearActions()
    {
        m_applyAction = null;
        m_motionAction = null;
        Viewer.CustomDraw = null;
        Viewer.CustomOverlayDraw = null;
    }

    private void OnCanvasLostFocus(object sender, EventArgs e)
    {
        ClearActions();
    }

    private void RemoveSelectedNodes()
    {
        Canvas.CircuitData.Circuit.RemoveAll(node => NodeSelection.IsNodeSelected(node));
    }

    private void OnCanvasChanged()
    {
        CanvasChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnMouseDown(object sender, CircuitEditorInputEvent inputEvent)
    {
        if (m_motionAction != null)
            return;

        if (inputEvent.IsNodeFocused)
        {
            if (!NodeSelection.IsNodeSelected(inputEvent.Node))
            {
                NodeSelection.SetSingleSelection(inputEvent.Node);
            }

            StartDrag(inputEvent);
        }
        else
        {
            StartAreaSelection(inputEvent);
        }
    }

    private void StartDrag(CircuitEditorInputEvent inputEvent)
    {
        if (NodeSelection.IsSingleSelection)
        {
            StartSingleDrag(inputEvent);
        }
        else if (NodeSelection.IsMultiSelection)
        {
            StartMultiDrag(inputEvent);
        }
    }

    private void StartSingleDrag(CircuitEditorInputEvent startEvent)
    {
        Vector2 offset = EditorDrag.StartDrag(startEvent.Node, startEvent.CanvasMousePosition);
        m_motionAction = (CanvasTransform canvas) => EditorDrag.Drag(CircuitEditorInput.MousePosition(canvas), startEvent.Node, offset);
    }

    private void StartMultiDrag(CircuitEditorInputEvent startEvent)
    {
        var nodes = EditorDrag.StartDrag(NodeSelection.SelectedNodes, startEvent.CanvasMousePosition);
        m_motionAction = (CanvasTransform canvas) => EditorDrag.Drag(CircuitEditorInput.MousePosition(canvas), nodes);
    }

    private void StartAreaSelection(CircuitEditorInputEvent inputEvent)
    {
        Vector2 startScreenSpace = Event.current.mousePosition;
        Vector2 startCanvasSpace = inputEvent.CanvasMousePosition;

        m_applyAction = (CircuitEditorInputEvent endEvent) =>
        {
            Vector2 endCanvasSpace = endEvent.CanvasMousePosition;
            var areaSelection = EditorAreaSelect.NodesWithinArea(Canvas.CircuitData.Circuit, startCanvasSpace, endCanvasSpace);
            NodeSelection.SetMultiSelected(areaSelection.ToList());
        };

        Viewer.CustomOverlayDraw = () =>
        {
            Vector2 endScreenSpace = Event.current.mousePosition;
            Rect selectionRect = EditorAreaSelect.SelectionArea(startScreenSpace, endScreenSpace);
            Color selectionColor = new Color(0f, 0.5f, 1f, 0.1f);
            Handles.DrawSolidRectangleWithOutline(selectionRect, selectionColor, Color.blue);
            OnCanvasChanged();
        };
    }

    private void OnClick(object sender, CircuitEditorInputEvent inputEvent)
    {
        if (inputEvent.IsNodeFocused && NodeSelection.IsMultiSelection)
        {
            NodeSelection.SetSingleSelection(inputEvent.Node);
        }
    }

    private void OnDoubleClick(object sender, CircuitEditorInputEvent inputEvent)
    {

    }

    private void OnNodeContextClicked(object sender, CircuitNode node)
    {
        NodeSelection.SetSingleSelection(node);
    }

    private void SwapRoadSegment(object sender, IRoadSectionBase roadSection)
    {
        Canvas.ReplaceSection(NodeSelection.SingleSelectedNode, roadSection);
    }

    private void OnCanvasContextClicked(object sender, EventArgs e)
    {
        
    }

    private void OnMouseUp(object sender, CircuitEditorInputEvent inputEvent)
    {
        m_applyAction?.Invoke(inputEvent);
        ClearActions();
    }

    private void OnSingleSelected(object sender, CircuitNode node)
    {
        Canvas.PushToEnd(node);
    }
}