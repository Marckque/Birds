using UnityEngine;
using System.Collections.Generic;

public class Boid : BoidsParameters
{
    #region Variables
    [SerializeField]
    private float m_RotationSpeed;

    public float m_SolidVelocityTimer;
    public bool m_IsAvoiding;
    private Vector3 m_SolidVelocity;
    private float m_SolidVelocityTime;

    // Movement modifiers
    private float m_MaxVelocity = 1f;
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

    private List<Transform> m_Targets = new List<Transform>();
    private Transform m_CurrentTarget;
    private int m_CurrentTargetIndex;
    private Vector3 m_Acceleration;
    private Vector3 m_CurrentVelocity;

    public Behaviour CurrentBehaviour { get; set; }
    public List<Boid> OtherBoids { get; set; }
    #endregion Variables

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
	
    protected void Awake()
    {
        ResetSolidVelocityTime();
    }

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
        m_CurrentVelocity = Vector3.ClampMagnitude(m_CurrentVelocity, m_MaxVelocity);

        transform.position += m_CurrentVelocity;

        //m_Acceleration = Vector3.zero;
    }
    #endregion VelocityCalculation

    #region Behaviours
    // Management
    public void SetBoidBehaviour(Behaviour a_Behaviour)
    {
        CurrentBehaviour = a_Behaviour;
    }

    public void SetTargets(WaypointsManager a_WaypointsManager)
    {
        for (int i = 0; i < a_WaypointsManager.Waypoints.Count; i++)
        {
            m_Targets.Add(a_WaypointsManager.Waypoints[i]);
        }

        m_CurrentTarget = m_Targets[0];
        m_CurrentTargetIndex = 0;
    }

    private void ResetSolidVelocityTime()
    {
        m_SolidVelocityTime = m_SolidVelocityTimer;
    }

    private void UpdateBehaviour()
    {
        // TO DO: Make sure it works with idle... Because I think it will not work.
        switch(CurrentBehaviour)
        {
            case Behaviour.TakeOff:
                TakeOff();
                break;

            case Behaviour.Fly:
                UpdateAcceleration(AvoidOtherBoids());

                if (m_IsAvoiding)
                {
                    UpdateAcceleration(m_SolidVelocity);

                    if (m_SolidVelocityTime > 0f)
                    {
                        m_SolidVelocityTime -= Time.deltaTime;
                    }
                    else
                    {
                        m_IsAvoiding = false;
                        ResetSolidVelocityTime();
                    }
                }

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

        if (CurrentBehaviour != Behaviour.Idle)
        {
            RotateTowardsDirection();
            UpdateVelocity();
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
        Vector3 targetDirection = m_CurrentTarget.transform.position - transform.position;

        float distanceToTarget = targetDirection.sqrMagnitude;
        float maxSpeed = 0;

        if (distanceToTarget < m_MinimumDistanceToTarget)
        {
            maxSpeed = ExtensionMethods.Remap(m_MaxVelocity, 0, m_MaxVelocity, m_MaxVelocity, 0);

            if (m_CurrentTargetIndex < m_Targets.Count - 1)
            {
                m_CurrentTargetIndex++;
            }
            else
            {
                m_CurrentTargetIndex = 0;
            }

            m_CurrentTarget = m_Targets[m_CurrentTargetIndex];
        }
        else
        {
            maxSpeed = m_MaxVelocity;
        }

        targetDirection.Normalize();
        targetDirection *= maxSpeed;

        Vector3 steer = targetDirection - m_CurrentVelocity;
        steer *= m_ArriveFactor;
        steer = Vector3.ClampMagnitude(steer, m_MaxVelocity);
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
            steer = Vector3.ClampMagnitude(steer, m_MaxBoidsAvoidanceForce);
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
        steer = Vector3.ClampMagnitude(steer, m_MaxBoidsAvoidanceForce);

        // TO DO: Remove ; Debug purposes only
        //Debug.DrawLine(transform.position, steer, Color.yellow);

        return steer;
    }

    private Vector3 DetermineSolidAvoidanceDirection(Collision a_Collision)
    {
        Vector3 steer = Vector3.zero;
        ContactPoint contactPoint = a_Collision.contacts[0];
        Vector3 collisionNormal = contactPoint.normal;

        float angle = Vector3.Angle(collisionNormal, m_CurrentVelocity);
        float distanceToCollision = (transform.position - a_Collision.contacts[0].point).sqrMagnitude;
        float multiplier = 0.2f + (distanceToCollision * 0.01f);

        Debug.DrawRay(contactPoint.point, collisionNormal, Color.blue, 5f); // BLUE = NORMAL

        steer = (collisionNormal + m_CurrentVelocity).normalized;

        if (angle <= 30f)
        {
            steer += Vector3.right;
        }
        else
        {
            steer += Vector3.left;
        }

        if (transform.position.y * 1.25f > contactPoint.point.y)
        {
            steer += Vector3.up;
        }
      
        steer *= multiplier;
        steer = Vector3.ClampMagnitude(steer, m_MaxSolidsAvoidanceForce);
        Debug.DrawRay(transform.position, steer, Color.red, 5.0f); // RED = STEERING

        m_IsAvoiding = true;

        return steer;
    }

    protected void OnCollisionStay(Collision a_Collision)
    {
        if (!m_IsAvoiding)
        {
            Solid solid = a_Collision.gameObject.GetComponent<Solid>();

            if (solid)
            {
                m_SolidVelocity = DetermineSolidAvoidanceDirection(a_Collision);
            }
        }
    }

    #endregion AvoidanceBehaviours
    #endregion Behaviours

    private void OnDrawGizmos()
    {
        if (m_CurrentTarget)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, m_CurrentTarget.transform.position);
            Gizmos.DrawWireSphere(m_CurrentTarget.transform.position, 1f);
        }
    }
}