using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/GameConfig")]
public class GameConfig : ScriptableObject
{
    /*       Config Values      */
    /*     ================     */
    [Range(24, 240)]
    public int TargetFPS = 60;

    [Header("Materials")]
    public Material Road1;
    public Material Road2;
    public Material Grass1;
    public Material Grass2;

    [Header("Road Information")]
    public int SegmentLength = 100;
    public int RoadWidth = 1000;
    public int DrawnSegments = 200;

    [Header("Camera Information")]
    public float BaseCameraYPosition = 1000;
    public float CameraDistanceToCar = 200;

    [Header("Driving Forces")]
    [Range(0, 1)]
    public float CentrifugalForce = 0.3f;
    [Tooltip("Percent of MaxSpeed added to CurrentSpeed when accelerating")]
    [Range(0, 1)]
    public float AccelerationForce = 0.2f;
    [Tooltip("Percent of MaxSpeed removed from CurrentSpeed when idle")]
    [Range(0, 1)]
    public float DecelerationForce = 0.2f;
    [Tooltip("Percent of MaxSpeed removed from CurrentSpeed when offroad and above MaxSpeedOffRoad")]
    [Range(0, 1)]
    public float OffRoadDecelerationForce = 0.5f;
    [Range(0, 1)]
    public float OffRoadMaximumSpeedModifier = 0.25f;
    [Tooltip("Percent of MaxSpeed removed from CurrentSpeed when braking")]
    [Range(0, 1)]
    public float BrakingForce = 1.0f;

    /*      Derived Values      */
    /*     ================     */

    /*       Camera Values      */
    public float DistanceToPlane { get; private set; }

    /*      Vehicle Values      */
    public float FrameStep { get; private set; }
    public float MaxSpeed { get; private set; }
    public float MaxSpeedOffRoad { get; private set; }

    /*       Game Values        */
    public int TrackLength { get; private set; }
    public int SegmentCount { get; private set; }
    public int ScreenHeight { get; private set; }
    public int ScreenWidth { get; private set; }

    public Matrix4x4 ScaleMatrix { get; private set; }

    public void InitializeGameConfig(int numberOfTrackSegments, Camera renderCamera)
    {
        Application.targetFrameRate = TargetFPS;

        DistanceToPlane = 1 / (BaseCameraYPosition / CameraDistanceToCar);

        FrameStep = 1f / TargetFPS;
        MaxSpeed = SegmentLength / FrameStep;
        MaxSpeedOffRoad = MaxSpeed * OffRoadMaximumSpeedModifier;

        ScreenHeight = Screen.height;
        ScreenWidth = Screen.width;

        TrackLength = numberOfTrackSegments * SegmentLength;
        SegmentCount = numberOfTrackSegments;

        float refHeight = renderCamera.orthographicSize * 2;
        float refHScale = refHeight / ScreenHeight;
        float heightScale = ((float)ScreenHeight) / renderCamera.pixelHeight;
        float unscaledAspectRatio = (heightScale * renderCamera.pixelWidth) / ScreenWidth;

        ScaleMatrix = Matrix4x4.Scale(new Vector3(unscaledAspectRatio * refHScale, refHScale, 1));
    }
}