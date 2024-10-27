using UnityEngine;

public class FloorDependentRotation : MonoBehaviour
{
    public Transform floor; // Reference to the floor object whose Y-rotation we want to match
    public GameObject linkedObject1; // The first object to rotate like a compass
    public GameObject linkedObject2; // The second object to rotate like a compass
    public GameObject linkedObject3; // The third object to rotate like a compass

    void Update()
    {
        if (floor == null) return;

        // Get the Y rotation of the floor
        float floorYRotation = floor.eulerAngles.y;

        // Create a new rotation with the same Y rotation as the floor, ignoring any X and Z rotations
        Quaternion newRotation = Quaternion.Euler(0, floorYRotation, 0);

        // Apply the rotation to the linked objects independently of their parent rotation
        if (linkedObject1 != null)
            linkedObject1.transform.rotation = newRotation;

        if (linkedObject2 != null)
            linkedObject2.transform.rotation = newRotation;

        if (linkedObject3 != null)
            linkedObject3.transform.rotation = newRotation;
    }
}