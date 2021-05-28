using UnityEngine;

public static class MeshHelper
{
    private static readonly Vector3[] s_normals = new Vector3[]
    {
        -Vector3.forward,
        -Vector3.forward,
        -Vector3.forward,
        -Vector3.forward
    };

    private static readonly Vector2[] s_uvs = new Vector2[]
    {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(0, 1),
        new Vector2(1, 1)
    };

    private static readonly int[] s_triangles = new int[]
    {
        0, 2, 1,
        2, 3, 1
    };

    public static void SetMesh(ref Mesh mesh, float x1, float y1, float width1, float x2, float y2, float width2)
    {
        if (!mesh)
            mesh = new Mesh();

        mesh.Clear();

        var vertices = new Vector3[4];
        vertices[0] = new Vector3(x1 - width1, y1, 0);
        vertices[1] = new Vector3(x1 + width1, y1, 0);
        vertices[2] = new Vector3(x2 - width2, y2, 0);
        vertices[3] = new Vector3(x2 + width2, y2, 0);

        mesh.SetVertices(vertices);
        mesh.SetNormals(s_normals);
        mesh.SetUVs(0, s_uvs);
        mesh.SetTriangles(s_triangles, 0);
    }
}