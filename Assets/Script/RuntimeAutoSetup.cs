using UnityEngine;
using Mediapipe.Unity.Sample.HandLandmarkDetection;

public class RuntimeAutoSetup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnRuntimeMethodLoad()
    {
        Debug.Log("[RuntimeAutoSetup] Checking for Flag objects...");

        // 1. Find the Runner
        HandLandmarkerRunner runner = Object.FindObjectOfType<HandLandmarkerRunner>();
        if (runner == null)
        {
            Debug.LogError("[RuntimeAutoSetup] HandLandmarkerRunner NOT FOUND! Interaction will not work.");
        }

        // 2. Find Potential Flag Objects
        // We look for objects with common names
        string[] searchTerms = new string[] { "Flag", "Bayrak", "Palestine", "Filistin" };
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            bool match = false;
            foreach (string term in searchTerms)
            {
                if (obj.name.IndexOf(term, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    match = true;
                    break;
                }
            }

            if (match)
            {
                Debug.Log($"[RuntimeAutoSetup] Found specific object: {obj.name}");
                SetupObject(obj, runner);
            }
        }
    }

    static void SetupObject(GameObject obj, HandLandmarkerRunner runner)
    {
        // Add FlagInteraction if missing
        FlagInteraction interaction = obj.GetComponent<FlagInteraction>();
        if (interaction == null)
        {
            interaction = obj.AddComponent<FlagInteraction>();
            Debug.Log($"[RuntimeAutoSetup] Attached FlagInteraction to {obj.name}");
        }

        // Add AudioSource if missing
        if (obj.GetComponent<AudioSource>() == null)
        {
            AudioSource audio = obj.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            Debug.Log($"[RuntimeAutoSetup] Attached AudioSource to {obj.name}");
        }

        // Assign Runner
        if (interaction.handLandmarkerRunner == null)
        {
            interaction.handLandmarkerRunner = runner;
            Debug.Log($"[RuntimeAutoSetup] Assigned Runner to {obj.name}");
        }
    }
}
