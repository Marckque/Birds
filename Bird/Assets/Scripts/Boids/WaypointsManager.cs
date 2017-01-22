using UnityEngine;
using System.Collections.Generic;

public class WaypointsManager : MonoBehaviour
{
    [Header("Update"), SerializeField]
    private bool m_UpdateLists;

    [Header("Containers"), SerializeField]
    private Transform m_WaypointsContainer;
    [SerializeField]
    private Transform m_LandingSpotsContainer;

    [Header("Debug"), SerializeField]
    private bool m_HighlightAllWaypoints;
    
    private List<Transform> m_Waypoints = new List<Transform>();
    private List<LandingSpot> m_LandingSpots = new List<LandingSpot>();

    public List<Transform> Waypoints { get { return m_Waypoints; } }
    public List<LandingSpot> LandingSpots { get { return m_LandingSpots; } }

    protected void Awake()
    {
        UpdateAllLists();
    }
    
    protected void OnValidate()
    {
        if (m_UpdateLists)
        {
            UpdateAllLists();
            m_UpdateLists = false;
        }
    }
    
    private void UpdateAllLists()
    {
        UpdateWaypointsList();
        UpdateLandingSpotsList();
    }

    private void UpdateWaypointsList()
    {
        m_Waypoints.Clear();

        for (int i = 0; i < m_WaypointsContainer.childCount; i++)
        {
            m_Waypoints.Add(m_WaypointsContainer.GetChild(i).GetComponent<Transform>());
        }
    }

    private void UpdateLandingSpotsList()
    {
        m_LandingSpots.Clear();

        for (int i = 0; i < m_LandingSpotsContainer.childCount; i++)
        {
            m_LandingSpots.Add(m_LandingSpotsContainer.GetChild(i).GetComponent<LandingSpot>());
        }
    }

    protected void OnDrawGizmos()
    {
        if (m_HighlightAllWaypoints)
        {
            Gizmos.color = Color.green;

            foreach (Transform t in Waypoints)
            {
                Gizmos.DrawWireSphere(t.transform.position, 1f);
            }
        }
    }

    protected void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        foreach (Transform t in Waypoints)
        {
            Gizmos.DrawWireSphere(t.transform.position, 1f);
        }
    }
}