using DG.Tweening;
using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    [SerializeField] private Transform camTransform;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeStrength = 0.4f;

    private Tween currentShake;

    public void Shake()
    {
        // Kill any running shake first
        currentShake?.Kill();

        // "DOShakePosition" = random offset-based shake animation
        currentShake = camTransform.DOShakePosition(
            duration: shakeDuration,
            strength: shakeStrength,
            vibrato: 10,
            randomness: 90,
            fadeOut: true
        );
    }
}
