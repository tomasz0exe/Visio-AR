using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Import SceneManager

public class VRScriptManager : MonoBehaviour
{
    public enum Phase
    {
        Calibration,
        Setup,
        Play
    }

    [Tooltip("List of GameObjects to be active during Calibration phase")]
    public List<GameObject> calibrationObjects;

    [Tooltip("List of GameObjects to be active during Setup phase")]
    public List<GameObject> setupObjects;

    [Tooltip("List of GameObjects to be active during Play phase")]
    public List<GameObject> playObjects;

    [Tooltip("Name of the scene to load when left joystick is pressed during Calibration phase")]
    public string calibrationSceneName; // Scene name as a string

    private Phase currentPhase = Phase.Calibration;
    private float pressDuration = 2.0f;
    private float leftPressTime = 0.0f;
    private float rightPressTime = 0.0f;

    void Start()
    {
        // Start in Calibration phase
        ActivatePhase(Phase.Calibration);
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // Right joystick press for 2 seconds to move to the next phase
        if (OVRInput.Get(OVRInput.Button.SecondaryThumbstick))
        {
            rightPressTime += Time.deltaTime;
            if (rightPressTime >= pressDuration)
            {
                ActivateNextPhase();
                rightPressTime = 0.0f;
            }
        }
        else
        {
            rightPressTime = 0.0f;
        }

        // Left joystick press for 2 seconds to move to the previous phase or load a scene if in Calibration phase
        if (OVRInput.Get(OVRInput.Button.PrimaryThumbstick))
        {
            leftPressTime += Time.deltaTime;
            if (leftPressTime >= pressDuration)
            {
                if (currentPhase == Phase.Calibration)
                {
                    LoadCalibrationScene();
                }
                else
                {
                    ActivatePreviousPhase();
                }
                leftPressTime = 0.0f;
            }
        }
        else
        {
            leftPressTime = 0.0f;
        }
    }

    void LoadCalibrationScene()
    {
        if (!string.IsNullOrEmpty(calibrationSceneName))
        {
            // Load the scene by name
            SceneManager.LoadScene(calibrationSceneName);
        }
        else
        {
            Debug.LogWarning("Calibration scene name is not set in the inspector!");
        }
    }

    void ActivateNextPhase()
    {
        switch (currentPhase)
        {
            case Phase.Calibration:
                ActivatePhase(Phase.Setup);
                break;
            case Phase.Setup:
                ActivatePhase(Phase.Play);
                break;
            case Phase.Play:
                // Do nothing, already at last phase
                break;
        }
    }

    void ActivatePreviousPhase()
    {
        switch (currentPhase)
        {
            case Phase.Play:
                ActivatePhase(Phase.Setup);
                break;
            case Phase.Setup:
                ActivatePhase(Phase.Calibration);
                break;
            case Phase.Calibration:
                // Do nothing, already at first phase
                break;
        }
    }

    void ActivatePhase(Phase newPhase)
    {
        // Deactivate all objects for all phases
        SetObjectsActive(calibrationObjects, false);
        SetObjectsActive(setupObjects, false);
        SetObjectsActive(playObjects, false);

        // Activate objects for the new phase
        switch (newPhase)
        {
            case Phase.Calibration:
                SetObjectsActive(calibrationObjects, true);
                break;
            case Phase.Setup:
                SetObjectsActive(setupObjects, true);
                break;
            case Phase.Play:
                SetObjectsActive(playObjects, true);
                break;
        }

        currentPhase = newPhase;
    }

    void SetObjectsActive(List<GameObject> objects, bool active)
    {
        foreach (var obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(active);
            }
        }
    }
}