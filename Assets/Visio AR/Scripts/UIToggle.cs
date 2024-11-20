using UnityEngine;

public class UIToggle : MonoBehaviour
{
    [SerializeField]
    private GameObject targetObject;  // The GameObject to toggle

    private void Update()
    {
        // Check if the Y button on the left controller is pressed
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch))
        {
            // Toggle the active state of the target object
            if (targetObject != null)
            {
                targetObject.SetActive(!targetObject.activeSelf);
            }
        }
    }
}
