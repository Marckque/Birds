using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField, Header("Movement")]
    private float m_MovementSpeed;
    private Vector3 m_MovementDirection;

    [SerializeField, Header("Mouse")]
    private float m_MouseSpeedX;
    [SerializeField]
    private float m_MouseSpeedY;

    [SerializeField, Header("Camera")]
    private Transform m_CameraTransform;

    protected void Start()
    {
        InitialiseMouse();
    }

    private void InitialiseMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    protected void Update()
    {
        // Movement
        GetKeyboardInput();
        Move();

        // Mouse
        GetMouseInput();
	}

    private void GetKeyboardInput()
    {
        m_MovementDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }

    private void Move()
    {
        if (m_MovementDirection != Vector3.zero)
        {
            transform.Translate(m_MovementDirection * m_MovementSpeed * Time.deltaTime);
        }
    }

    private void GetMouseInput()
    {
        float rotationY = Input.GetAxis("Mouse X") * m_MouseSpeedX;
        float rotationX = Input.GetAxis("Mouse Y") * m_MouseSpeedY;

        transform.localRotation *= Quaternion.Euler(0f, rotationY, 0f);
        m_CameraTransform.localRotation *= Quaternion.Euler(-rotationX, 0f, 0f);

        m_CameraTransform.localRotation = ClampCameraRotation(m_CameraTransform.localRotation);
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