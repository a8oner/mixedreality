using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity.Sample.HandLandmarkDetection;

public class FlagInteraction : MonoBehaviour
{
    [Header("References")]
    public HandLandmarkerRunner handLandmarkerRunner;
    public AudioSource audioSource;

    [Header("Settings")]
    public float touchThreshold = 0.1f; // Increased threshold for easier testing
    public float touchCooldown = 1.0f;

    // Debug vars
    private HandLandmarkerResult? _latestResult;
    private object _lockObj = new object();
    private bool _hasNewResult = false;
    private float _lastTouchTime;

    // GUI Debug
    private string _debugStatus = "Waiting...";
    private float _lastDist = float.MaxValue;
    private Vector2 _lastFingerPos;

    void Start()
    {
        Debug.Log("[FlagInteraction] Script Started on: " + gameObject.name);

        if (handLandmarkerRunner == null)
        {
            handLandmarkerRunner = FindObjectOfType<HandLandmarkerRunner>();
        }

        if (handLandmarkerRunner != null)
        {
            Debug.Log("[FlagInteraction] HandLandmarkerRunner found. Subscribing...");
            handLandmarkerRunner.OnLandmarksUpdated += OnLandmarksUpdated;
        }
        else
        {
            Debug.LogError("[FlagInteraction] CRITICAL: HandLandmarkerRunner NOT FOUND in scene!");
            _debugStatus = "Runner Not Found!";
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (audioSource.clip == null)
        {
            audioSource.clip = CreateToneAudioClip(440f, 0.1f);
            Debug.Log("[FlagInteraction] Generated procedural audio clip.");
        }
    }

    void OnDestroy()
    {
        if (handLandmarkerRunner != null)
        {
            handLandmarkerRunner.OnLandmarksUpdated -= OnLandmarksUpdated;
        }
    }

    private void OnLandmarksUpdated(HandLandmarkerResult result)
    {
        lock (_lockObj)
        {
            _latestResult = result;
            _hasNewResult = true;
        }
    }

    void Update()
    {
        HandLandmarkerResult? currentResult = null;
        lock (_lockObj)
        {
            if (_hasNewResult)
            {
                currentResult = _latestResult;
                _hasNewResult = false;
            }
        }

        if (currentResult.HasValue)
        {
            ProcessHands(currentResult.Value);
        }
    }

    private void ProcessHands(HandLandmarkerResult result)
    {
        if (result.handLandmarks == null || result.handLandmarks.Count == 0)
        {
            _debugStatus = "No Hands Detected";
            return;
        }

        if (Camera.main == null)
        {
            _debugStatus = "No Main Camera!";
            return;
        }

        // Check first hand
        var landmarks = result.handLandmarks[0];
        if (landmarks.landmarks == null || landmarks.landmarks.Count < 9) return;

        var indexTip = landmarks.landmarks[8]; // Index Finger Tip
        
        // World -> Viewport (0..1)
        Vector3 flagVP = Camera.main.WorldToViewportPoint(transform.position);

        // MediaPipe Y is usually inverted relative to Unity Viewport?
        // Let's calculate BOTH normal and inverted distances to be safe.
        // MP Top-Left (0,0) -> Bottom-Right (1,1)
        // Unity Viewport Bottom-Left (0,0) -> Top-Right (1,1)
        
        // Finger Pos in Viewport coords (assuming MP y is inverted)
        float fingerY_Inverted = 1.0f - indexTip.y;
        Vector2 fingerPosInv = new Vector2(indexTip.x, fingerY_Inverted);

        // Raw Finger Pos
        Vector2 fingerPosRaw = new Vector2(indexTip.x, indexTip.y);

        // Distance
        float distInv = Vector2.Distance(fingerPosInv, new Vector2(flagVP.x, flagVP.y));
        float distRaw = Vector2.Distance(fingerPosRaw, new Vector2(flagVP.x, flagVP.y));

        // Use the smaller one to be generous
        float minDist = Mathf.Min(distInv, distRaw);
        
        _lastDist = minDist;
        _lastFingerPos = fingerPosInv; // for GUI
        _debugStatus = $"Tracking... Dist: {minDist:F3}";

        if (minDist < touchThreshold)
        {
            if (Time.time - _lastTouchTime > touchCooldown)
            {
                PlaySound();
            }
        }
    }

    private void PlaySound()
    {
        _lastTouchTime = Time.time;
        if (audioSource != null)
        {
            audioSource.Play();
            Debug.Log("[FlagInteraction] PLAYING SOUND!");
            _debugStatus = "TOUCHED! Sound Playing.";
        }
    }



    private AudioClip CreateToneAudioClip(float frequency, float duration)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / sampleRate);
        }
        AudioClip clip = AudioClip.Create("ProceduralTone", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
