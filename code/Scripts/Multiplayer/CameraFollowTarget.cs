using UnityEngine;

public class CameraFollowTarget : MonoBehaviour
{
    [Header("Smoothing")]
    public Transform root;                  // drag the player root here
    public float smoothTime = 0.02f;        // lower = snappier, higher = floatier
    public float rotationSmoothTime = 0.05f;

    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        // Detach from the player hierarchy at runtime
        // so the transform hierarchy stops dragging us along
        transform.SetParent(null);
    }

    void LateUpdate()
    {
        transform.position = Vector3.SmoothDamp(
            transform.position,
            root.position,
            ref velocity,
            smoothTime
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            root.rotation,
            1f - Mathf.Exp(-rotationSmoothTime * 60f * Time.deltaTime)
        );
    }
}