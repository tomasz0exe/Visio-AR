using UnityEngine;

public class FloorRotator : MonoBehaviour
{
    public GameObject floor; // Reference to the floor GameObject
    public GameObject leftController; // Reference to the left controller
    public GameObject cylinderPrefab; // Prefab to visualize the raycast
    public float maxCylinderLength = 3f; // Max length of the cylinder ray

    private bool isRotating = false; // Tracks if rotation is active
    private MeshRenderer floorMeshRenderer;
    private GameObject raycastCylinder;

    void Start()
    {
        // Get the MeshRenderer component of the floor GameObject
        floorMeshRenderer = floor.GetComponent<MeshRenderer>();

        // Instantiate and initialize the raycast visual cylinder
        raycastCylinder = Instantiate(cylinderPrefab, Vector3.zero, Quaternion.identity);
        raycastCylinder.transform.localScale = new Vector3(0.01f, maxCylinderLength / 2, 0.01f); // Initial scale
        raycastCylinder.SetActive(false); // Disable by default
    }

    void Update()
    {
        // Check if the left trigger is pressed to start or stop rotating
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
        {
            isRotating = true;
        }
        else if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
        {
            isRotating = false;
        }

        // Rotate the floor while the trigger is held
        if (isRotating)
        {
            RotateFloor();
        }

        // Visualize the raycast at all times
        VisualizeRaycast();
    }

    private void RotateFloor()
    {
        // Raycast from the left controller to the floor
        Ray ray = new Ray(leftController.transform.position, leftController.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 playerPosition = new Vector3(0, floor.transform.position.y, 0); // Player starts in the middle
            Vector3 hitPoint = hit.point;
            Vector3 direction = hitPoint - playerPosition; // Calculate the direction from the center

            // Calculate the angle to rotate around the Y axis
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            // Rotate the floor to face the direction the controller is pointing
            floor.transform.rotation = Quaternion.Euler(0, angle, 0);
        }
    }

    private void VisualizeRaycast()
    {
        // Raycast from the left controller's forward direction
        Ray ray = new Ray(leftController.transform.position, leftController.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxCylinderLength))
        {
            // Set the cylinder position and scale based on raycast hit
            raycastCylinder.transform.position = ray.origin + ray.direction * (hit.distance / 2);
            raycastCylinder.transform.localScale = new Vector3(0.01f, hit.distance / 2, 0.01f); // Adjust the length of the cylinder
            raycastCylinder.transform.rotation = Quaternion.LookRotation(ray.direction) * Quaternion.Euler(90, 0, 0); // Flip by 90 degrees
            raycastCylinder.SetActive(true);
        }
        else
        {
            // If no hit, extend the ray to maxCylinderLength
            raycastCylinder.transform.position = ray.origin + ray.direction * (maxCylinderLength / 2);
            raycastCylinder.transform.localScale = new Vector3(0.01f, maxCylinderLength / 2, 0.01f); // Max length of the cylinder
            raycastCylinder.transform.rotation = Quaternion.LookRotation(ray.direction) * Quaternion.Euler(90, 0, 0);
            raycastCylinder.SetActive(true);
        }
    }

    // This method is called when the script is enabled
    private void OnEnable()
    {
        // Enable the MeshRenderer of the floor
        if (floorMeshRenderer != null)
        {
            floorMeshRenderer.enabled = true;
        }

        // Enable raycast visualization
        if (raycastCylinder != null)
        {
            raycastCylinder.SetActive(true);
        }
    }

    // This method is called when the script is disabled
    private void OnDisable()
    {
        // Disable the MeshRenderer of the floor
        if (floorMeshRenderer != null)
        {
            floorMeshRenderer.enabled = false;
        }

        // Disable raycast visualization
        if (raycastCylinder != null)
        {
            raycastCylinder.SetActive(false);
        }
    }
}