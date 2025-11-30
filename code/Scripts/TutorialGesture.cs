using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// TutorialGesture listens for gesture strings (matches the signature
/// OnGestureRecognized(string left, string right) used by GestureRecognizerRunner)
/// and detects when both hands show the configured gesture (default: "ThumbsUp")
/// for a configurable number of stable frames. When detected it invokes the
/// UnityEvent OnReady. Thread-safe: OnGestureRecognized can be called from
/// any thread.
/// </summary>
public class TutorialGesture : MonoBehaviour
{
    [Header("Gesture Settings")]
    [Tooltip("Name of the gesture to require on both hands (exact match).")]
    public string requiredGestureName = "Thumb_Up";

    [Tooltip("How many consecutive stable frames the required gesture must be held for.")]
    public int requiredStableFrames = 5;

    [Tooltip("If true, the OnReady event will only fire once.")]
    public bool triggerOnce = true;

    [Header("Optional Scene Load")]
    [Tooltip("If true, the specified scene will be loaded when OnReady fires.")]
    public bool loadSceneOnReady = false;

    [Tooltip("Scene name to load when ready (only used if loadSceneOnReady is true).")]
    public int sceneToLoad = 1;

    [Header("Events")]
    public UnityEvent OnReady;

    // Thread-safe queue where gesture callbacks enqueue recognized gestures
    private readonly ConcurrentQueue<(string left, string right)> _gestureQueue = new();

    // Pending / stable tracking
    private string _pendingLeft = string.Empty;
    private string _pendingRight = string.Empty;
    private string _stableLeft = string.Empty;
    private string _stableRight = string.Empty;
    private int _stableCounter = 0;

    private bool _hasTriggered = false;

    /// <summary>
    /// Public method expected to be invoked by GestureRecognizerRunner.OnGestureRecognized
    /// or other sources. This is thread-safe.
    /// </summary>
    public void OnGestureRecognized(string left, string right)
    {
        Debug.Log($"TutorialGesture OnGestureRecognized called with left: {left}, right: {right}");
        // Normalize nulls to empty strings
        left ??= string.Empty;
        right ??= string.Empty;

        _gestureQueue.Enqueue((left, right));
    }

    private void Update()
    {
        Debug.Log("TutorialGesture Update called");
        if (triggerOnce && _hasTriggered) return;

        // Process any pending queue items (thread-safe dequeue)
        while (_gestureQueue.TryDequeue(out var pair))
        {
            Debug.Log($"Gesture pair: left={pair.left}, right={pair.right}");

            // Ignore if either is empty or "None"
            if (string.IsNullOrEmpty(pair.left) || string.IsNullOrEmpty(pair.right))
            {
                // treat as break in stability
                _pendingLeft = pair.left;
                _pendingRight = pair.right;
                _stableCounter = 0;
                continue;
            }

            // If same as pending, increment stability counter
            if (pair.left == _pendingLeft && pair.right == _pendingRight)
            {
                _stableCounter++;
            }
            else
            {
                // New pending values
                _pendingLeft = pair.left;
                _pendingRight = pair.right;
                _stableCounter = 1;
            }

            // If we've reached required stable frames and it differs from last stable

            Debug.Log($"Stable gesture detected: left={pair.left}, right={pair.right}");
            Debug.Log($"loading scene: {loadSceneOnReady}, scene index: {sceneToLoad}");
            Debug.Log($"required stable frames: {requiredStableFrames}, current stable count: {_stableCounter}");
            Debug.Log($"required gesture: {requiredGestureName}, current left: {pair.left}, current right: {pair.right}");
            Debug.Log($"Comparing left: {string.Equals(pair.left, requiredGestureName, System.StringComparison.OrdinalIgnoreCase)}, right: {string.Equals(pair.right, requiredGestureName, System.StringComparison.OrdinalIgnoreCase)}");
             if (loadSceneOnReady && _stableCounter >= requiredStableFrames && string.Equals(pair.left, "Thumb_Up", System.StringComparison.OrdinalIgnoreCase)
                  && string.Equals(pair.right, "Thumb_Up", System.StringComparison.OrdinalIgnoreCase))
                {
                    // Load the scene (main thread safe)
                    SceneManager.LoadScene("GameScene");
                }
        }
    }

    /// <summary>
    /// Reset internal state so the recognition process can run again.
    /// </summary>
    public void ResetState()
    {
        _gestureQueue.Clear();
        _pendingLeft = _pendingRight = _stableLeft = _stableRight = string.Empty;
        _stableCounter = 0;
        _hasTriggered = false;
    }
}
