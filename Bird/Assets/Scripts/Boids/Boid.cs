using UnityEngine;
using System.Collections.Generic;

public class Boid : BoidsParameters
{
    [SerializeField]
    private float m_RotationSpeed;

    // Movement modifiers
    private float m_MaxVelocity = 1f;
    // TO DO: Add MaxAvoidanceForceSOLID and MaxAvoidanceForceBOIDS // Separate both stuff
    private float m_MaxBoidsAvoidanceForce = 1f;
    private float m_MaxSolidsAvoidanceForce = 1f;
    private float m_AccelerationFactor = 1f;
    private float m_DecelerationFactor = 1f;  

    // Behavior modifiers
    private float m_ArriveFactor = 1f;
    private float m_BoidAvoidanceFactor = 1f;
    private float m_SolidAvoidanceFactor = 1f;
    private float m_MinimumDistanceToTarget = 1f;
    private float m_MinimumDistanceToOtherBoid = 1f;

    // Other
    private bool m_AvoidsSolid;
    private Solid m_SolidToAvoid;

    private Transform m_Target;
    private Vector3 m_Acceleration;
    private Vector3 m_CurrentVelocity;

    public Behaviour CurrentBehaviour { get; set; }
    public List<Boid> OtherBoids { get; set; }

    #region Modifiers
    public void SetMovementModifiers(float a_MaxVelocity, float a_MaxAvoidanceForce, float a_MaxSolidsAvoidanceForce, float a_AccelerationFactor, float a_DecelerationFactor)
    {
        m_MaxVelocity = a_MaxVelocity;
        m_MaxBoidsAvoidanceForce = a_MaxAvoidanceForce;
        m_MaxSolidsAvoidanceForce = a_MaxSolidsAvoidanceForce;
        m_AccelerationFactor = a_AccelerationFactor;
        m_DecelerationFactor = a_DecelerationFactor;
    }

    public void SetBehaviorModifers(float a_ArriveFactor, float a_BoidAvoidanceFactor, float a_SolidAvoidanceFactor, float a_MinimumDistanceToTarget, float a_MinimumDistanceToOtherBoid)
    {
        m_ArriveFactor = a_ArriveFactor;
        m_BoidAvoidanceFactor = a_BoidAvoidanceFactor;
        m_SolidAvoidanceFactor = a_SolidAvoidanceFactor;
        m_MinimumDistanceToTarget = a_MinimumDistanceToTarget;
        m_MinimumDistanceToOtherBoid = a_MinimumDistanceToOtherBoid;
    }
    #endregion Modifiers
	
	protected void FixedUpdate()
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
            UpdateVelocity();
        }

        switch(CurrentBehaviour)
        {
            case Behaviour.TakeOff:
                TakeOff();
                break;

            case Behaviour.Fly:
                UpdateAcceleration(Fly());
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

    #region BasedOnBehaviourEnum
    // Based on: private enum Behaviour { Idle, TakeOff, Fly, Land };
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
    #endregion BasedOnBehaviourEnum

    // Other behaviours
    private void RotateTowardsDirection()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(m_CurrentVelocity), m_RotationSpeed * Time.deltaTime);
    }

    #region AvoidanceBehaviours
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
            steer *= m_BoidAvoidanceFactor;
            Vector3.ClampMagnitude(steer, m_MaxBoidsAvoidanceForce);
            return steer;
        }

        return Vector3.zero;
    }

    private Vector3 AvoidSolid(Collision a_Solid)
    {
        Vector3 desiredVelocity = Vector3.zero;
        Vector3 closestPoint = a_Solid.contacts[0].point;

        Vector3 oppositeDirection = transform.position - closestPoint;

        float distanceToSolid = oppositeDirection.sqrMagnitude;

        oppositeDirection.Normalize();
        oppositeDirection /= distanceToSolid;
        desiredVelocity += oppositeDirection;

        Vector3 steer = desiredVelocity - m_CurrentVelocity;
        steer *= m_SolidAvoidanceFactor;
        Vector3.ClampMagnitude(steer, m_MaxBoidsAvoidanceForce);

        // TO DO: Remove ; Debug purposes only
        //Debug.DrawLine(transform.position, steer, Color.yellow);

        return steer;
    }

    private Vector3 DetermineSolidAvoidanceDirection(Collision a_Solid)
    {
        Vector3 desiredVelocity = Vector3.zero;

        Vector3 closestExitPoint = Vector3.zero;
        float newClosestExitPointX = 0;
        float newClosestExitPointY = 0;
        //float newClosestExitPointZ = 0;

        Vector3 closestCollisionPoint = a_Solid.contacts[0].point;

        // X desired velocity
        /*
        if (closestCollisionPoint.x <= a_Solid.collider.bounds.center.x)
        {
            newClosestExitPointX = a_Solid.collider.bounds.min.x;
        }
        else
        {
            newClosestExitPointX = a_Solid.collider.bounds.max.x;
        }
        */
        // Y desired velocity
        newClosestExitPointY = a_Solid.collider.bounds.max.x * 3;

        // Z desired velocity
        // To do: think about whether or not we actually do need that one
        /*
        if (closestCollisionPoint.z <= a_Solid.collider.bounds.center.z)
        {
            newClosestExitPointZ = a_Solid.collider.bounds.min.z;
        }
        else
        {
            newClosestExitPointZ = a_Solid.collider.bounds.max.z;
        }

    */
        desiredVelocity = new Vector3(newClosestExitPointX, newClosestExitPointY, 0);

        Vector3 steer = desiredVelocity - m_CurrentVelocity;
        steer *= m_SolidAvoidanceFactor;
        Vector3.ClampMagnitude(steer, m_MaxSolidsAvoidanceForce);

        // TO DO: Remove ; Debug purposes only
        Debug.DrawLine(transform.position, steer, Color.yellow);

        return steer;
    }

    protected void OnCollisionStay(Collision a_Solid)
    {
        Solid solid = a_Solid.collider.GetComponent<Solid>();

        if (solid is Solid)
        {
            //UpdateAcceleration(AvoidSolid(a_Solid));
            //DetermineSolidAvoidanceDirection(a_Solid);
        }
    }

    /*
    protected void OnCollisionExit(Collision a_Solid)
    {
        Solid solid = a_Solid.collider.GetComponent<Solid>();

        if (solid is Solid)
        {
            m_AvoidsSolid = false;
        }
    }
    */
    #endregion AvoidanceBehaviours
    #endregion Behaviours
}