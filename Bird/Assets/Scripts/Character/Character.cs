﻿using UnityEngine;

public class Character : MonoBehaviour
{
    private const float RIGIDBODY_MULTIPLIER = 500f;

    [Header("Components"), SerializeField]
    private Rigidbody m_Rigidbody;
    [SerializeField]
    private CapsuleCollider m_CapsuleCollider;
    [SerializeField, Range(0.01f, 2f)]
    private float m_GravityOffset = 0.2f;

    [Header("Movement"), SerializeField, Range(0f, 200f)]
    private float m_Acceleration;
    [SerializeField, Range(0f, 200f)]
    private float m_Deceleration;
    [SerializeField, Range(0f, 200f)]
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

            m_CurrentVelocity = Mathf.Lerp(0, m_MaxVelocity, m_Acceleration * Time.fixedDeltaTime);
        }
        else
        {
            m_CurrentVelocity = Mathf.Lerp(m_CurrentVelocity, 0, m_Deceleration * Time.fixedDeltaTime);
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
        Vector3 rayOrigin = new Vector3(transform.position.x, m_CapsuleCollider.bounds.min.y, transform.position.z);
        Ray ray = new Ray(rayOrigin, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (hit.distance > m_GravityOffset)
            {
                m_Rigidbody.useGravity = true;
            }
            else
            {
                m_Rigidbody.useGravity = false;
            }
        }
    }
}