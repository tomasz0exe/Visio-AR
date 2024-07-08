using System.Collections.Generic;
using UnityEngine;

public class GridPlacer : MonoBehaviour
{
    public GameObject gridPrefab; // Prefab to place on the grid
    public GameObject prefabVariantA; // Variant A prefab
    public GameObject prefabVariantB; // Variant B prefab
    public GameObject prefabVariantC; // Variant C prefab
    public GameObject cylinderPrefab; // Prefab to visualize the raycast
    public GameObject leftController; // Reference to the left controller
    public float gridSize = 1f; // Size of each grid square
    public float maxCylinderLength = 3f; // Max length of the cylinder ray
    public string gridTag = "Grid"; // Tag to identify valid grid placement areas
    public string prefabLayerName = "PlacedPrefab"; // Layer name for placed prefabs
    public bool previewBreathing = false; // Enable or disable preview breathing
    [Range(0f, 0.5f)] public float breathScale = 0.1f; // Max additional scale factor for breathing
    [Range(0.1f, 5f)] public float breathTiming = 1f; // Time duration for a complete breathing cycle in seconds

    private List<GameObject> placedTiles = new List<GameObject>();
    private GameObject raycastCylinder;
    private GameObject previewTile;
    private float breathTimer = 0f;
    private int prefabLayer;

