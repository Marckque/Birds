using UnityEngine;
using System.Collections.Generic;

public class Boid : BoidsParameters
{
    [SerializeField]
    private float m_RotationSpeed;

    // Movement modifiers
    private float m_MaxVelocity = 1f;
    private float m_MaxAvoidanceForce = 1f;
    private float m_AccelerationFactor = 1f;
    private float m_DecelerationFactor = 1f;  

    // Behavior modifiers
    private float m_ArriveFactor = 1f;
    private float m_AvoidanceFactor = 1f;
    //private float m_SolidAvoidanceFactor = 1f;
    private float m_MinimumDistanceToTarget = 1f;
    private float m_MinimumDistanceToOtherBoid = 1f;

    // Other
    private Transform m_Target;
    private Vector3 m_Acceleration;
    private Vector3 m_CurrentVelocity;

    public Behaviour CurrentBehaviour { get; set; }
    public List<Boid> OtherBoids { get; set; }

    #region Modifiers
    public void SetMovementModifiers(float a_MaxVelocity, float a_MaxAvoidanceForce, float a_AccelerationFactor, float a_DecelerationFactor)
    {
        m_MaxVelocity = a_MaxVelocity;
        m_MaxAvoidanceForce = a_MaxAvoidanceForce;
        m_AccelerationFactor = a_AccelerationFactor;
        m_DecelerationFactor = a_DecelerationFactor;
    }

    public void SetBehaviorModifers(float a_ArriveFactor, float a_AvoidanceFactor, float a_MinimumDistanceToTarget, float a_MinimumDistanceToOtherBoid)
    {
        m_ArriveFactor = a_ArriveFactor;
        m_AvoidanceFactor = a_AvoidanceFactor;
        m_MinimumDistanceToTarget = a_MinimumDistanceToTarget;
        m_MinimumDistanceToOtherBoid = a_MinimumDistanceToOtherBoid;
    }
    #endregion Modifiers
	
	protected void Update()
    {
        UpdateBehaviour();
	}

    #region VelocityCalculation
    private void UpdateAcceleration(Vector3 a_Force)
    {
        m_Acceleration += a_Force * m_AccelerationFactor * Time.deltaTime;
    }

    private void UpdateVelocity()
    {
        m_CurrentVelocity += m_Acceleration;
        Vector3.ClampMagnitude(m_CurrentVelocity, m_MaxVelocity);
        transform.position += m_CurrentVelocity;

        m_Acceleration = Vector3.zero;
    }
    #endregion VelocityCalculation

    #region Behaviours
    // Management
    public void SetBoidBehaviour(Behaviour a_Behaviour)
    {
        CurrentBehaviour = a_Behaviour;
    }

    public void SetTarget(Transform a_Target)
    {
        m_Target = a_Target;
    }

    private void UpdateBehaviour()
    {
        // TO DO: Make sure it works with idle... Because I think it will not work.
        if (CurrentBehaviour != Behaviour.Idle)
        {
            UpdateAcceleration(AvoidOtherBoids());
            RotateTowardsDirection();
        }

        switch(CurrentBehaviour)
        {
            case Behaviour.TakeOff:
                TakeOff();
                break;

            case Behaviour.Fly:
                UpdateAcceleration(Fly());
                UpdateVelocity();
                break;

            case Behaviour.Land:
                Land();
                break;

            case Behaviour.Idle:
            default:
                Idle();
                break;
        }
    }

    // Based on: private enum CurrentBehaviour { Idle, TakeOff, Fly, Land };
    private void Idle()
    {
        // Stand still
    }

    private void TakeOff()
    {

    }

    private Vector3 Fly()
    {
        Vector3 targetDirection = m_Target.transform.position - transform.position;

        float distanceToTarget = targetDirection.sqrMagnitude;
        float maxSpeed = 0;

        if (distanceToTarget < m_MinimumDistanceToTarget)
        {
            maxSpeed = ExtensionMethods.Remap(m_MaxVelocity, 0, m_MaxVelocity, m_MaxVelocity, 0);
        }
        else
        {
            maxSpeed = m_MaxVelocity;
        }

        targetDirection.Normalize();
        targetDirection *= maxSpeed;

        Vector3 steer = targetDirection - m_CurrentVelocity;
        steer *= m_ArriveFactor;
        Vector3.ClampMagnitude(steer, m_MaxVelocity);
        return steer;
    }

    private void Land()
    {

    }

    // Other behaviours
    private void RotateTowardsDirection()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(m_CurrentVelocity), m_RotationSpeed * Time.deltaTime);
    }

    private Vector3 AvoidOtherBoids()
    {
        int numberOfCloseBoids = 0;
        Vector3 desiredVelocity = Vector3.zero;

        foreach (Boid otherBoid in OtherBoids)
        {
            Vector3 oppositeDirection = transform.position - otherBoid.transform.position;
            float distanceToOtherBoids = oppositeDirection.sqrMagnitude;

            if (distanceToOtherBoids > 0 && distanceToOtherBoids < m_MinimumDistanceToOtherBoid)
            {
                numberOfCloseBoids++;

                oppositeDirection.Normalize();
                oppositeDirection /= distanceToOtherBoids;
                desiredVelocity += oppositeDirection;
            }
        }

        if (numberOfCloseBoids > 0)
        {
            desiredVelocity /= numberOfCloseBoids;

            Vector3 steer = desiredVelocity - m_CurrentVelocity;
            steer *= m_AvoidanceFactor;
            Vector3.ClampMagnitude(steer, m_MaxAvoidanceForce);
            return steer;
        }

        return Vector3.zero;
    }

    private Vector3 AvoidSolid(Collision a_Solid)
    {
        print("I avoid the solid.");

        Vector3 desiredVelocity = Vector3.zero;
        Vector3 closestPoint = a_Solid.contacts[0].point;

        Debug.DrawLine(transform.position, closestPoint, Color.red);

        Vector3 oppositeDirection = transform.position - closestPoint;

        float distanceToSolid = oppositeDirection.sqrMagnitude;

        oppositeDirection.Normalize();
        oppositeDirection /= distanceToSolid;
        desiredVelocity += oppositeDirection;

        Vector3 steer = desiredVelocity - m_CurrentVelocity;
        steer *= m_AvoidanceFactor;
        Vector3.ClampMagnitude(steer, m_MaxAvoidanceForce);
        return steer;
    }

    protected void OnCollisionEnter(Collision a_Solid)
    {
        print("OnCollisionEnter");
        Solid solid = a_Solid.collider.GetComponent<Solid>();

        if (solid is Solid)
        {
            //print("I am a solid.");
            AvoidSolid(a_Solid);
        }
    }
    #endregion Behaviours
}