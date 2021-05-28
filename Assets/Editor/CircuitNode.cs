using System;
using UnityEngine;

[Serializable]
public class CircuitNode
{
    [SerializeField] private Rect m_rectPosition;

    [NonSerialized] int m_nodeCircuitIndex;
    [NonSerialized] IRoadSectionBase m_segment;

    public Rect RectPosition => m_rectPosition;
    public Sprite Icon { get; private set; }

    public float Rotation { get; set; }

    public int Index
    {
        get { return m_nodeCircuitIndex; }
        set { m_nodeCircuitIndex = value; }
    }

    public IRoadSectionBase Segment
    {
        get { return m_segment; }
        set
        {
            m_segment = value;
            UpdateGUI();
        }
    }

    public Vector2 Position
    {
        get { return m_rectPosition.position; }
        set { m_rectPosition.position = value; }
    }

    public Vector2 Center
    {
        get { return m_rectPosition.center; }
        set { m_rectPosition.center = value; }
    }

    public Vector2 Size
    {
        get { return m_rectPosition.size; }
    }

    public CircuitNode(Sprite icon = null)
    {
        if (!icon)
        {
            icon = CircuitDesignerPreferences.instance.FallbackIcon;
        }

        Icon = icon;
    }

    public CircuitNode SetSegment(int circuitIndex, IRoadSectionBase segment, Sprite icon)
    {
        if (!icon)
        {
            icon = CircuitDesignerPreferences.instance.FallbackIcon;
        }

        Icon = icon;
        Segment = segment;
        m_nodeCircuitIndex = circuitIndex;

        return this;
    }

    public void UpdateGUI()
    {
        m_rectPosition.size = new Vector2(Icon.textureRect.width, Icon.textureRect.height);
    }

    public void SetIcon(Sprite icon)
    {
        if (!icon)
        {
            icon = CircuitDesignerPreferences.instance.FallbackIcon;
        }

        Icon = icon;
    }
}