using UnityEngine;
using System.Collections.Generic;

public class ProceduralBuildingGenerator : MonoBehaviour
{
    // 6 footprints: 2 rectangular, 4 concave
    private List<int[,]> footprints;

    // Public texture variables for wall and roof textures
    public Texture2D wallTexture1;
    public Texture2D wallTexture2;
    public Texture2D roofTexture1;
    public Texture2D roofTexture2;

    // Door and window prefabs
    public GameObject[] doorPrefabs;   
    public GameObject[] windowPrefabs; 

    // Public field for random seed
    public int randomSeed;

    private void Start()
    {
        // Seed the random number generator
        Random.InitState(randomSeed);

        InitializeFootprints();

        // Generate three buildings randomly
        for (int i = 0; i < 3; i++)
        {
            // Randomly select a footprint
            int footprintIndex = Random.Range(0, footprints.Count);
            int[,] selectedFootprint = footprints[footprintIndex];

            // Randomly determine the position for the building
            Vector3 position = new Vector3(i * 4, 0, 0); // Adjust spacing as needed

            GenerateBuilding(position, selectedFootprint);
        }
    }

    void InitializeFootprints()
    {
        footprints = new List<int[,]>();

        // Rectangular footprints (simple)
        footprints.Add(new int[,] {
            { 1, 1, 1 },
            { 1, 1, 1 },
        });

        footprints.Add(new int[,] {
            { 1, 1, 1 },
            { 1, 1, 1 },
            { 1, 1, 1 },
        });

        // L-shaped footprint
        footprints.Add(new int[,] {
            { 1, 0, 0 },
            { 1, 1, 1 },
        });

        // U-shaped footprint
        footprints.Add(new int[,] {
            { 1, 1, 1 },
            { 1, 0, 1 },
        });

        // weirdo-shaped footprint
        footprints.Add(new int[,] {
            { 1, 1, 1 },
            { 1, 1, 0 },
        });

        // Another concave shape
        footprints.Add(new int[,] {
            { 1, 1, 1 },
            { 1, 1, 1 },
            { 1, 0, 0 }
        });
    }

    void GenerateBuilding(Vector3 position, int[,] footprint)
    {
        // Create a parent GameObject for the building
        GameObject building = new GameObject("Building");
        building.transform.position = position;

        MeshFilter meshFilter = building.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = building.AddComponent<MeshRenderer>();

        // Randomly select wall and roof textures
        Texture2D selectedWallTexture = (Random.value < 0.5f) ? wallTexture1 : wallTexture2;
        Texture2D selectedRoofTexture = (Random.value < 0.5f) ? roofTexture1 : roofTexture2;

        // Create materials
        Material wallMaterial = new Material(Shader.Find("Standard"));
        wallMaterial.mainTexture = selectedWallTexture;

        Material roofMaterial = new Material(Shader.Find("Standard"));
        roofMaterial.mainTexture = selectedRoofTexture;

        // Assign materials to the mesh renderer
        meshRenderer.materials = new Material[] { wallMaterial, roofMaterial };

        // Create building mesh based on grid
        Mesh buildingMesh = CreateBuildingMesh(footprint, out Dictionary<Vector3, float> cubeHeights);
        meshFilter.mesh = buildingMesh;

        // Add doors and windows
        AddDoorsAndWindows(building.transform, footprint, position, cubeHeights);
    }

    Mesh CreateBuildingMesh(int[,] footprint, out Dictionary<Vector3, float> cubeHeights)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        List<int> roofTriangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        cubeHeights = new Dictionary<Vector3, float>();

        int depth = footprint.GetLength(0); // Z-axis (rows)
        int width = footprint.GetLength(1); // X-axis (columns)

        // Create vertices and triangles based on grid
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                if (footprint[z, x] == 1)
                {
                    float randomHeight = Random.Range(1, 4);
                    Vector3 cubePosition = new Vector3(x, 0, z);

                    AddCube(vertices, wallTriangles, uvs, cubePosition, randomHeight);
                    AddHipRoof(vertices, roofTriangles, uvs, cubePosition, randomHeight);

                    cubeHeights[cubePosition] = randomHeight;
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.subMeshCount = 2; // One for walls, one for roofs
        mesh.SetTriangles(wallTriangles.ToArray(), 0); // Walls
        mesh.SetTriangles(roofTriangles.ToArray(), 1); // Roofs
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals(); // Ensure normals are correct

        return mesh;
    }

