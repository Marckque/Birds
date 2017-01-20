using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaypointsManager : MonoBehaviour
{
    [SerializeField]
    private List<Transform> m_Waypoints = new List<Transform>();

    public List<Transform> Waypoints { get { return m_Waypoints; } }
}