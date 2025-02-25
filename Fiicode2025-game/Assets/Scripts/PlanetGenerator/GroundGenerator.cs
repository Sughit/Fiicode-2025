using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GroundGenerator : MonoBehaviour
{
    [Header("Planet Settings")]
    public float radius = 1f;                   // Base radius of the planet
    [Range(0, 3)]
    public int subdivisions = 3;                // More subdivisions = finer mesh

    [Header("Base Noise Settings")]
    public float noiseScale = 1f;               // Base scale for noise
    public float noiseStrength = 0.7f;          // Base displacement strength
    [Tooltip("Offset added to noise sampling to break symmetry.")]
    public Vector2 noiseOffset = new Vector2(0.75f, 0.25f);

    [Header("Fractal Noise Settings")]
    public int noiseOctaves = 4;                // Number of noise layers
    public float noiseLacunarity = 2f;          // Frequency multiplier per octave
    public float noisePersistence = 0.5f;       // Amplitude multiplier per octave

    [Header("Relief Settings")]
    [Tooltip("Exponent to shape the noise for relief (values >1 accentuate peaks).")]
    public float reliefExponent = 4f;           // Exponent to exaggerate noise
    [Tooltip("Multiplier to further scale the noise effect on the terrain.")]
    public float reliefMultiplier = 2f;         // Further scales the noise impact

    [Header("Color Settings")]
    public Gradient elevationGradient;          // Gradient for elevation-based coloring

    private Mesh mesh;

    void Start()
    {
        mesh = new Mesh();
        mesh.name = "LowPolyPlanet";
        GetComponent<MeshFilter>().mesh = mesh;
        CreatePlanet();
    }

    void CreatePlanet()
    {
        // Build initial lists for vertices and triangles (icosahedron)
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // Golden ratio factor for icosahedron vertices
        float t = (1f + Mathf.Sqrt(5f)) / 2f;

        // Create the 12 vertices of the icosahedron and scale them to the base radius
        vertices.Add(new Vector3(-1, t, 0).normalized * radius);
        vertices.Add(new Vector3(1, t, 0).normalized * radius);
        vertices.Add(new Vector3(-1, -t, 0).normalized * radius);
        vertices.Add(new Vector3(1, -t, 0).normalized * radius);

        vertices.Add(new Vector3(0, -1, t).normalized * radius);
        vertices.Add(new Vector3(0, 1, t).normalized * radius);
        vertices.Add(new Vector3(0, -1, -t).normalized * radius);
        vertices.Add(new Vector3(0, 1, -t).normalized * radius);

        vertices.Add(new Vector3(t, 0, -1).normalized * radius);
        vertices.Add(new Vector3(t, 0, 1).normalized * radius);
        vertices.Add(new Vector3(-t, 0, -1).normalized * radius);
        vertices.Add(new Vector3(-t, 0, 1).normalized * radius);

        // Define the 20 triangular faces of the icosahedron
        int[] faces = new int[]
        {
            0, 11, 5,
            0, 5, 1,
            0, 1, 7,
            0, 7, 10,
            0, 10, 11,

            1, 5, 9,
            5, 11, 4,
            11, 10, 2,
            10, 7, 6,
            7, 1, 8,

            3, 9, 4,
            3, 4, 2,
            3, 2, 6,
            3, 6, 8,
            3, 8, 9,

            4, 9, 5,
            2, 4, 11,
            6, 2, 10,
            8, 6, 7,
            9, 8, 1
        };

        triangles.AddRange(faces);

        // Subdivide the mesh to increase detail (more triangles)
        for (int i = 0; i < subdivisions; i++)
        {
            Subdivide(vertices, triangles);
        }

        // Apply fractal noise to each vertex to simulate dramatic, natural relief
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 v = vertices[i].normalized;
            // Compute fractal noise over multiple octaves
            float fractalNoise = ComputeFractalNoise(v);
            // Exaggerate the noise using the relief exponent
            fractalNoise = Mathf.Pow(fractalNoise, reliefExponent);
            float elevation = fractalNoise * noiseStrength * reliefMultiplier;
            vertices[i] = v * (radius + elevation);
        }

        // Build the mesh
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        // Dynamic color assignment based on elevation
        float[] elevations = new float[vertices.Count];
        float minElev = float.MaxValue;
        float maxElev = float.MinValue;
        for (int i = 0; i < vertices.Count; i++)
        {
            float elev = vertices[i].magnitude - radius;
            elevations[i] = elev;
            if (elev < minElev) minElev = elev;
            if (elev > maxElev) maxElev = elev;
        }

        Color[] colors = new Color[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            float normalizedElev = (elevations[i] - minElev) / (maxElev - minElev);
            colors[i] = elevationGradient.Evaluate(normalizedElev);
        }
        mesh.colors = colors;
    }

    // Computes fractal noise by summing several octaves of Perlin noise
    float ComputeFractalNoise(Vector3 v)
    {
        float total = 0f;
        float amplitude = 1f;
        float frequency = noiseScale;
        float maxValue = 0f; // for normalization

        for (int i = 0; i < noiseOctaves; i++)
        {
            total += Mathf.PerlinNoise((v.x + noiseOffset.x) * frequency, (v.y + noiseOffset.y) * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= noisePersistence;
            frequency *= noiseLacunarity;
        }
        return total / maxValue;
    }

    // Subdivides each triangle into four new triangles
    void Subdivide(List<Vector3> vertices, List<int> triangles)
    {
        Dictionary<long, int> midPointCache = new Dictionary<long, int>();
        List<int> newTriangles = new List<int>();

        int GetMidPoint(int i1, int i2)
        {
            long key = i1 < i2 ? ((long)i1 << 32) + i2 : ((long)i2 << 32) + i1;
            if (midPointCache.TryGetValue(key, out int ret))
                return ret;
            Vector3 mid = (vertices[i1] + vertices[i2]).normalized * radius;
            vertices.Add(mid);
            int index = vertices.Count - 1;
            midPointCache.Add(key, index);
            return index;
        }

        for (int i = 0; i < triangles.Count; i += 3)
        {
            int v1 = triangles[i];
            int v2 = triangles[i + 1];
            int v3 = triangles[i + 2];

            int a = GetMidPoint(v1, v2);
            int b = GetMidPoint(v2, v3);
            int c = GetMidPoint(v3, v1);

            newTriangles.Add(v1);
            newTriangles.Add(a);
            newTriangles.Add(c);

            newTriangles.Add(v2);
            newTriangles.Add(b);
            newTriangles.Add(a);

            newTriangles.Add(v3);
            newTriangles.Add(c);
            newTriangles.Add(b);

            newTriangles.Add(a);
            newTriangles.Add(b);
            newTriangles.Add(c);
        }

        triangles.Clear();
        triangles.AddRange(newTriangles);
    }
}
