using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Boid : BoidsParameters
{
    #region Variables
    [SerializeField]
    private float m_RotationSpeed;
    [SerializeField]
    private float m_MinimumLandingDistance;
    [SerializeField]
    private float m_LandingTimer;
    [SerializeField]
    private float m_LandingChance;
    [SerializeField]
    private float m_LandingDuration;

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

    private List<Transform> m_Waypoint = new List<Transform>();
    private List<LandingSpot> m_LandingSpots = new List<LandingSpot>();
    private Transform m_CurrentTarget;
    private int m_CurrentLandingSpotIndex;
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
        StartCoroutine(CheckIfCanLand());
    }

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
        m_CurrentVelocity = Vector3.ClampMagnitude(m_CurrentVelocity, m_MaxVelocity);

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

    public void SetTargets(WaypointsManager a_WaypointsManager)
    {
        for (int i = 0; i < a_WaypointsManager.Waypoints.Count; i++)
        {
            m_Waypoint.Add(a_WaypointsManager.Waypoints[i]);
        }

        m_CurrentTarget = m_Waypoint[0];
        m_CurrentTargetIndex = 0;
    }

    public void SetLandingSpots(WaypointsManager a_WaypointsManager)
    {
        for (int i = 0; i < a_WaypointsManager.LandingSpots.Count; i++)
        {
            m_LandingSpots.Add(a_WaypointsManager.LandingSpots[i]);
        }
    }

    private IEnumerator CheckIfCanLand()
    {
        int chanceToLand = 0;

        while (chanceToLand < m_LandingChance)
        {
            yield return new WaitForSeconds(m_LandingTimer);
            chanceToLand = Random.Range(0, 100);
        }

        for (int i = 0; i < m_LandingSpots.Count; i++)
        {
            if (m_LandingSpots[i].LandedBoid == null)
            {
                m_LandingSpots[i].LandedBoid = this;
                CurrentBehaviour = Behaviour.Land;
                m_CurrentTarget = m_LandingSpots[i].transform;
                m_CurrentLandingSpotIndex = i;
                yield break;
            }
        }

        yield return null;
    }

    private IEnumerator BackToFlyState()
    {
        yield return new WaitForSeconds(m_LandingDuration);

        StartCoroutine(CheckIfCanLand());

        m_CurrentTarget = m_Waypoint[m_CurrentTargetIndex];

        CurrentBehaviour = Behaviour.Fly;

        m_LandingSpots[m_CurrentLandingSpotIndex].LandedBoid = null;
    }

    private void UpdateBehaviour()
    {
        // TO DO: Make sure it works with idle... Because I think it will not work.
        switch (CurrentBehaviour)
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
                UpdateAcceleration(Land());
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

    private void UpdateCurrentTargetIndex()
    {
        if (m_CurrentTargetIndex < m_Waypoint.Count - 1)
        {
            m_CurrentTargetIndex++;
        }
        else
        {
            m_CurrentTargetIndex = 0;
        }

        m_CurrentTarget = m_Waypoint[m_CurrentTargetIndex];
    }

    #region BasedOnBehaviourEnum
    // Based on: private enum Behaviour { Idle, TakeOff, Fly, Land };
    private void Idle()
    {
        StartCoroutine(BackToFlyState());
    }

    private void TakeOff()
    {

    }

    private Vector3 Fly()
    {
        Vector3 targetDirection = m_CurrentTarget.transform.position - transform.position;

        float distanceToTarget = targetDirection.sqrMagnitude;
        float maxSpeed = 0;

        maxSpeed = ExtensionMethods.Remap(m_MaxVelocity, 0, m_MaxVelocity, m_MaxVelocity, 0);

        if (distanceToTarget < m_MinimumDistanceToTarget)
        {
            UpdateCurrentTargetIndex();
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

    private Vector3 Land()
    {
        Vector3 targetDirection = m_CurrentTarget.transform.position - transform.position;
        targetDirection += Vector3.up * 0.25f;

        float distanceToTarget = targetDirection.sqrMagnitude;
        float maxSpeed = 0;

        maxSpeed = ExtensionMethods.Remap(m_MaxVelocity, 0, m_MaxVelocity, m_MaxVelocity, 0);

        if (distanceToTarget < m_MinimumLandingDistance)
        {
            CurrentBehaviour = Behaviour.Idle;
            return Vector3.zero;
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

    private void ResetSolidVelocityTime()
    {
        m_SolidVelocityTime = m_SolidVelocityTimer;
    }

    #endregion AvoidanceBehaviours
    #endregion Behaviours

    private void OnDrawGizmos()
    {
        if (m_CurrentTarget)
        {
            if (CurrentBehaviour == Behaviour.Fly)
            {
                Gizmos.color = Color.cyan;
            }
            else
            {
                Gizmos.color = Color.white;
            }

            Gizmos.DrawLine(transform.position, m_CurrentTarget.transform.position);
            Gizmos.DrawWireSphere(m_CurrentTarget.transform.position, 1f);
        }
    }
}