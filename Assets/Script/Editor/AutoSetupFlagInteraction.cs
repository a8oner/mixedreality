#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Mediapipe.Unity.Sample.HandLandmarkDetection;

[InitializeOnLoad]
public class AutoSetupFlagInteraction
{
    static AutoSetupFlagInteraction()
    {
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [MenuItem("Tools/Setup Flag Interaction")]
    public static void ForceSetup()
    {
        OnHierarchyChanged();
        Debug.Log("Forced Setup Complete.");
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            OnHierarchyChanged();
        }
    }

    private static void OnHierarchyChanged()
    {
        // Find HandLandmarkerRunner
        HandLandmarkerRunner runner = Object.FindObjectOfType<HandLandmarkerRunner>();
        if (runner == null) return;

        // Find all objects with "Flag" or "Bayrak" in their name
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.IndexOf("Flag", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                obj.name.IndexOf("Bayrak", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // Skip if it is not a scene object (e.g. prefab asset)
                if (obj.scene.rootCount == 0) continue;

                // Ensure FlagInteraction is attached
                FlagInteraction interaction = obj.GetComponent<FlagInteraction>();
                if (interaction == null)
                {
                    interaction = obj.AddComponent<FlagInteraction>();
                    Debug.Log($"AutoSetup: Added FlagInteraction to {obj.name}");
                }

                // Ensure AudioSource is attached
                if (obj.GetComponent<AudioSource>() == null)
                {
                    AudioSource audio = obj.AddComponent<AudioSource>();
                    audio.playOnAwake = false;
                    Debug.Log($"AutoSetup: Added AudioSource to {obj.name}");
                }

                // specific fix: Assign Runner reference if missing
                if (interaction.handLandmarkerRunner == null)
                {
                    interaction.handLandmarkerRunner = runner;
                    EditorUtility.SetDirty(interaction);
                    Debug.Log($"AutoSetup: Assigned HandLandmarkerRunner to {obj.name}");
                }
            }
        }
    }
}
#endif
