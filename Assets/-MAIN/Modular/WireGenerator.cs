using System.Collections.Generic;
using UnityEngine;

public class WireGenerator : MonoBehaviour
{
    [SerializeField] private List<Transform> wirePoints = new List<Transform>();
    [SerializeField] private Material wireMaterial;
    [SerializeField] private int cylinderResolution = 12;
    [SerializeField] private float wireThickness = 0.05f;
    [SerializeField] private bool addEndCaps = true;
    [SerializeField] private bool generateWireOnStart = true;

    private List<GameObject> wireSegments = new List<GameObject>();

    public void Start()
    {
        if (generateWireOnStart) { GenerateWires(); }
    }

    public void GenerateWires()
    {
        // Cleanup old wires
        foreach (var segment in wireSegments)
        {
            if (segment != null) Destroy(segment);
        }
        wireSegments.Clear();

        // Loop through pairs
        for (int i = 0; i < wirePoints.Count - 1; i++)
        {
            var start = wirePoints[i];
            var end = wirePoints[i + 1];
            if (start == null || end == null) continue;

            GameObject cylinder = GenerateCylinderBetweenPoints(start.position, end.position);
            wireSegments.Add(cylinder);
        }
    }

    private GameObject GenerateCylinderBetweenPoints(Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;
        float length = direction.magnitude;
        Vector3 midPoint = (start + end) / 2f;

        // Create GameObject holder
        GameObject go = new GameObject("WireSegment");
        go.transform.SetParent(transform);
        go.transform.position = midPoint;
        go.transform.up = direction.normalized;

        // Add mesh components
        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = wireMaterial;

        // Generate mesh
        mf.mesh = CreateCylinderMesh(length / 2f, wireThickness, cylinderResolution);

        return go;
    }

    private Mesh CreateCylinderMesh(float halfHeight, float radius, int resolution)
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        float angleStep = 360f / resolution;

        // Keep track of index offsets
        int topCenterIndex, bottomCenterIndex;

        // Side vertices
        for (int i = 0; i <= resolution; i++)
        {
            float angle = Mathf.Deg2Rad * i * angleStep;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            Vector3 bottom = new Vector3(x, -halfHeight, z);
            Vector3 top = new Vector3(x, halfHeight, z);

            vertices.Add(bottom); // i * 2
            vertices.Add(top);    // i * 2 + 1

            uvs.Add(new Vector2(i / (float)resolution, 0));
            uvs.Add(new Vector2(i / (float)resolution, 1));
        }

        // Side triangles
        for (int i = 0; i < resolution * 2; i += 2)
        {
            triangles.Add(i);
            triangles.Add(i + 1);
            triangles.Add(i + 2);

            triangles.Add(i + 1);
            triangles.Add(i + 3);
            triangles.Add(i + 2);
        }

        // Center points for caps
        topCenterIndex = vertices.Count;
        vertices.Add(new Vector3(0, halfHeight, 0)); // Top center
        uvs.Add(new Vector2(0.5f, 0.5f));

        bottomCenterIndex = vertices.Count;
        vertices.Add(new Vector3(0, -halfHeight, 0)); // Bottom center
        uvs.Add(new Vector2(0.5f, 0.5f));

        // Top cap
        for (int i = 0; i < resolution; i++)
        {
            int current = i * 2 + 1;
            int next = ((i + 1) % resolution) * 2 + 1;

            triangles.Add(topCenterIndex);
            triangles.Add(next);
            triangles.Add(current);
        }

        // Bottom cap
        for (int i = 0; i < resolution; i++)
        {
            int current = i * 2;
            int next = ((i + 1) % resolution) * 2;

            triangles.Add(bottomCenterIndex);
            triangles.Add(current);
            triangles.Add(next);
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();

        return mesh;
    }

}