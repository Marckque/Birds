using UnityEngine;
using System.Collections.Generic;

public class BoidsManager : BoidsParameters
{
    [Header("Boids parameters"), SerializeField, Range(0.001f, 10f)]
    private float m_MaxVelocity = 1f;
    [SerializeField, Range(0.001f, 10f)]
    private float m_MaxBoidsAvoidanceForce = 1f;
    [SerializeField, Range(0.001f, 10f)]
    private float m_MaxSolidsAvoidanceForce = 1f;
    [SerializeField, Range(0.001f, 10f)]
    private float m_AccelerationFactor = 1f;
    [SerializeField, Range(0.001f, 10f)]
    private float m_DecelerationFactor = 1f;

    // Behavior modifiers
    [SerializeField, Range(0.001f, 10f)]
    private float m_ArriveFactor = 1f;
    [SerializeField, Range(0.001f, 10f)]
    private float m_BoidAvoidanceFactor = 1f;
    [SerializeField, Range(0.001f, 10f)]
    private float m_SolidAvoidanceFactor = 1f;
    [SerializeField, Range(0.001f, 10f)]
    private float m_MinimumDistanceToTarget = 1f;
    [SerializeField, Range(0.001f, 10f)]
    private float m_MinimumDistanceToOtherBoid = 1f;

    [Header("BoidsManager parameters"), SerializeField]
    private Boid m_Boid;
    [SerializeField]
    private int m_NumberOfBoids;
    [SerializeField]
    private Transform m_InitialTarget;

    private List<Boid> m_Boids = new List<Boid>();

    #region Singleton
    private static BoidsManager s_instance = null;

    public static BoidsManager Instance
    {
        get { return s_instance; }
    }

    protected void Awake()
    {
        if (s_instance != null)
        {
            Debug.LogError(name + " shouldn't have a BoidsManager referenced already.");
        }
        else
        {
            s_instance = this;
        }
    }
    #endregion Singleton

    public List<Boid> Boids { get { return m_Boids; } }

    protected void Start()
    {
        InitialiseBoids();
	}
	
    private void InitialiseBoids()
    {
        CreateBoids();
        BoidListToIndividualBoids();
        SetBoidsTarget(m_InitialTarget);
        SetBoidsBehaviour(Behaviour.Fly);
        SetBoidsBehaviourModifiers();
        SetBoidsMovementModifiers();
    }

    private void CreateBoids()
    {
        for (int i = 0; i < m_NumberOfBoids; i++)
        {
            // TO DO: Find a better position to instantiate the boids!
            Boid boid = Instantiate(m_Boid, new Vector3(i * 5, 0, i * 5), Quaternion.identity) as Boid;
            m_Boids.Add(boid);
        }
    }

    // Set the "OtherBoids" list of every boids to m_Boids
	private void BoidListToIndividualBoids()
    {
        foreach (Boid boid in m_Boids)
        {
            boid.OtherBoids = m_Boids;
        }
    }

    private void SetBoidsBehaviour(Behaviour a_Behaviour)
    {
        foreach (Boid boid in m_Boids)
        {
            boid.SetBoidBehaviour(a_Behaviour);
        }
    }

    private void SetBoidsTarget(Transform a_Target)
    {
        foreach (Boid boid in m_Boids)
        {
            boid.SetTarget(a_Target);
        }
    }

    private void SetBoidsMovementModifiers()
    {
        foreach (Boid boid in m_Boids)
        {
            boid.SetMovementModifiers(m_MaxVelocity, m_MaxBoidsAvoidanceForce, m_MaxSolidsAvoidanceForce, m_AccelerationFactor, m_DecelerationFactor);
        }
    }

    private void SetBoidsBehaviourModifiers()
    {
        foreach (Boid boid in m_Boids)
        {
            boid.SetBehaviorModifers(m_ArriveFactor, m_BoidAvoidanceFactor, m_SolidAvoidanceFactor, m_MinimumDistanceToTarget, m_MinimumDistanceToOtherBoid);
        }
    }

    protected void OnValidate()
    {
        SetBoidsBehaviourModifiers();
        SetBoidsMovementModifiers();
    }
}