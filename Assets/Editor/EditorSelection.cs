using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public interface IReadOnlySelection
{
    IReadOnlyList<CircuitNode> SelectedNodes { get; }
    CircuitNode SingleSelectedNode { get; }

    bool IsNodeSelected(CircuitNode node);

    int SelectedCount { get; }

    bool IsEmpty { get; }
    bool IsSingleSelection { get; }
    bool IsMultiSelection { get; }
}

public class EditorSelection : IReadOnlySelection
{
    public event EventHandler<CircuitNode> SingleSelected;

    private readonly List<CircuitNode> m_selectedNodes = new List<CircuitNode>();
    public IReadOnlyList<CircuitNode> SelectedNodes => m_selectedNodes;

    public CircuitNode SingleSelectedNode => SelectedNodes.FirstOrDefault();
    public CircuitNode FirstNodeSelected => SelectedNodes.FirstOrDefault();

    public int SelectedCount => SelectedNodes.Count;

    public bool IsEmpty => SelectedCount == 0;

    public bool IsSingleSelection => SelectedCount == 1;

    public bool IsMultiSelection => SelectedCount > 1;

    public bool IsNodeSelected(CircuitNode node)
    {
        return SelectedNodes.Contains(node);
    }

    public void SetMultiSelected(List<CircuitNode> newSelection)
    {
        if (newSelection.Count == 1)
        {
            SetSingleSelection(newSelection[0]);
        }
        else
        {
            m_selectedNodes.Clear();
            if (newSelection.Count > 0)
            {
                m_selectedNodes.AddRange(newSelection);
            }
        }
    }

    public void SetSingleSelection(CircuitNode newSingleSelection)
    {
        m_selectedNodes.Clear();
        m_selectedNodes.Add(newSingleSelection);
        SingleSelected?.Invoke(this, newSingleSelection);
    }

    public void ToggleSelection(CircuitNode node)
    {
        if (IsNodeSelected(node))
        {
            m_selectedNodes.Remove(node);
        }
        else
        {
            m_selectedNodes.Add(node);
        }

        if (IsSingleSelection)
        {
            SingleSelected?.Invoke(this, SingleSelectedNode);
        }
    }

    public void ClearSelection()
    {
        if (!IsEmpty)
        {
            m_selectedNodes.Clear();
        }
    }
}