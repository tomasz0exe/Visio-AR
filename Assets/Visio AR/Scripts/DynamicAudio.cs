using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DynamicAudio : MonoBehaviour
{
    public string targetTag = "Target"; // Tag of the target game objects
    public float minVolume = 0.1f; // Minimum volume when far from target
    public float maxVolume = 1.0f; // Maximum volume when near the target
    public float minPitch = 0.5f; // Minimum pitch when far from target
    public float maxPitch = 1.5f; // Maximum pitch when near the target
    public float maxDistance = 20.0f; // Maximum distance for audio to start playing

    private AudioSource audioSource;
    private GameObject[] targets;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Find all game objects with the specified tag
        targets = GameObject.FindGameObjectsWithTag(targetTag);

        if (targets.Length == 0)
        {
            audioSource.volume = 0;
            return; // No targets, no sound
        }

        // Find the closest target
        GameObject closestTarget = GetClosestTarget();

        // Calculate the distance to the closest target
        float distance = Vector3.Distance(transform.position, closestTarget.transform.position);

        // Adjust audio volume and pitch based on the distance
        if (distance < maxDistance)
        {
            float t = Mathf.InverseLerp(maxDistance, 0, distance); // Normalize distance (0 to maxDistance)
            audioSource.volume = Mathf.Lerp(minVolume, maxVolume, t);
            audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, t);

            if (!audioSource.isPlaying)
                audioSource.Play(); // Start playing the audio if it's not playing
        }
        else
        {
            audioSource.volume = 0; // Mute the audio if out of range
            audioSource.Stop(); // Stop playing if too far
        }
    }

    GameObject GetClosestTarget()
    {
        GameObject closest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject target in targets)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = target;
            }
        }

        return closest;
    }

    // Stop audio when the script is disabled
    void OnDisable()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}