﻿using UnityEngine;
using System.Collections.Generic;

public class WaypointsManager : MonoBehaviour
{
    [Header("Update"), SerializeField]
    private bool m_UpdateLists;

    [Header("Containers"), SerializeField]
    private Transform m_WaypointsContainer;
    [SerializeField]
    private Transform m_LandingSpotsContainer;
    
    private List<Transform> m_Waypoints = new List<Transform>();
    private List<Transform> m_LandingSpots = new List<Transform>();

    public List<Transform> Waypoints { get { return m_Waypoints; } }
    public List<Transform> LandingSpots { get { return m_LandingSpots; } }

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
            m_LandingSpots.Add(m_LandingSpotsContainer.GetChild(i).GetComponent<Transform>());
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