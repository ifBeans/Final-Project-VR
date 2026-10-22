using UnityEngine;

public class UIBillboard : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Camera to face. If null, will automatically find Camera.main or active camera.")]
    public Transform targetCamera;

    [Header("Settings")]
    [Tooltip("Keep vertical upright rotation only (yaw only). Recommended for speech bubbles.")]
    public bool lockVerticalPitch = true;

    [Tooltip("Smoothly rotate towards player.")]
    public bool smooth = true;

    [Tooltip("Rotation smoothing speed.")]
    public float smoothSpeed = 10f;

    [Tooltip("Invert facing direction if canvas appears backwards.")]
    public bool reverseFace = false;

    private void Start()
    {
        FindCamera();
    }

    private void FindCamera()
    {
        if (targetCamera == null)
        {
            if (Camera.main != null)
            {
                targetCamera = Camera.main.transform;
            }
            else
            {
                Camera cam = FindFirstObjectByType<Camera>();
                if (cam != null)
                {
                    targetCamera = cam.transform;
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            FindCamera();
            if (targetCamera == null) return;
        }

        Vector3 forwardToCam = targetCamera.position - transform.position;

        if (lockVerticalPitch)
        {
            forwardToCam.y = 0;
        }

        if (forwardToCam.sqrMagnitude < 0.001f)
            return;

        // In Unity WorldSpace Canvas, pointing transform.forward towards the camera ensures the front faces the viewer
        Vector3 lookDir = reverseFace ? -forwardToCam : forwardToCam;
        Quaternion targetRotation = Quaternion.LookRotation(lookDir, Vector3.up);

        if (smooth)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);
        }
        else
        {
            transform.rotation = targetRotation;
        }
    }
}