    void AddCube(List<Vector3> vertices, List<int> wallTriangles, List<Vector2> uvs, Vector3 basePosition, float height)
    {
        int startIndex;

        // Front face (Z+)
        startIndex = vertices.Count;
        vertices.Add(basePosition + new Vector3(0, 0, 1)); // 0
        vertices.Add(basePosition + new Vector3(1, 0, 1)); // 1
        vertices.Add(basePosition + new Vector3(1, height, 1)); // 2
        vertices.Add(basePosition + new Vector3(0, height, 1)); // 3

        
        wallTriangles.Add(startIndex + 0);
        wallTriangles.Add(startIndex + 1);
        wallTriangles.Add(startIndex + 2);
        wallTriangles.Add(startIndex + 0);
        wallTriangles.Add(startIndex + 2);
        wallTriangles.Add(startIndex + 3);

        // UVs for front face
        uvs.Add(new Vector2(0, 0));          // Bottom-left
        uvs.Add(new Vector2(1, 0));          // Bottom-right
        uvs.Add(new Vector2(1, height));     // Top-right
        uvs.Add(new Vector2(0, height));     // Top-left

        // Right face (X+)
        startIndex = vertices.Count;
        vertices.Add(basePosition + new Vector3(1, 0, 1)); // 0
        vertices.Add(basePosition + new Vector3(1, 0, 0)); // 1
        vertices.Add(basePosition + new Vector3(1, height, 0)); // 2
        vertices.Add(basePosition + new Vector3(1, height, 1)); // 3

        
        wallTriangles.Add(startIndex + 0);
        wallTriangles.Add(startIndex + 1);
        wallTriangles.Add(startIndex + 2);
        wallTriangles.Add(startIndex + 0);
        wallTriangles.Add(startIndex + 2);
        wallTriangles.Add(startIndex + 3);

        // UVs for right face
        uvs.Add(new Vector2(0, 0));          // Bottom-left
        uvs.Add(new Vector2(1, 0));          // Bottom-right
        uvs.Add(new Vector2(1, height));     // Top-right
        uvs.Add(new Vector2(0, height));     // Top-left

        // Back face (Z-)
        startIndex = vertices.Count;
        vertices.Add(basePosition + new Vector3(1, 0, 0)); // 0
        vertices.Add(basePosition + new Vector3(0, 0, 0)); // 1
        vertices.Add(basePosition + new Vector3(0, height, 0)); // 2
        vertices.Add(basePosition + new Vector3(1, height, 0)); // 3

        
        wallTriangles.Add(startIndex + 0);
        wallTriangles.Add(startIndex + 1);
        wallTriangles.Add(startIndex + 2);
        wallTriangles.Add(startIndex + 0);
        wallTriangles.Add(startIndex + 2);
        wallTriangles.Add(startIndex + 3);

        // UVs for back face
        uvs.Add(new Vector2(0, 0));          // Bottom-left
        uvs.Add(new Vector2(1, 0));          // Bottom-right
        uvs.Add(new Vector2(1, height));     // Top-right
        uvs.Add(new Vector2(0, height));     // Top-left

        // Left face (X-)
        startIndex = vertices.Count;
        vertices.Add(basePosition + new Vector3(0, 0, 0)); // 0
        vertices.Add(basePosition + new Vector3(0, 0, 1)); // 1
        vertices.Add(basePosition + new Vector3(0, height, 1)); // 2
        vertices.Add(basePosition + new Vector3(0, height, 0)); // 3

        
        wallTriangles.Add(startIndex + 0);
        wallTriangles.Add(startIndex + 1);
        wallTriangles.Add(startIndex + 2);
        wallTriangles.Add(startIndex + 0);
        wallTriangles.Add(startIndex + 2);
        wallTriangles.Add(startIndex + 3);

        // UVs for left face
        uvs.Add(new Vector2(0, 0));          // Bottom-left
        uvs.Add(new Vector2(1, 0));          // Bottom-right
        uvs.Add(new Vector2(1, height));     // Top-right
        uvs.Add(new Vector2(0, height));     // Top-left
    }



    void AddHipRoof(List<Vector3> vertices, List<int> roofTriangles, List<Vector2> uvs, Vector3 basePosition, float height)
    {
        // Calculate the roof apex position
        Vector3 apex = basePosition + new Vector3(0.5f, height + 0.5f, 0.5f); // Centered on top of the cube

        // Add the roof base vertices (4 corners) and apex
        int startIndex = vertices.Count;

        // Base vertices
        Vector3 v1 = basePosition + new Vector3(0, height, 0); // Back-left
        Vector3 v2 = basePosition + new Vector3(1, height, 0); // Back-right
        Vector3 v3 = basePosition + new Vector3(1, height, 1); // Front-right
        Vector3 v4 = basePosition + new Vector3(0, height, 1); // Front-left

        vertices.AddRange(new Vector3[] { apex, v1, v2, v3, v4 });

        // Create triangles for the roof
        int apexIndex = startIndex;
        int v1Index = startIndex + 1;
        int v2Index = startIndex + 2;
        int v3Index = startIndex + 3;
        int v4Index = startIndex + 4;

        // Define the triangles with correct winding order
        roofTriangles.AddRange(new int[]
        {
        // Back face (Z-)
        apexIndex, v2Index, v1Index,
        // Right face (X+)
        apexIndex, v3Index, v2Index,
        // Front face (Z+)
        apexIndex, v4Index, v3Index,
        // Left face (X-)
        apexIndex, v1Index, v4Index
        });

        // UVs for the roof
        uvs.Add(new Vector2(0.5f, 1.0f)); // Apex

        uvs.Add(new Vector2(0.0f, 0.0f)); // Back-left
        uvs.Add(new Vector2(1.0f, 0.0f)); // Back-right
        uvs.Add(new Vector2(1.0f, 1.0f)); // Front-right
        uvs.Add(new Vector2(0.0f, 1.0f)); // Front-left
    }

