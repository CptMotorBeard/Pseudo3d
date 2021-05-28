using System;
using System.Collections.Generic;
using UnityEngine;

public class Circuit : MonoBehaviour
{
    [SerializeField] private Camera m_renderCamera;
    [SerializeField] private SpriteRenderer m_roadRenderer;
    [SerializeField] private GameConfig m_gameConfig;
    [SerializeField] private RoadSectionGroup m_circuit;

    private List<SegmentData> m_segments;
    private RenderTexture m_renderTexture;

    private Vector4 m_playerCameraPosition;
    private Vehicle m_playerVehicle;

    private Mesh m_road1Mesh;
    private Mesh m_road2Mesh;
    private Mesh m_grass1Mesh;
    private Mesh m_grass2Mesh;

    private Mesh[] m_meshes;

    private void Start()
    {
        m_segments = m_circuit.GetSections().Build(m_gameConfig);
        m_gameConfig.InitializeGameConfig(m_segments.Count, m_renderCamera);

        Texture2D texture2D = new Texture2D(m_gameConfig.ScreenWidth, m_gameConfig.ScreenHeight, TextureFormat.RGBA32, false);
        texture2D.filterMode = FilterMode.Point;

        m_roadRenderer.sprite = Sprite.Create(texture2D, new Rect(0, 0, m_gameConfig.ScreenWidth, m_gameConfig.ScreenHeight), new Vector2(0.5f, 0.5f));
        m_roadRenderer.sprite.name = "runtimeRenderer";

        m_road1Mesh = new Mesh();
        m_road2Mesh = new Mesh();
        m_grass1Mesh = new Mesh();
        m_grass2Mesh = new Mesh();

        m_meshes = new Mesh[m_gameConfig.DrawnSegments * 2];
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        Vector2 input = new Vector2();
        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");

        m_playerCameraPosition.z += dt * m_playerVehicle.CurrentSpeed;

        while (m_playerCameraPosition.z > m_gameConfig.TrackLength)
            m_playerCameraPosition.z -= m_gameConfig.TrackLength;
        while (m_playerCameraPosition.z < 0)
            m_playerCameraPosition.z += m_gameConfig.TrackLength;

        SegmentData playerSegment = GetSegment(m_playerCameraPosition.z + m_playerVehicle.Position.z);
        float speedPercent = m_playerVehicle.CurrentSpeed / m_gameConfig.MaxSpeed;
        float dx = dt * 2 * speedPercent;

        if (input.y > 0)
            m_playerVehicle.CurrentSpeed += m_gameConfig.AccelerationForce * m_gameConfig.MaxSpeed * dt;
        else if (input.y < 0)
            m_playerVehicle.CurrentSpeed -= m_gameConfig.BrakingForce * m_gameConfig.MaxSpeed * dt;
        else
            m_playerVehicle.CurrentSpeed -= m_gameConfig.DecelerationForce * m_gameConfig.MaxSpeed * dt;

        m_playerVehicle.Position.x += input.x * dx;
        if ((m_playerVehicle.Position.x < -1 || m_playerVehicle.Position.x > 1) &&
            (m_playerVehicle.CurrentSpeed > m_gameConfig.MaxSpeedOffRoad))
        {
            m_playerVehicle.CurrentSpeed -= m_gameConfig.OffRoadDecelerationForce * m_gameConfig.MaxSpeed * dt;
        }

        m_playerVehicle.Position.x -= dx * speedPercent * playerSegment.Curve * m_gameConfig.CentrifugalForce;
        m_playerCameraPosition.y = m_gameConfig.BaseCameraYPosition + playerSegment.WorldPosition.y;

        m_playerVehicle.Position.x = Mathf.Clamp(m_playerVehicle.Position.x, -2, 2);
        m_playerVehicle.CurrentSpeed = Mathf.Clamp(m_playerVehicle.CurrentSpeed, 0, m_gameConfig.MaxSpeed);

        Render();
    }

