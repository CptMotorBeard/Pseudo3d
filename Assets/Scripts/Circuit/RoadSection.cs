using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class IRoadSectionBase : ScriptableObject
{
    public abstract List<SectionBuilder> GetSections();
}

[Serializable]
[CreateAssetMenu(fileName = "RoadSection", menuName = "Game/RoadSection")]
public class RoadSection : IRoadSectionBase
{
    public SectionBuilder Segment;

    public override List<SectionBuilder> GetSections()
    {
        return new List<SectionBuilder>() { Segment };
    }
}