    void AddDoorsAndWindows(Transform buildingTransform, int[,] footprint, Vector3 buildingPosition, Dictionary<Vector3, float> cubeHeights)
    {
        int depth = footprint.GetLength(0);
        int width = footprint.GetLength(1);

        // Keep track of which faces have already had a door or window placed
        HashSet<string> occupiedPositions = new HashSet<string>();

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                if (footprint[z, x] == 1)
                {
                    Vector3 cubeBasePosition = new Vector3(x, 0, z) + buildingPosition;
                    float cubeHeight = cubeHeights[new Vector3(x, 0, z)];

                    // For each floor level
                    for (int y = 0; y < Mathf.FloorToInt(cubeHeight); y++)
                    {
                        Vector3 cubePosition = cubeBasePosition + new Vector3(0, y, 0);

                        // Check each face for being an external face
                        // Front face (z + 1)
                        if (IsExternalFace(footprint, z + 1, x))
                        {
                            TryPlacePrefab(cubePosition, Vector3.forward, 0, buildingTransform, occupiedPositions, y == 0);
                        }
                        // Back face (z - 1)
                        if (IsExternalFace(footprint, z - 1, x))
                        {
                            TryPlacePrefab(cubePosition, Vector3.back, 180, buildingTransform, occupiedPositions, y == 0);
                        }
                        // Right face (x + 1)
                        if (IsExternalFace(footprint, z, x + 1))
                        {
                            TryPlacePrefab(cubePosition, Vector3.right, 90, buildingTransform, occupiedPositions, y == 0);
                        }
                        // Left face (x - 1)
                        if (IsExternalFace(footprint, z, x - 1))
                        {
                            TryPlacePrefab(cubePosition, Vector3.left, -90, buildingTransform, occupiedPositions, y == 0);
                        }
                    }
                }
            }
        }
    }

    bool IsExternalFace(int[,] footprint, int z, int x)
    {
        int depth = footprint.GetLength(0);
        int width = footprint.GetLength(1);

        // Check if the position is out of bounds or has no cube
        if (z < 0 || z >= depth || x < 0 || x >= width)
        {
            return true; // Out of bounds means external face
        }

        return footprint[z, x] == 0; // No cube at adjacent position means external face
    }

    void TryPlacePrefab(Vector3 cubePosition, Vector3 direction, float yRotation, Transform parentTransform, HashSet<string> occupiedPositions, bool isGroundFloor)
    {
        // Ensure only one prefab per face per floor
        string faceKey = $"{cubePosition}_{direction}";
        if (occupiedPositions.Contains(faceKey))
            return; // Already placed a prefab on this face at this floor

        // Random chance to place a door, window, or nothing
        float randomValue = Random.value;
        GameObject prefabToInstantiate = null;

        if (isGroundFloor && randomValue < 0.3f && doorPrefabs.Length > 0) // 30% chance to place a door on ground floor
        {
            int index = Random.Range(0, doorPrefabs.Length);
            prefabToInstantiate = doorPrefabs[index];
        }
        else if (randomValue < 0.6f && windowPrefabs.Length > 0) // 30% chance to place a window
        {
            int index = Random.Range(0, windowPrefabs.Length);
            prefabToInstantiate = windowPrefabs[index];
        }
        // Else do nothing (remaining chance)

        if (prefabToInstantiate != null)
        {
            // Instantiate the prefab
            GameObject instance = Instantiate(prefabToInstantiate, parentTransform);

            // Calculate the center point of the wall face
            Vector3 faceCenter = cubePosition + direction * 0.5f + Vector3.up * 0.5f;

            // Get prefab dimensions
            Renderer prefabRenderer = instance.GetComponent<Renderer>();
            if (prefabRenderer == null)
            {
                Debug.LogError("Prefab does not have a Renderer component.");
                return;
            }

            float prefabHeight = prefabRenderer.bounds.size.y;
            float prefabDepth = prefabRenderer.bounds.size.z;

            // Adjust position for prefab pivot and depth
            Vector3 adjustedPosition = faceCenter + Vector3.up * (prefabHeight / 2f - 0.5f) + direction * (prefabDepth / 2f);

            // Set the position of the prefab
            instance.transform.position = adjustedPosition;

            // Rotate the prefab to face the correct direction
            instance.transform.rotation = Quaternion.LookRotation(-direction);

            occupiedPositions.Add(faceKey);
        }
    }
}