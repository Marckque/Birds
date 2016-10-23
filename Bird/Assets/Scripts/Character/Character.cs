using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Movement"), SerializeField, Range(0f, 10f)]
    private float m_Acceleration;
    [SerializeField, Range(0f, 10f)]
    private float m_Deceleration;
    [SerializeField, Range(0f, 100f)]
    private float m_MaxMovementSpeed;
    private float m_CurrentSpeed;
    private Vector3 m_MovementDirection;
    private Vector3 m_LastMovementDirection;

    [Header("Mouse"), SerializeField]
    private float m_MouseSpeedX;
    [SerializeField]
    private float m_MouseSpeedY;
    [SerializeField, Range(0f, 10f)]
    private float m_SmoothSpeed;

    private bool m_CursorIsLocked = true;
    private Quaternion m_TargetRotation;
    private Quaternion m_CameraTargetRotation;

    [Header("Camera"), SerializeField]
    private Transform m_CameraTransform;

    protected void Start()
    {
        InitialiseMouse();
        InitialiseTargetRotation();
    }

    private void InitialiseMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void InitialiseTargetRotation()
    {
        m_TargetRotation = transform.localRotation;
        m_CameraTargetRotation = m_CameraTransform.localRotation;
    }

    protected void Update()
    {
        // Movement
        GetKeyboardInput();
        Move();

        // Mouse
        GetMouseInput();
        LockUpdate();
	}

    private void GetKeyboardInput()
    {
        m_MovementDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
    }

    private void Move()
    {
        if (m_MovementDirection != Vector3.zero)
        {
            m_LastMovementDirection = m_MovementDirection;
            m_CurrentSpeed = Mathf.Lerp(0, m_MaxMovementSpeed, Time.deltaTime * m_Acceleration);
        }
        else
        {
            m_CurrentSpeed = Mathf.Lerp(m_CurrentSpeed, 0, Time.deltaTime * m_Deceleration);
        }

        transform.Translate(m_LastMovementDirection * m_CurrentSpeed * Time.deltaTime);
    }

    private void LockUpdate()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (m_CursorIsLocked)
            {
                m_CursorIsLocked = false;
            }
            else
            {
                m_CursorIsLocked = true;
            }
        }

        if (m_CursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void GetMouseInput()
    {
        float rotationY = Input.GetAxis("Mouse X") * m_MouseSpeedX;
        float rotationX = Input.GetAxis("Mouse Y") * m_MouseSpeedY;

        m_TargetRotation *= Quaternion.Euler(0f, rotationY, 0f);
        m_CameraTargetRotation *= Quaternion.Euler(-rotationX, 0f, 0f);

        m_CameraTransform.localRotation = ClampCameraRotation(m_CameraTransform.localRotation);

        transform.localRotation = Quaternion.Slerp(transform.localRotation, m_TargetRotation, Time.deltaTime * m_SmoothSpeed);
        m_CameraTransform.localRotation = Quaternion.Slerp(m_CameraTransform.localRotation, m_CameraTargetRotation, Time.deltaTime * m_SmoothSpeed);
    }

    private Quaternion ClampCameraRotation(Quaternion a_CameraRotation)
    {
        a_CameraRotation.x /= a_CameraRotation.w;
        a_CameraRotation.y /= a_CameraRotation.w;
        a_CameraRotation.z /= a_CameraRotation.w;
        a_CameraRotation.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(a_CameraRotation.x);
        angleX = Mathf.Clamp(angleX, -90, 90);
        a_CameraRotation.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return a_CameraRotation;
    }
}