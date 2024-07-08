using System.Collections.Generic;
using UnityEngine;

public class VRScriptManager : MonoBehaviour
{
    [Tooltip("List of scripts to be active during setup phase")]
    public List<MonoBehaviour> setupScripts;

    [Tooltip("List of scripts to be active during running phase")]
    public List<MonoBehaviour> runningScripts;

    [Tooltip("Tag for objects to keep alive after transition")]
    public string keepTag = "KeepAlive";

    [Tooltip("Parent transform for all setup-related objects")]
    public Transform setupContainer;

    [Tooltip("List of additional GameObjects to destroy outside the setup container")]
    public List<GameObject> additionalObjectsToDestroy;

    private bool isSetupActive = true;
    private float pressDuration = 2.0f;
    private float pressTime = 0.0f;

    void Start()
    {
        // Ensure setup scripts are enabled and running scripts are disabled initially
        SetScriptsActive(setupScripts, true);
        SetScriptsActive(runningScripts, false);
    }

    void Update()
    {
        // Check if the left joystick is pressed for the required duration
        if (OVRInput.Get(OVRInput.Button.PrimaryThumbstick))
        {
            pressTime += Time.deltaTime;
            if (pressTime >= pressDuration)
            {
                // Transition to running scripts
                TransitionToRunningScripts();
            }
        }
        else
        {
            pressTime = 0.0f;
        }
    }

    void TransitionToRunningScripts()
    {
        if (isSetupActive)
        {
            // Disable setup scripts and destroy their objects unless tagged to keep
            foreach (var script in setupScripts)
            {
                if (script != null)
                {
                    if (!script.gameObject.CompareTag(keepTag))
                    {
                        Destroy(script.gameObject);
                    }
                    else
                    {
                        script.enabled = false;
                    }
                }
            }

            // Destroy all objects in the setup container unless they are tagged to keep
            List<GameObject> objectsToDestroy = new List<GameObject>();
            foreach (Transform child in setupContainer)
            {
                if (!child.CompareTag(keepTag))
                {
                    objectsToDestroy.Add(child.gameObject);
                }
            }

            foreach (var obj in objectsToDestroy)
            {
                Destroy(obj);
            }

            // Destroy additional objects outside the setup container unless tagged to keep
            foreach (var obj in additionalObjectsToDestroy)
            {
                if (obj != null && !obj.CompareTag(keepTag))
                {
                    Destroy(obj);
                }
            }

            // Enable running scripts
            SetScriptsActive(runningScripts, true);

            // Mark setup as inactive and disable this script
            isSetupActive = false;
            this.enabled = false;
        }
    }

    void SetScriptsActive(List<MonoBehaviour> scripts, bool active)
    {
        foreach (var script in scripts)
        {
            if (script != null)
            {
                script.enabled = active;
            }
        }
    }
}