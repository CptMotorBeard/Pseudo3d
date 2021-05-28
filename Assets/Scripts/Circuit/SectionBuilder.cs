using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SectionBuilder
{
    public Road.Length EaseInSegments;
    public Road.Length MainSegments;
    public Road.Length EaseOutSegments;

    public Road.Curve Curve;

    public Road.Hill Hill;

    public void Build(ref List<SegmentData> segments, GameConfig gameConfig)
    {
        int easeInSegments = (int)EaseInSegments;
        int mainSegments = (int)MainSegments;
        int easeOutSegments = (int)EaseOutSegments;

        int curve = (int)Curve;
        int hill = (int)Hill;

        float startY = segments.Count == 0 ? 0 : segments[segments.Count - 1].WorldPosition.y;
        float endY = startY + hill * gameConfig.SegmentLength;
        float totalSegments = easeInSegments + mainSegments + easeOutSegments;

        int i;

        for (i = 0; i < easeInSegments; ++i)
        {
            CreateSegment(ref segments, gameConfig,
                EaseIn(0, curve, i / (float)easeInSegments), EaseInOut(startY, endY, i / totalSegments));
        }

        for (i = 0; i < mainSegments; ++i)
        {
            CreateSegment(ref segments, gameConfig,
                curve, EaseInOut(startY, endY, (easeInSegments + i) / totalSegments));
        }

        for (i = 0; i < easeOutSegments; ++i)
        {
            CreateSegment(ref segments, gameConfig,
                EaseInOut(curve, 0, i / (float)easeOutSegments), EaseInOut(startY, endY, (easeInSegments + mainSegments + i) / totalSegments));
        }
    }

    private void CreateSegment(ref List<SegmentData> segments, GameConfig gameConfig, float curve, float yPos)
    {
        int index = segments.Count;
        segments.Add(new SegmentData
        {
            Curve = curve,
            Index = index,
            Scale = -1,

            WorldPosition = new  Vector3(0, yPos, index * gameConfig.SegmentLength),
            ScreenPosition = Vector3.zero,
            CameraPosition = Vector3.zero
        });
    }

    private float EaseIn(float a, float b, float percent)
    {
        return a + (b - a) * Mathf.Pow(percent, 2);
    }

    private float EaseOut(float a, float b, float percent)
    {
        return a + (b - a) * (1 - Mathf.Pow((1 - percent), 2));
    }

    private float EaseInOut(float a, float b, float percent)
    {
        return a + (b - a) * ((-Mathf.Cos(percent * Mathf.PI) / 2) + 0.5f);
    }
}

public static class SectionBuilderExt
{
    public static List<SegmentData> Build<T>(this List<T> sectionBuilders, GameConfig gameConfig) where T : SectionBuilder
    {
        List<SegmentData> segments = new List<SegmentData>();

        foreach (var sectionBuilder in sectionBuilders)
        {
            sectionBuilder.Build(ref segments, gameConfig);
        }

        return segments;
    }
}