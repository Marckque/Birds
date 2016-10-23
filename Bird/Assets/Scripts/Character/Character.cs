using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Rigidbody"), SerializeField]
    private Rigidbody m_Rigidbody;
    [SerializeField, Range(0.2f, 3f)]
    private float m_GravityOffset = 0.2f;

    private const float RIGIDBODY_MULTIPLIER = 500f;

    [Header("Movement"), SerializeField, Range(0f, 100f)]
    private float m_Acceleration;
    [SerializeField, Range(0f, 100f)]
    private float m_Deceleration;
    [SerializeField, Range(0f, 100f)]
    private float m_MaxVelocity;
    private float m_CurrentVelocity;
    private Vector3 m_MovementDirection;
    private Vector3 m_LastMovementDirection;

    protected void Update()
    {
        GetKeyboardInput();
	}

    protected void FixedUpdate()
    {
        ApplyGravity();
        CalculateVelocity();
        Move();
    }

    private void GetKeyboardInput()
    {
        m_MovementDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
    }

    private void CalculateVelocity()
    {
        if (m_MovementDirection != Vector3.zero)
        {
            m_LastMovementDirection = m_MovementDirection;

            m_CurrentVelocity = Mathf.Lerp(0, m_MaxVelocity, m_Acceleration * Time.fixedDeltaTime * RIGIDBODY_MULTIPLIER);
        }
        else
        {
            m_CurrentVelocity = Mathf.Lerp(m_CurrentVelocity, 0, m_Deceleration * Time.fixedDeltaTime * RIGIDBODY_MULTIPLIER);
        }
    }

    private void Move()
    {
        Vector3 newForce = m_LastMovementDirection * m_CurrentVelocity * Time.fixedDeltaTime * RIGIDBODY_MULTIPLIER;
        newForce = Vector3.ClampMagnitude(newForce, m_MaxVelocity);

        m_Rigidbody.AddRelativeForce(newForce);
    }

    private void ApplyGravity()
    {
        Ray ray = new Ray(GetComponent<CapsuleCollider>().bounds.min, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (hit.distance > m_GravityOffset)
            {
                m_Rigidbody.useGravity = true;
            }
        }
    }

    protected void OnCollisionEnter(Collision a_Other)
    {
        Solid solid = a_Other.collider.GetComponent<Solid>();

        if (solid is Solid)
        {
            m_Rigidbody.useGravity = false;
        }
    }
}