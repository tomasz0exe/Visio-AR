using UnityEngine;
using System.Collections;

public class CollisionSoundController : MonoBehaviour
{
    public string targetTag = "YourTargetTag"; // The tag of the colliding object to trigger the sound
    public AudioClip collisionSound; // The sound to play on collision

    [Range(0f, 1f)]
    public float minVolume = 0.1f; // Minimum sound volume
    [Range(0f, 1f)]
    public float maxVolume = 1f; // Maximum sound volume

    [Range(0.1f, 3f)]
    public float minPitch = 0.5f; // Minimum sound pitch
    [Range(0.1f, 3f)]
    public float maxPitch = 2f; // Maximum sound pitch

    public float maxDistance = 10f; // Maximum distance from the center for volume and pitch calculation
    public float transitionSpeed = 1f; // Speed of volume transition between collisions

    private AudioSource audioSource;
    private Collision currentCollision = null;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = collisionSound;
        audioSource.loop = true; // Loop the sound while colliding
        audioSource.spatialBlend = 1.0f; // Makes the audio 3D
        audioSource.volume = 0f; // Start with volume at 0
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag(targetTag))
        {
            UpdateClosestCollision(collision);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag(targetTag))
        {
            UpdateClosestCollision(collision);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag(targetTag) && collision == currentCollision)
        {
            currentCollision = null;
            StartCoroutine(FadeOut());
        }
    }

    void UpdateClosestCollision(Collision collision)
    {
        if (currentCollision == null || IsCloserToCenter(collision))
        {
            currentCollision = collision;
            UpdateAudio(collision);
            StartCoroutine(FadeIn());
        }
    }

    bool IsCloserToCenter(Collision collision)
    {
        Vector3 contactPoint = collision.contacts[0].point;
        float newDistance = Vector3.Distance(contactPoint, transform.position);

        if (currentCollision != null)
        {
            Vector3 currentContactPoint = currentCollision.contacts[0].point;
            float currentDistance = Vector3.Distance(currentContactPoint, transform.position);
            return newDistance < currentDistance;
        }

        return true;
    }

    void UpdateAudio(Collision collision)
    {
        if (collision == null) return;

        Vector3 contactPoint = collision.contacts[0].point;
        float distance = Vector3.Distance(contactPoint, transform.position);
        float proximityFactor = Mathf.Clamp01(1 - distance / maxDistance);

        audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, proximityFactor);

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    IEnumerator FadeIn()
    {
        while (currentCollision != null && audioSource.volume < maxVolume)
        {
            audioSource.volume += Time.deltaTime * transitionSpeed;
            yield return null;
        }
        audioSource.volume = maxVolume;
    }

    IEnumerator FadeOut()
    {
        while (audioSource.volume > 0)
        {
            audioSource.volume -= Time.deltaTime * transitionSpeed;
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = 0f;
    }
}
