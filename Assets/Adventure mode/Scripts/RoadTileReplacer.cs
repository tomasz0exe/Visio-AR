using UnityEngine;
using System.Collections.Generic;

public class RoadTileReplacer : MonoBehaviour
{
    // Prefabs for each road type
    public GameObject roadEndPrefab;
    public GameObject straightRoadPrefab;
    public GameObject curvedRoadPrefab;
    public GameObject tRoadPrefab;
    public GameObject crossRoadPrefab;
    public GameObject holeRoadPrefab;

    // Reference to the source prefab that will be replaced
    public GameObject sourcePrefab;

    // Reference to the actual grid GameObject
    public GameObject gridObject;

    // Grid size and reference to the original grid of road tiles
    private GameObject[,] grid;
    private float prefabWidth;
    private float prefabHeight;
    private bool isGridInitialized = false;

    // Object pool for reuse
    private Dictionary<string, Queue<GameObject>> objectPools = new Dictionary<string, Queue<GameObject>>();

    // Direction vectors to check neighbors (Up, Down, Left, Right)
    private static readonly Vector2Int[] directions = {
        new Vector2Int(0, 1),  // Up
        new Vector2Int(0, -1), // Down
        new Vector2Int(-1, 0), // Left
        new Vector2Int(1, 0)   // Right
    };

    // Reusable neighbor array (fixed size to reduce allocations)
    private readonly Vector2Int[] reusableNeighbors = new Vector2Int[4];

    // Entry point to initialize the grid in FixedUpdate
    private void FixedUpdate()
    {
        if (!isGridInitialized && gridObject != null)
        {
            InitializeGrid();
            isGridInitialized = true;
        }

        if (isGridInitialized)
        {
            ReplaceTiles();
        }
    }

    // Initializes the grid based on the gridObject's position and rotation
    private void InitializeGrid()
    {
        if (gridObject != null && sourcePrefab != null)
        {
            prefabWidth = sourcePrefab.GetComponent<Renderer>().bounds.size.x;
            prefabHeight = sourcePrefab.GetComponent<Renderer>().bounds.size.y;

            Vector3 gridSize = gridObject.transform.localScale;
            int gridSizeX = Mathf.FloorToInt(gridSize.x / prefabWidth);
            int gridSizeY = Mathf.FloorToInt(gridSize.y / prefabHeight);

            grid = new GameObject[gridSizeX, gridSizeY];

            PopulateGrid(gridObject.transform.position, gridObject.transform.rotation);
        }
    }

