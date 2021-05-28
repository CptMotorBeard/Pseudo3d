using System;
using UnityEngine;

[Serializable]
public struct SegmentData
{
    public Vector3 WorldPosition;
    public Vector3 ScreenPosition;
    public Vector3 CameraPosition;

    public int Index;

    public float Curve;
    public float Scale;
}