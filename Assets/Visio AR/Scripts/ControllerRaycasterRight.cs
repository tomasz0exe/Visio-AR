using UnityEngine;
using System.Collections;

public class VRRaycastControllerRight : MonoBehaviour
{
    public Transform controllerTransform; // The transform of the controller
    public Transform raycastOriginTransform; // The transform from which the raycast will originate
    public float rayLength = 2.0f;
    public float vibrationDuration = 0.1f; // Duration of the vibration in seconds
    public float vibrationIntensity = 1.0f; // Intensity of the vibration (0 to 1)

    public GameObject rayVisualPrefab; // Prefab for visualizing the ray
    public Material hitMaterial; // Material for when the ray hits something
    public Material noHitMaterial; // Material for when the ray doesn't hit anything

    public GameObject targetPoint; // Empty game object to be moved based on raycast
    public string hitTag = "Interactable"; // Tag to check for vibration
    public MonoBehaviour additionalScriptToToggle; // Script to toggle along with the raycast

    private int ignoreRaycastLayerMask; // Layer mask to ignore raycasts on "Ignore Raycast" layer

    private GameObject rayVisual;
    private MeshRenderer rayVisualRenderer;
    private bool isEnabled = true; // Toggle to enable or disable everything

    void Start()
    {
        ignoreRaycastLayerMask = LayerMask.GetMask("Ignore Raycast");

        if (rayVisualPrefab != null)
        {
            rayVisual = Instantiate(rayVisualPrefab);
            rayVisualRenderer = rayVisual.GetComponent<MeshRenderer>();
        }
    }

    void Update()
    {
        // Check for button press to toggle enable/disable
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            isEnabled = !isEnabled;
            ToggleComponents(isEnabled);
        }

        if (!isEnabled)
        {
            if (rayVisual != null)
            {
                rayVisual.SetActive(false);
            }
            return;
        }

        if (raycastOriginTransform == null)
        {
            return;
        }

        Vector3 rayOriginPosition = raycastOriginTransform.position;
        Vector3 rayDirection = raycastOriginTransform.forward;

        Ray ray = new Ray(rayOriginPosition, rayDirection);
        RaycastHit hit;

        // Use layer mask to ignore collisions with objects on "Ignore Raycast" layer
        if (Physics.Raycast(ray, out hit, rayLength, ~ignoreRaycastLayerMask))
        {
            // Visualize the raycast with a cylinder prefab
            VisualizeRaycast(rayOriginPosition, hit.point);

            // Move the target point to the hit position
            if (targetPoint != null)
            {
                targetPoint.transform.position = hit.point;
            }

            if (hit.collider != null && hit.collider.CompareTag(hitTag))
            {
                // Vibrate the controller for a short duration
                StartCoroutine(VibrateController(vibrationDuration, vibrationIntensity)); // Use the editable duration and intensity

                if (rayVisualRenderer != null)
                {
                    rayVisualRenderer.material = hitMaterial;
                }
            }
            else
            {
                if (rayVisualRenderer != null)
                {
                    rayVisualRenderer.material = noHitMaterial;
                }
            }
        }
        else
        {
            // Visualize the raycast up to the maximum length if it doesn't hit anything
            VisualizeRaycast(rayOriginPosition, rayOriginPosition + rayDirection * rayLength);

            if (rayVisualRenderer != null)
            {
                rayVisualRenderer.material = noHitMaterial;
            }

            // Move the target point to the end of the ray
            if (targetPoint != null)
            {
                targetPoint.transform.position = ray.origin + ray.direction * rayLength;
            }
        }
    }

    private void VisualizeRaycast(Vector3 start, Vector3 end)
    {
        if (rayVisual != null)
        {
            Vector3 direction = end - start;
            float distance = direction.magnitude;

            rayVisual.transform.position = start + direction / 2;
            rayVisual.transform.rotation = Quaternion.LookRotation(direction);
            rayVisual.transform.localScale = new Vector3(0.01f, 0.01f, distance);

            // Ensure the ray visualizer is active
            rayVisual.SetActive(true);
        }
    }

    private IEnumerator VibrateController(float duration, float intensity)
    {
        OVRInput.SetControllerVibration(intensity, intensity, OVRInput.Controller.RTouch);
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
    }

    private void ToggleComponents(bool state)
    {
        if (additionalScriptToToggle != null)
        {
            additionalScriptToToggle.enabled = state;
        }
    }
}