    // Populate the grid with the source prefabs
    private void PopulateGrid(Vector3 gridOrigin, Quaternion gridRotation)
    {
        // Cache source prefabs placed in the scene (run only once during initialization)
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name == sourcePrefab.name)
            {
                Vector2Int localGridPosition = GetGridPosition(obj.transform.position);
                if (localGridPosition.x >= 0 && localGridPosition.x < grid.GetLength(0) && localGridPosition.y >= 0 && localGridPosition.y < grid.GetLength(1))
                {
                    grid[localGridPosition.x, localGridPosition.y] = obj;
                }
            }
        }
    }

    // Converts world position to grid position based on prefab size
    private Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        int gridX = Mathf.FloorToInt(worldPosition.x / prefabWidth);
        int gridY = Mathf.FloorToInt(worldPosition.y / prefabHeight);
        return new Vector2Int(gridX, gridY);
    }

    // Replaces tiles based on the neighbors
    private void ReplaceTiles()
    {
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                GameObject currentTile = grid[x, y];

                if (currentTile != null && currentTile.name == sourcePrefab.name)
                {
                    int neighborCount = 0;

                    // Use the reusable neighbor array to avoid allocating a new one
                    for (int i = 0; i < directions.Length; i++)
                    {
                        Vector2Int dir = directions[i];
                        int neighborX = x + dir.x;
                        int neighborY = y + dir.y;

                        if (neighborX >= 0 && neighborX < grid.GetLength(0) && neighborY >= 0 && neighborY < grid.GetLength(1))
                        {
                            GameObject neighborTile = grid[neighborX, neighborY];

                            if (neighborTile != null && neighborTile.name == sourcePrefab.name)
                            {
                                reusableNeighbors[neighborCount++] = dir;
                            }
                        }
                    }

                    ReplaceTile(currentTile, neighborCount, reusableNeighbors, x, y);
                }
            }
        }
    }

    // Replaces the tile based on the neighbor count and direction
    private void ReplaceTile(GameObject currentTile, int neighborCount, Vector2Int[] neighborDirections, int x, int y)
    {
        GameObject prefabToUse = null;
        Quaternion rotation = Quaternion.identity;

        switch (neighborCount)
        {
            case 0:
                prefabToUse = holeRoadPrefab;
                break;
            case 1:
                prefabToUse = roadEndPrefab;
                rotation = GetRotationForEnd(neighborDirections[0]);
                break;
            case 2:
                if (IsStraight(neighborDirections))
                {
                    prefabToUse = straightRoadPrefab;
                    rotation = GetRotationForStraight(neighborDirections);
                }
                else
                {
                    prefabToUse = curvedRoadPrefab;
                    rotation = GetRotationForCurve(neighborDirections);
                }
                break;
            case 3:
                prefabToUse = tRoadPrefab;
                rotation = GetRotationForT(neighborDirections);
                break;
            case 4:
                prefabToUse = crossRoadPrefab;
                break;
        }

        if (prefabToUse != null)
        {
            if (currentTile.name != prefabToUse.name)  // Only replace if necessary
            {
                GameObject newTile = GetPooledObject(prefabToUse);
                newTile.transform.position = currentTile.transform.position;
                newTile.transform.rotation = rotation;

                // Reuse the current position in the grid and mark the currentTile as inactive instead of destroying it
                grid[x, y] = newTile;
                ReturnToPool(currentTile);
            }
        }
    }

    // Object pooling: Gets a pooled object, or creates one if none are available
    private GameObject GetPooledObject(GameObject prefab)
    {
        string prefabName = prefab.name;
        if (!objectPools.ContainsKey(prefabName))
        {
            objectPools[prefabName] = new Queue<GameObject>();
        }

        if (objectPools[prefabName].Count > 0)
        {
            GameObject pooledObject = objectPools[prefabName].Dequeue();
            pooledObject.SetActive(true);
            return pooledObject;
        }
        else
        {
            return Instantiate(prefab);
        }
    }

    // Returns the object back to the pool
    private void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        string prefabName = obj.name.Replace("(Clone)", "").Trim();
        objectPools[prefabName].Enqueue(obj);
    }

    // Helper functions to calculate rotations
    private Quaternion GetRotationForEnd(Vector2Int direction)
    {
        if (direction == Vector2Int.up) return Quaternion.Euler(0, 0, 0);
        if (direction == Vector2Int.right) return Quaternion.Euler(0, 90, 0);
        if (direction == Vector2Int.down) return Quaternion.Euler(0, 180, 0);
        if (direction == Vector2Int.left) return Quaternion.Euler(0, -90, 0);
        return Quaternion.identity;
    }

    private Quaternion GetRotationForStraight(Vector2Int[] directions)
    {
        if ((directions[0] == Vector2Int.up && directions[1] == Vector2Int.down) ||
            (directions[0] == Vector2Int.down && directions[1] == Vector2Int.up))
        {
            return Quaternion.Euler(0, 0, 0);
        }
        if ((directions[0] == Vector2Int.left && directions[1] == Vector2Int.right) ||
            (directions[0] == Vector2Int.right && directions[1] == Vector2Int.left))
        {
            return Quaternion.Euler(0, 90, 0);
        }
        return Quaternion.identity;
    }

    private Quaternion GetRotationForCurve(Vector2Int[] directions)
    {
        if ((directions[0] == Vector2Int.up && directions[1] == Vector2Int.right) ||
            (directions[0] == Vector2Int.right && directions[1] == Vector2Int.up))
        {
            return Quaternion.Euler(0, 90, 0);
        }
        if ((directions[0] == Vector2Int.down && directions[1] == Vector2Int.right) ||
            (directions[0] == Vector2Int.right && directions[1] == Vector2Int.down))
        {
            return Quaternion.Euler(0, 180, 0);
        }
        if ((directions[0] == Vector2Int.down && directions[1] == Vector2Int.left) ||
            (directions[0] == Vector2Int.left && directions[1] == Vector2Int.down))
        {
            return Quaternion.Euler(0, -90, 0);
        }
        if ((directions[0] == Vector2Int.up && directions[1] == Vector2Int.left) ||
            (directions[0] == Vector2Int.left && directions[1] == Vector2Int.up))
        {
            return Quaternion.Euler(0, 0, 0);
        }
        return Quaternion.identity;
    }

    private Quaternion GetRotationForT(Vector2Int[] directions)
    {
        if (!System.Array.Exists(directions, dir => dir == Vector2Int.down)) return Quaternion.Euler(0, 0, 0);
        if (!System.Array.Exists(directions, dir => dir == Vector2Int.left)) return Quaternion.Euler(0, 90, 0);
        if (!System.Array.Exists(directions, dir => dir == Vector2Int.up)) return Quaternion.Euler(0, 180, 0);
        if (!System.Array.Exists(directions, dir => dir == Vector2Int.right)) return Quaternion.Euler(0, -90, 0);
        return Quaternion.identity;
    }

    private bool IsStraight(Vector2Int[] directions)
    {
        return (directions[0] == Vector2Int.up && directions[1] == Vector2Int.down) ||
               (directions[0] == Vector2Int.down && directions[1] == Vector2Int.up) ||
               (directions[0] == Vector2Int.left && directions[1] == Vector2Int.right) ||
               (directions[0] == Vector2Int.right && directions[1] == Vector2Int.left);
    }
}