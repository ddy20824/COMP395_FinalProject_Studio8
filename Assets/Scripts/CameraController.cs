using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Rotation Limits")]
    public float minVerticalAngle = -20f;
    public float maxVerticalAngle = 80f;
    public float minHorizontalAngle = -45f;
    public float maxHorizontalAngle = 45f;

    public float lookSensitivity = 0.1f;

    [Header("Panning & Zoom")]
    public float panSpeed = 0.05f;
    public float zoomSpeed = 50f;

    [Header("Position Boundaries")]
    public Vector3 minBounds = new Vector3(-10, 0.5f, -10);
    public Vector3 maxBounds = new Vector3(10, 10, 10);

    [Header("Camera Focus")]
    [SerializeField]
    private CameraFocusEventChannel cameraFocusChannel;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isExternalControlled = false;
    private Coroutine moveCoroutine;

    // Save current rotation angles
    private float yaw;
    private float pitch;

    void Start()
    {
        // init angles based on current rotation
        Vector3 rot = transform.localRotation.eulerAngles;
        // turn 0~360 to -180~180 for easy Clamp calculation
        yaw = (rot.y > 180) ? rot.y - 360 : rot.y;
        pitch = (rot.x > 180) ? rot.x - 360 : rot.x;
    }

    private void OnEnable() => cameraFocusChannel.Subscribe(OnCameraFocus);
    private void OnDisable() => cameraFocusChannel.Unsubscribe(OnCameraFocus);

    // use LateUpdate to ensure it runs after all other Updates (e.g. after potential target movement)
    void LateUpdate()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;
        if (isExternalControlled) return; // skip manual control when externally controlled

        // 1. right button rotation
        if (mouse.rightButton.isPressed)
        {
            Vector2 delta = mouse.delta.ReadValue();

            yaw += delta.x * lookSensitivity;
            pitch -= delta.y * lookSensitivity;

            // limit rotation angles
            yaw = Mathf.Clamp(yaw, minHorizontalAngle, maxHorizontalAngle);
            pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

            transform.localRotation = Quaternion.Euler(pitch, yaw, 0);
        }

        // 2. middle button panning
        if (mouse.middleButton.isPressed)
        {
            Vector2 delta = mouse.delta.ReadValue();
            Vector3 move = new Vector3(-delta.x * panSpeed, -delta.y * panSpeed, 0);
            transform.Translate(move, Space.Self);
        }

        // 3. scroll wheel zooming
        float scroll = mouse.scroll.ReadValue().y;
        if (scroll != 0)
        {
            transform.Translate(0, 0, scroll * zoomSpeed * Time.deltaTime, Space.Self);
        }

        // 4. limit position within boundaries
        ApplyPositionBoundaries();
    }

    void ApplyPositionBoundaries()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        pos.y = Mathf.Clamp(pos.y, minBounds.y, maxBounds.y);
        pos.z = Mathf.Clamp(pos.z, minBounds.z, maxBounds.z);
        transform.position = pos;
    }

    private void OnCameraFocus(CameraFocusData data)
    {
        if (data.isFocusing)
        {
            // record original position and rotation before moving to target
            originalPosition = transform.position;
            originalRotation = transform.rotation;
            isExternalControlled = true;

            StartSmoothMove(data.targetTransform.position, data.targetTransform.rotation * new Quaternion(0, 180, 0, 1)); // fly to targetTransform and rotate to face it
        }
        else
        {
            // Turn back to original position and rotation
            StartSmoothMove(originalPosition, originalRotation);
            isExternalControlled = false;
        }
    }

    private void StartSmoothMove(Vector3 targetPos, Quaternion targetRot)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(SmoothMove(targetPos, targetRot));
    }

    private IEnumerator SmoothMove(Vector3 targetPos, Quaternion targetRot)
    {
        float duration = 1.0f; // how long the transition takes
        float elapsed = 0;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;
            // use smoothstep curve for smoother start and end (ease in/out)
            float curve = Mathf.SmoothStep(0, 1, percent);

            transform.position = Vector3.Lerp(startPos, targetPos, curve);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, curve);
            yield return null;
        }
    }
}