    void Start()
    {
        prefabLayer = LayerMask.NameToLayer(prefabLayerName);

        raycastCylinder = Instantiate(cylinderPrefab, Vector3.zero, Quaternion.identity);
        raycastCylinder.transform.localScale = new Vector3(0.01f, maxCylinderLength / 2, 0.01f); // Adjust the scale to match the ray length
        previewTile = Instantiate(gridPrefab, Vector3.zero, Quaternion.identity);
        previewTile.SetActive(false);
        //InvokeRepeating("UpdateTileVariants", 1, 1);
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
        {
            HandlePlacement();
            //UpdateTileVariants();
        }

        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))
        {
            HandleDeletion();
            //UpdateTileVariants();
        }
        VisualizeRaycast();
        ShowPreview();
        //UpdateTileVariants();
    }

    private void FixedUpdate()
    {
        UpdateTileVariants();
    }

    private void OnDisable()
    {
        if (raycastCylinder != null)
        {
            Destroy(raycastCylinder);
        }
        if (previewTile != null)
        {
            Destroy(previewTile);
        }
    }

    private void VisualizeRaycast()
    {
        Ray ray = new Ray(leftController.transform.position, leftController.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxCylinderLength))
        {
            raycastCylinder.transform.position = ray.origin + ray.direction * (hit.distance / 2);
            raycastCylinder.transform.localScale = new Vector3(0.01f, hit.distance / 2, 0.01f); // Adjust the length of the cylinder
            raycastCylinder.transform.rotation = Quaternion.LookRotation(ray.direction) * Quaternion.Euler(90, 0, 0); // Flip by 90 degrees
        }
        else
        {
            raycastCylinder.transform.position = ray.origin + ray.direction * (maxCylinderLength / 2);
            raycastCylinder.transform.localScale = new Vector3(0.01f, maxCylinderLength / 2, 0.01f); // Adjust the length of the cylinder
            raycastCylinder.transform.rotation = Quaternion.LookRotation(ray.direction) * Quaternion.Euler(90, 0, 0); // Flip by 90 degrees
        }
    }

    private void ShowPreview()
    {
        Ray ray = new Ray(leftController.transform.position, leftController.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxCylinderLength))
        {
            if (hit.collider.CompareTag(gridTag))
            {
                Vector3 hitPoint = hit.point;
                // Snap the position to the grid by rounding to the nearest grid unit
                Vector3 snappedPosition = new Vector3(Mathf.Round(hitPoint.x / gridSize) * gridSize, 0, Mathf.Round(hitPoint.z / gridSize) * gridSize);

                previewTile.transform.position = snappedPosition;
                previewTile.SetActive(true);

                if (previewBreathing)
                {
                    breathTimer += Time.deltaTime;
                    float scale = 1f + Mathf.Sin(breathTimer / breathTiming * 2 * Mathf.PI) * breathScale;
                    previewTile.transform.localScale = new Vector3(scale, scale, scale);
                }
                else
                {
                    previewTile.transform.localScale = Vector3.one;
                }
            }
            else
            {
                previewTile.SetActive(false);
            }
        }
        else
        {
            previewTile.SetActive(false);
        }
    }

    private void HandlePlacement()
    {
            Ray ray = new Ray(leftController.transform.position, leftController.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, maxCylinderLength))
            {
                if (hit.collider.CompareTag(gridTag))
                {
                    Vector3 hitPoint = hit.point;
                    // Snap the position to the grid by rounding to the nearest grid unit
                    Vector3 snappedPosition = new Vector3(Mathf.Round(hitPoint.x / gridSize) * gridSize, 0, Mathf.Round(hitPoint.z / gridSize) * gridSize);

                    if (IsPositionEmpty(snappedPosition))
                    {
                        GameObject newTile = Instantiate(gridPrefab, snappedPosition, Quaternion.identity);
                        newTile.layer = prefabLayer; // Assign the layer to the new prefab
                        AssignLayerRecursively(newTile, prefabLayer); // Assign the layer to all child objects
                        placedTiles.Add(newTile);
                    }
                }
            }
    }

    private void HandleDeletion()
    {
            Ray ray = new Ray(leftController.transform.position, leftController.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, maxCylinderLength))
            {
                GameObject hitObject = hit.collider.gameObject;
                GameObject rootObject = GetRootWithLayer(hitObject, prefabLayer);
                if (rootObject != null && placedTiles.Contains(rootObject))
                {
                    placedTiles.Remove(rootObject);
                    Destroy(rootObject);
                }
            }
    }

    private bool IsPositionEmpty(Vector3 position)
    {
        foreach (GameObject tile in placedTiles)
        {
            if (tile.transform.position == position)
            {
                return false;
            }
        }
        return true;
    }

    private void UpdateTileVariants()
    {
        foreach (GameObject tile in placedTiles)
        {
            Vector3 position = tile.transform.position;
            int neighbourCount = 0;
            bool left = false, right = false, up = false, down = false;

            foreach (GameObject otherTile in placedTiles)
            {
                if (otherTile == tile) continue;

                Vector3 otherPosition = otherTile.transform.position;
                if (otherPosition == position + Vector3.left * gridSize) { neighbourCount++; left = true; }
                if (otherPosition == position + Vector3.right * gridSize) { neighbourCount++; right = true; }
                if (otherPosition == position + Vector3.forward * gridSize) { neighbourCount++; up = true; }
                if (otherPosition == position + Vector3.back * gridSize) { neighbourCount++; down = true; }
            }

            if (neighbourCount == 0 || neighbourCount == 1)
            {
                ChangePrefab(tile, prefabVariantA);
            }
            else if (neighbourCount == 2 && left && right)
            {
                ChangePrefab(tile, prefabVariantB);
            }
            else if (neighbourCount == 2 && up && down)
            {
                ChangePrefab(tile, prefabVariantC);
            }
            else
            {
                ChangePrefab(tile, prefabVariantA);
            }
        }
    }

    private void ChangePrefab(GameObject oldTile, GameObject newPrefab)
    {
        Vector3 position = oldTile.transform.position;
        Quaternion rotation = oldTile.transform.rotation;
        Destroy(oldTile);
        GameObject newTile = Instantiate(newPrefab, position, rotation);
        newTile.layer = prefabLayer; // Assign the layer to the new prefab
        AssignLayerRecursively(newTile, prefabLayer); // Assign the layer to all child objects
        placedTiles.Remove(oldTile);
        placedTiles.Add(newTile);
    }

    private void AssignLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            AssignLayerRecursively(child.gameObject, layer);
        }
    }

    private GameObject GetRootWithLayer(GameObject obj, int layer)
    {
        while (obj != null)
        {
            if (obj.layer == layer)
            {
                return obj;
            }
            if (obj.transform.parent == null)
            {
                return null;
            }
            obj = obj.transform.parent.gameObject;
        }
        return null;
    }
}