    private void Render()
    {
        m_renderTexture = RenderTexture.GetTemporary(m_gameConfig.ScreenWidth, m_gameConfig.ScreenHeight);
        RenderTexture activeTarget = RenderTexture.active;
        Graphics.SetRenderTarget(m_renderTexture);
        GL.Clear(false, true, Color.clear);
        GL.PushMatrix();

        try
        {
            bool isLooping = false;
            int baseIndex = GetSegment(m_playerCameraPosition.z).Index;
            float basePercent = (m_playerCameraPosition.z % m_gameConfig.SegmentLength) / m_gameConfig.SegmentLength;
            float dx = -(m_segments[baseIndex].Curve * basePercent);
            float x = 0f;

            m_road1Mesh.Clear();
            m_road2Mesh.Clear();
            m_grass1Mesh.Clear();
            m_grass2Mesh.Clear();

            List<CombineInstance> grass1 = new List<CombineInstance>();
            List<CombineInstance> grass2 = new List<CombineInstance>();
            List<CombineInstance> road1 = new List<CombineInstance>();
            List<CombineInstance> road2 = new List<CombineInstance>();

            for (int i = 0; i < m_gameConfig.DrawnSegments; ++i)
            {
                int curIndex = (baseIndex + i) % m_gameConfig.SegmentCount;
                int prevIndex = (curIndex > 0) ? curIndex - 1 : m_gameConfig.SegmentCount - 1;

                SegmentData curSegment = m_segments[curIndex];
                SegmentData prevSegment = m_segments[prevIndex];

                if (isLooping)
                {
                    curSegment.WorldPosition.z += m_gameConfig.TrackLength;
                    prevSegment.WorldPosition.z += m_gameConfig.TrackLength;
                }

                if (curIndex < prevIndex && baseIndex != 0)
                {
                    isLooping = true;
                    curSegment.WorldPosition.z += m_gameConfig.TrackLength;
                }

                Project3D(ref prevSegment, x, 0);
                Project3D(ref curSegment, x, dx);

                if (prevSegment.CameraPosition.z == 0 || curSegment.CameraPosition.z == 0
                    || prevSegment.ScreenPosition.y > 0.5f || curSegment.ScreenPosition.y > 0.5f
                    || prevSegment.CameraPosition.z <= m_gameConfig.DistanceToPlane)
                {
                    continue;
                }

                x += dx;
                dx += curSegment.Curve;

                MeshHelper.SetMesh(ref m_meshes[i * 2],
                    0,
                    prevSegment.ScreenPosition.y,
                    m_gameConfig.ScreenWidth / 2,
                    0,
                    curSegment.ScreenPosition.y,
                    m_gameConfig.ScreenWidth / 2);

                MeshHelper.SetMesh(ref m_meshes[i * 2 + 1],
                    prevSegment.ScreenPosition.x - (m_gameConfig.ScreenWidth / 2),
                    prevSegment.ScreenPosition.y,
                    prevSegment.ScreenPosition.z,
                    curSegment.ScreenPosition.x - (m_gameConfig.ScreenWidth / 2),
                    curSegment.ScreenPosition.y,
                    curSegment.ScreenPosition.z);

                if (i % 2 == 1)
                {
                    grass1.Add(new CombineInstance { mesh = m_meshes[i * 2] });
                    road1.Add(new CombineInstance { mesh = m_meshes[i * 2 + 1] });
                }
                else
                {
                    grass2.Add(new CombineInstance { mesh = m_meshes[i * 2] });
                    road2.Add(new CombineInstance { mesh = m_meshes[i * 2 + 1] });
                }
            }

            m_grass1Mesh.CombineMeshes(grass1.ToArray(), true, false, false);
            m_grass2Mesh.CombineMeshes(grass2.ToArray(), true, false, false);
            m_road1Mesh.CombineMeshes(road1.ToArray(), true, false, false);
            m_road2Mesh.CombineMeshes(road2.ToArray(), true, false, false);

            Graphics.DrawMesh(m_grass1Mesh, m_gameConfig.ScaleMatrix, m_gameConfig.Grass1, 0, null, 0);
            Graphics.DrawMesh(m_grass2Mesh, m_gameConfig.ScaleMatrix, m_gameConfig.Grass2, 0, null, 0);
            Graphics.DrawMesh(m_road1Mesh, m_gameConfig.ScaleMatrix, m_gameConfig.Road1, 0, null, 0);
            Graphics.DrawMesh(m_road2Mesh, m_gameConfig.ScaleMatrix, m_gameConfig.Road2, 0, null, 0);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        Graphics.CopyTexture(m_renderTexture, m_roadRenderer.sprite.texture);
        GL.PopMatrix();
        Graphics.SetRenderTarget(activeTarget);
        RenderTexture.ReleaseTemporary(m_renderTexture);
    }

    private SegmentData GetSegment(float positionZ)
    {
        if (positionZ < 0)
        {
            positionZ += m_gameConfig.TrackLength;
        }
        var index = Mathf.FloorToInt(positionZ / m_gameConfig.SegmentLength) % m_gameConfig.SegmentCount;
        return m_segments[index];
    }

    private void Project3D(ref SegmentData point, float x, float dx)
    {
        point.CameraPosition.x = point.WorldPosition.x - m_playerVehicle.Position.x * m_gameConfig.RoadWidth - x - dx;
        point.CameraPosition.y = point.WorldPosition.y - m_playerCameraPosition.y;
        point.CameraPosition.z = point.WorldPosition.z - m_playerCameraPosition.z;

        if (point.CameraPosition.z == 0)
            return;

        point.Scale = m_gameConfig.DistanceToPlane / point.CameraPosition.z;

        point.ScreenPosition.x = Mathf.Round((1 + point.Scale * point.CameraPosition.x) * m_gameConfig.ScreenWidth / 2);
        point.ScreenPosition.y = Mathf.Round(point.Scale * point.CameraPosition.y * m_gameConfig.ScreenHeight / 2);
        point.ScreenPosition.z = Mathf.Round(point.Scale * m_gameConfig.RoadWidth * m_gameConfig.ScreenWidth / 2);
    }
}