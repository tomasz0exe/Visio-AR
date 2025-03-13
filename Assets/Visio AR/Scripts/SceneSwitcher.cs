using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [Tooltip("Scene name to switch to for Dutch.")]
    public string dutchSceneName; // Scene name as a string

    [Tooltip("Scene name to switch to for English.")]
    public string englishSceneName; // Scene name as a string

    [Tooltip("Scene name to switch to for German.")]
    public string germanSceneName; // Scene name as a string

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // Detect X button press to switch to Dutch scene
        if (OVRInput.GetDown(OVRInput.Button.Three)) // 'X' button
        {
            SwitchToScene(dutchSceneName);
        }

        // Detect A button press to switch to English scene
        if (OVRInput.GetDown(OVRInput.Button.One)) // 'A' button
        {
            SwitchToScene(englishSceneName);
        }

        // Detect B button press to switch to German scene
        if (OVRInput.GetDown(OVRInput.Button.Two)) // 'B' button
        {
            SwitchToScene(germanSceneName);
        }
    }

    private void SwitchToScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}