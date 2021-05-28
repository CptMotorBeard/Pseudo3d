using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CircuitCanvas
{
    private RoadSectionGroup m_circuit;
    public CircuitData CircuitData { get; }
    public IRoadSectionBase Root { get; private set; }

    public CircuitCanvas(RoadSectionGroup circuit)
    {
        string path = AssetDatabase.GetAssetPath(circuit);
        path = path.Replace(".asset", "_editor.asset");

        CircuitData = CircuitData.CreateOrLoad(path);

        m_circuit = circuit;
        Root = m_circuit.Segments.FirstOrDefault();

        for (int i = 0; i < m_circuit.Segments.Count; ++i)
        {
            var segment = m_circuit.Segments[i];

            if (i < CircuitData.Circuit.Count)
            {
                Vector2 position = CircuitData.Circuit[i].Position;
                CircuitData.Circuit[i].SetSegment(i, segment, CircuitDesignerPreferences.instance.GetTexture(segment));
                CircuitData.Circuit[i].Position = position;
            }
            else
            {
                CircuitData.Circuit.Add(new CircuitNode().SetSegment(i, segment, CircuitDesignerPreferences.instance.GetTexture(segment)));
            }            
        }
    }

    public void ReplaceSection(CircuitNode node, IRoadSectionBase roadSection)
    {
        m_circuit.Segments[node.Index] = roadSection;
        node.SetSegment(node.Index, roadSection, CircuitDesignerPreferences.instance.GetTexture(roadSection));
    }

    public void CreateSection(IRoadSectionBase roadSection)
    {
        CircuitData.Circuit.Add(new CircuitNode().SetSegment(m_circuit.Segments.Count, roadSection, CircuitDesignerPreferences.instance.GetTexture(roadSection)));
        m_circuit.Segments.Add(roadSection);
    }

    public bool RemoveSection(IRoadSectionBase roadSection)
    {
        int index = m_circuit.Segments.FindIndex(s => s == roadSection);
        if (index < 0)
            return false;

        for (int i = 0; i < CircuitData.Circuit.Count; ++i)
        {
            if (CircuitData.Circuit[i].Index == index)
            {
                CircuitData.Circuit.RemoveAt(i);
                --i;
            }
            else if (CircuitData.Circuit[i].Index > index)
            {
                --CircuitData.Circuit[i].Index;
            }
        }

        m_circuit.Segments.RemoveAt(index);

        return true;
    }

    public void PushToEnd(CircuitNode node)
    {
        bool removed = CircuitData.Circuit.Remove(node);
        if (removed)
        {
            CircuitData.Circuit.Add(node);
        }
    }
}