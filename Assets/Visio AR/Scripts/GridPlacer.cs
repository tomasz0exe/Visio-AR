using System.Collections.Generic;
using UnityEngine;

public class GridPlacer : MonoBehaviour
{
    public GameObject prefabA; // Prefab A
    public GameObject prefabB; // Prefab B
    public GameObject prefabC; // Prefab C
    public GameObject previewPrefab; // Unique Preview Tile Prefab
    public GameObject cylinderPrefab; // Prefab to visualize the raycast
    public GameObject leftController; // Reference to the left controller
    public GameObject snapToObject; // Specific GameObject for snapping the grid and rotation
    public float gridSize = 1f; // Size of each grid square
    public float maxCylinderLength = 3f; // Max length of the cylinder ray
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
        raycastCylinder.SetActive(false); // Disable by default

        // Instantiate the preview tile but keep it inactive
        previewTile = Instantiate(previewPrefab, Vector3.zero, Quaternion.identity);
        previewTile.SetActive(false);
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
        {
            PlacePrefab(prefabA); // Place Prefab A
        }

        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))
        {
            PlacePrefab(prefabB); // Place Prefab B
        }

        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch))
        {
            PlacePrefab(prefabC); // Place Prefab C
        }

        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))
        {
            HandleDeletion(); // Remove tile
        }

        VisualizeRaycast();
        ShowPreview();
    }

    private void VisualizeRaycast()
    {
        Ray ray = new Ray(leftController.transform.position, leftController.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxCylinderLength))
        {
            raycastCylinder.transform.position = ray.origin + ray.direction * (hit.distance / 2);
            raycastCylinder.transform.localScale = new Vector3(0.01f, hit.distance / 2, 0.01f); // Adjust the length of the cylinder
            raycastCylinder.transform.rotation = Quaternion.LookRotation(ray.direction) * Quaternion.Euler(90, 0, 0); // Flip by 90 degrees
            raycastCylinder.SetActive(true);
        }
        else
        {
            raycastCylinder.transform.position = ray.origin + ray.direction * (maxCylinderLength / 2);
            raycastCylinder.transform.localScale = new Vector3(0.01f, maxCylinderLength / 2, 0.01f); // Adjust the length of the cylinder
            raycastCylinder.transform.rotation = Quaternion.LookRotation(ray.direction) * Quaternion.Euler(90, 0, 0); // Flip by 90 degrees
            raycastCylinder.SetActive(true);
        }
    }

    private void ShowPreview()
    {
        Ray ray = new Ray(leftController.transform.position, leftController.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxCylinderLength))
        {
            if (hit.collider.gameObject == snapToObject)
            {
                Vector3 hitPoint = hit.point;

                // Snap to specific GameObject's grid and apply group rotation
                Vector3 snappedPosition = SnapToGrid(hitPoint, snapToObject.transform.rotation, snapToObject.transform);
                previewTile.transform.position = snappedPosition;

                // Rotate the preview tile based on the snapToObject's rotation
                previewTile.transform.rotation = snapToObject.transform.rotation;

                previewTile.SetActive(true);

                // Breathing animation
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

    private Vector3 SnapToGrid(Vector3 hitPoint, Quaternion rotation, Transform snapToObject)
    {
        // Snap the position to the grid by rounding to the nearest grid unit
        Vector3 localPoint = Quaternion.Inverse(rotation) * hitPoint; // Transform into local space
        localPoint = new Vector3(Mathf.Round(localPoint.x / gridSize) * gridSize, snapToObject.position.y, Mathf.Round(localPoint.z / gridSize) * gridSize);
        return rotation * localPoint; // Transform back to world space
    }

    private void PlacePrefab(GameObject prefab)
    {
        Ray ray = new Ray(leftController.transform.position, leftController.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxCylinderLength))
        {
            if (hit.collider.gameObject == snapToObject)
            {
                Vector3 hitPoint = hit.point;
                Vector3 snappedPosition = SnapToGrid(hitPoint, snapToObject.transform.rotation, snapToObject.transform);

                if (IsPositionEmpty(snappedPosition))
                {
                    GameObject newTile = Instantiate(prefab, snappedPosition, snapToObject.transform.rotation);
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

    private void OnDisable()
    {
        if (raycastCylinder != null)
        {
            raycastCylinder.SetActive(false);
        }
        if (previewTile != null)
        {
            previewTile.SetActive(false);
        }
    }

}
