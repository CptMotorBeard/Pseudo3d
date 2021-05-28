using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CircuitDesignerPreferences : ScriptableObject
{
    [Serializable]
    public class RoadSectionWrapper
    {
        public IRoadSectionBase RoadSection;
        public Sprite RoadTexture;
    }

    public int SnapStep = 12;
    public float ZoomDelta = 0.2f;
    public float RotateDelta = 0.2f;

    [Min(0.1f)]
    public float MinZoom = 1f;

    public float MaxZoom = 5f;
    public float PanSpeed = 1.2f;

    [Space()]
    public Texture2D GridTexture;
    public Sprite FallbackIcon;

    [Space()]
    public Color ConnectionColor;
    public float ConnectionWidth = 1.5f;

    [Space()]
    public List<RoadSectionWrapper> PossibleSections;

    private const string kFilePath = "Assets/CircuitDesignerPreferences.asset";

    private static CircuitDesignerPreferences s_instance;
    public static CircuitDesignerPreferences instance
    {
        get
        {
            if (s_instance == null)
                CreateAndLoad();

            return s_instance;
        }
    }

    public Sprite GetTexture(IRoadSectionBase segment)
    {
        var found = PossibleSections.Find(wrapper => wrapper.RoadSection == segment);
        if (found != null)
        {
            return found.RoadTexture;
        }

        return null;
    }

    protected CircuitDesignerPreferences()
    {
        if (s_instance != null)
        {
            Debug.LogError("ScriptableSingleton already exists. Did you query the singleton in a constructor?");
        }
        else
        {
            s_instance = this;
        }
    }

    private static void CreateAndLoad()
    {
        var existingAsset = AssetDatabase.LoadAssetAtPath(kFilePath, typeof(CircuitDesignerPreferences)) as CircuitDesignerPreferences;
        if (existingAsset)
            s_instance = existingAsset;
        else
        {
            s_instance = CreateInstance<CircuitDesignerPreferences>();
            AssetDatabase.CreateAsset(s_instance, kFilePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}