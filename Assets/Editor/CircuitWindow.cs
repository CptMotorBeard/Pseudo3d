using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class CircuitWindow : EditorWindow
{
    private const string kDefaultTitle = "Circuit";
    private const string kScriptableObjectPath = "Assets/ScriptableObjects";

    [MenuItem("Window/Circuit Designer")]
    static void Init()
    {
        var window = CreateInstance<CircuitWindow>();
        window.titleContent = new GUIContent(kDefaultTitle);
        window.Show();
    }

    public const float kToolbarHeight = 20f;

    [SerializeField]
    private RoadSectionGroup m_roadSectionGroup;
    public RoadSectionGroup Circuit => m_roadSectionGroup;

    public CircuitViewer Viewer { get; private set; }
    public CircuitEditor Editor { get; set; }

    private CanvasTransform Canvas
    {
        get
        {
            return new CanvasTransform
            {
                Pan = Viewer.PanOffset,
                Zoom = Viewer.ZoomScale,
                Size = position.size
            };
        }
    }

    public Rect CanvasInputRect
    {
        get
        {
            var rect = new Rect(Vector2.zero, position.size);
            rect.y += kToolbarHeight;
            rect.height -= kToolbarHeight;
            return rect;
        }
    }

    private void OnEnable()
    {
        Viewer = new CircuitViewer();
        Editor = new CircuitEditor();
        Editor.Viewer = Viewer;

        Editor.CanvasChanged += (s, e) => Repaint();
        Editor.Input.MouseDown += (s, e) => Repaint();
        Editor.Input.MouseUp += (s, e) => Repaint();
        Editor.Input.SaveRequest += (s, e) => Save();

        BuildCanvas();
    }

    private void OnDestroy()
    {
        Save();
    }

    public override void SaveChanges()
    {
        Save();
    }

    private void BuildCanvas()
    {
        if (Circuit)
        {
            Editor.SetCircuit(Circuit);
            Repaint();
        }
    }

    public static CircuitWindow OpenCircuit(RoadSectionGroup circuit)
    {
        if (!circuit)
            return null;

        var windows = Resources.FindObjectsOfTypeAll<CircuitWindow>();
        bool isAlreadyOpen = windows.Any(w => w.Circuit == circuit);

        if (isAlreadyOpen)
            return null;

        CircuitWindow window = windows.FirstOrDefault(w => w.Circuit == null);
        if (!window)
        {
            window = CreateInstance<CircuitWindow>();
            window.Show();
        }

        window.SetCircuit(circuit);

        return window;
    }

    public void SetCircuit(RoadSectionGroup circuit)
    {
        m_roadSectionGroup = circuit;
    }

    public void Save()
    {
        if (Circuit)
        {
            EditorUtility.SetDirty(Circuit);
        }
        if (Editor?.Canvas?.CircuitData)
        {
            EditorUtility.SetDirty(Editor.Canvas.CircuitData);
        }
    }

    private void OnGUI()
    {
        if (Circuit == null)
        {
            Viewer.DrawStaticGrid(position.size);
            Viewer.DrawStatus();
        }
        else
        {
            if (Editor.Canvas == null)
            {
                BuildCanvas();
            }

            CanvasTransform canvas = Canvas;
            Editor.PollInput(Event.current, canvas, CanvasInputRect);
            Viewer.Draw(canvas);
        }

        DrawToolbar();
        UpdateWindowTitle();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal("Toolbar");
        if (GUILayout.Button("File", EditorStyles.toolbarDropDown, GUILayout.Width(50f)))
        {
            var fileMenu = new GenericMenu();

            fileMenu.AddItem(new GUIContent("Create New"), false, CreateNew);
            fileMenu.AddSeparator("");
            fileMenu.AddItem(new GUIContent("Load"), false, Load);
            fileMenu.AddItem(new GUIContent("Save"), false, Save);
            fileMenu.DropDown(new Rect(5f, kToolbarHeight, 0f, 0f));
        }

        if (GUILayout.Button("Reset View"))
        {
            Viewer.PanOffset = Vector2.one;
            Viewer.Zoom = Vector2.one;
        }

        GUILayout.FlexibleSpace();
        GUILayout.Label(CircuitName());
        EditorGUILayout.EndHorizontal();
    }

    private void CreateNew()
    {
        string path = EditorUtility.SaveFilePanelInProject("New RoadSegmentGroup", "RoadSegmentGroup", "asset", "Select a destination to create a new asset", kScriptableObjectPath);
        if (!string.IsNullOrEmpty(path))
        {
            RoadSectionGroup circuit = CreateInstance<RoadSectionGroup>();
            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }
            AssetDatabase.CreateAsset(circuit, path);
            OpenCircuit(circuit);
        }
    }

    private void Load()
    {
        string openFile = EditorUtility.OpenFilePanel("Load RoadSegmentGroup", kScriptableObjectPath, "asset");
        if (!string.IsNullOrEmpty(openFile))
        {
            if (openFile.StartsWith(Application.dataPath))
            {
                openFile = "Assets" + openFile.Substring(Application.dataPath.Length);
            }
            var circuit = AssetDatabase.LoadAssetAtPath<RoadSectionGroup>(openFile);
            if (circuit)
            {
                OpenCircuit(circuit);
            }
            else
            {
                Debug.LogError($"Invalid file: {openFile}");
            }
        }        
    }

    private void UpdateWindowTitle()
    {
        if (Circuit != null && Circuit.name.Length != 0)
        {
            if (titleContent.text != Circuit.name)
                titleContent.text = Circuit.name;
        }
        else
        {
            if (titleContent.text != kDefaultTitle)
                titleContent.text = kDefaultTitle;
        }
    }

    private string CircuitName()
    {
        return Circuit ? (Circuit.name.Length == 0 ? "New Circuit" : Circuit.name) : "None";
    }

    [OnOpenAsset(0)]
    static bool OpenCircuitAsset(int instanceID, int line)
    {
        var circuit = EditorUtility.InstanceIDToObject(instanceID) as RoadSectionGroup;
        if (circuit)
        {
            OpenCircuit(circuit);
        }

        return circuit;
    }
}
