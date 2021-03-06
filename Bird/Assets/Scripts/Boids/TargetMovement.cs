﻿using UnityEngine;
using System.Collections.Generic;

public class TargetMovement : MonoBehaviour
{
    [Header("Circle"), SerializeField, Range(0.01f, 1f)]
    private float m_Amplitude = 0.5f;
    [SerializeField, Range(0f, 0.04f)]
    private float m_RotationSpeed = 0.01f;
    private float m_Angle = 0;

    [Header("Avoidance"), SerializeField, Range(0, 20f)]
    private float m_DefaultHeight = 5;
    [SerializeField, Range(0.01f, 20f)]
    private float m_AvoidanceSpeed = 0.01f;
    [SerializeField, Range(0.01f, 20f)]
    private float m_AnotherSpeed;
    private bool m_Avoid;
    private bool m_IsAvoiding;

    private float m_TargetHeight;

    private enum Movement { None, Circle };
    private Movement m_CurrentMovement;

    public List<Vector3> PreviousPosition { get; set; }

    protected void Start()
    {
        m_CurrentMovement = Movement.Circle;
        m_TargetHeight = m_DefaultHeight;
    }

	protected void FixedUpdate()
    {
        UpdateCurrentBehaviour();
	}

    private void UpdateCurrentBehaviour()
    {
        switch (m_CurrentMovement)
        {
            case Movement.Circle:
                Circle();
                break;

            case Movement.None:
            default:
                break;
        }
    }

    private void Circle()
    {
        m_Angle += m_RotationSpeed;

        // Polar coordinates
        float x = m_Amplitude * Mathf.Sin(m_Angle) * Mathf.Rad2Deg;
        float z = m_Amplitude * Mathf.Cos(m_Angle) * Mathf.Rad2Deg;

        // TO DO: Remove the magic number for the Y value
        transform.position = new Vector3(x, m_TargetHeight, z);
    }

    protected void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}