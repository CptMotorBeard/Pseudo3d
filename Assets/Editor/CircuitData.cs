using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class CircuitData : ScriptableObject
{
    [SerializeField] public List<CircuitNode> Circuit;

    private void Awake()
    {
        hideFlags = HideFlags.NotEditable;
    }

    public CircuitData()
    {
        Circuit = new List<CircuitNode>();
    }

    public static CircuitData CreateOrLoad(string filePath)
    {
        CircuitData returnData = null;
        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            returnData = AssetDatabase.LoadAssetAtPath<CircuitData>(filePath);
        }

        if (returnData == null)
        {            
            returnData = CreateInstance<CircuitData>();
            AssetDatabase.CreateAsset(returnData, filePath);
        }

        return returnData;
    }
}