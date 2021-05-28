using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "RoadSectionGroup", menuName = "Game/RoadSectionGroup")]
public class RoadSectionGroup : IRoadSectionBase
{
    [SerializeReference]
    public List<IRoadSectionBase> Segments = new List<IRoadSectionBase>();

    public override List<SectionBuilder> GetSections()
    {
        List<SectionBuilder> sections = new List<SectionBuilder>();
        foreach (var segment in Segments)
        {
            sections.AddRange(segment.GetSections());
        }

        return sections;
    }
}