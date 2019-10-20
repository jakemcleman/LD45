using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    class CameraState
    {
        public float yaw;
        public float pitch;
        public float roll;
        public float x;
        public float y;
        public float z;

        public void SetFromTransform(Transform t)
        {
            pitch = t.eulerAngles.x;
            yaw = t.eulerAngles.y;
            roll = t.eulerAngles.z;
            x = t.position.x;
            y = t.position.y;
            z = t.position.z;
        }

        public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
        {
            yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
            pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
            roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);
                
            x = Mathf.Lerp(x, target.x, positionLerpPct);
            y = Mathf.Lerp(y, target.y, positionLerpPct);
            z = Mathf.Lerp(z, target.z, positionLerpPct);
        }

        public void UpdateTransform(Transform t)
        {
            t.eulerAngles = new Vector3(pitch, yaw, roll);
        }
    }
        
    CameraState m_TargetCameraState = new CameraState();
    CameraState m_InterpolatingCameraState = new CameraState();

    [Header("Movement Settings")]
    [Tooltip("Exponential boost factor on translation, controllable by mouse wheel.")]
    public float boost = 3.5f;

    [Tooltip("Time it takes to interpolate camera position 99% of the way to the target."), Range(0.001f, 1f)]
    public float positionLerpTime = 0.2f;

    [Header("Rotation Settings")]
    [Tooltip("X = Change in mouse position.\nY = Multiplicative factor for camera rotation.")]
    public AnimationCurve mouseAccelerationCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f)); 

    [Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target."), Range(0.001f, 1f)]
    public float rotationLerpTime = 0.01f;

    [Tooltip("Whether or not to invert our Y axis for mouse input to rotation.")]
    public bool invertY = false;

    private static bool mouseAccel = true;
    private static float mouseSens = 1;

    public float quickTurnTime = 0.25f;
    private float quickTurnTimer;
    private float targetYaw, originalYaw;

    public float maxPitchDegrees = 80;
    public float minPitchDegrees = -80;

    public static void SetMouseAccel (bool val)
    {
        mouseAccel = val;
        PlayerPrefs.SetInt("MouseAcceleration", val ? 1 : 0);
    }

    public static void SetMouseSens (float val)
    {
        mouseSens = val;
        PlayerPrefs.SetFloat("MouseSensitivity", val);
    }

    void OnEnable()
    {
        // Hide and lock cursor
        m_TargetCameraState.SetFromTransform(transform);
        m_InterpolatingCameraState.SetFromTransform(transform);
        Cursor.lockState = CursorLockMode.Locked;
        quickTurnTimer = 0;
    }

    private void OnDisable()
    {
        // Unlock and show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // Rotation
        if (InQuickTurn())
        {
            QuickTurnUpdate();
        }
        else
        {
            Vector2 mouseMovement = new Vector2(Input.GetAxis("Mouse X") * mouseSens, Input.GetAxis("Mouse Y") * mouseSens * (invertY ? 1 : -1));

            float mouseAccelerationFactor = 1;

            if (mouseAccel)
            {
                mouseAccelerationFactor = mouseAccelerationCurve.Evaluate(mouseMovement.magnitude);
            }

            m_TargetCameraState.yaw += mouseMovement.x * mouseAccelerationFactor;
            m_TargetCameraState.pitch += mouseMovement.y * mouseAccelerationFactor;
            m_TargetCameraState.pitch = Mathf.Clamp(m_TargetCameraState.pitch, minPitchDegrees, maxPitchDegrees);

            // Framerate-independent interpolation
            // Calculate the lerp amount, such that we get 99% of the way to our target in the specified time
            float positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
            float rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);
            m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);
        }

        m_InterpolatingCameraState.UpdateTransform(transform);
    }

    public void StartQuickTurn(Vector3 dirToWall)
    {
        quickTurnTimer = quickTurnTime;
        float y = transform.forward.y;
        originalYaw = m_TargetCameraState.yaw;
        if (!Utility.Close(dirToWall, Vector3.zero))
        {
            targetYaw = originalYaw + Vector3.SignedAngle(transform.forward, -dirToWall, Vector3.up);
        }
        else
        {
            targetYaw = originalYaw + 180;
        }
    }

    private void QuickTurnUpdate()
    {
        quickTurnTimer -= Time.deltaTime;
        float t = 1 - (quickTurnTimer / quickTurnTime);
    
        m_TargetCameraState.yaw = Mathf.Lerp(originalYaw, targetYaw, t);
        m_InterpolatingCameraState.yaw = Mathf.Lerp(originalYaw, targetYaw, t);
    }

    public bool InQuickTurn()
    {
        return quickTurnTimer > 0;
    }
}
