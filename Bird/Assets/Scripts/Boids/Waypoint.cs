using UnityEngine;
using System.Collections.Generic;

namespace Birdies
{
    public class Waypoint : MonoBehaviour
    {
        [SerializeField]
        private float m_Speed = 5f;
        [SerializeField]
        private Transform m_WaypointsContainer;

        private int m_WaypointIndex = 0;
        private List<Transform> m_Waypoints = new List<Transform>();

        private Transform CurrentWaypoint
        {
            get
            {
                return m_Waypoints[m_WaypointIndex];
            }
        }

        protected void Start()
        {
            GetWaypoints();
            transform.position = m_Waypoints[0].transform.position;
        }

        protected void Update()
        {
            float distance = (transform.position - CurrentWaypoint.position).magnitude;
            distance -= Time.deltaTime * m_Speed;

            while (distance <= 0)
            {
                transform.position = CurrentWaypoint.position;

                m_WaypointIndex = (m_WaypointIndex + 1) % m_Waypoints.Count;
                distance += (transform.position - CurrentWaypoint.position).magnitude;
            }

            transform.position = CurrentWaypoint.position + Vector3.Normalize(transform.position - CurrentWaypoint.position) * distance;
        }

        protected void GetWaypoints()
        {
            m_Waypoints.Clear();

            for (int i = 0; i < m_WaypointsContainer.childCount; i++)
            {
                m_Waypoints.Add(m_WaypointsContainer.GetChild(i));
            }
        }

        protected void OnDrawGizmos()
        {
            if (m_Waypoints.Count > 0)
            {
                for (int i = 0; i < m_Waypoints.Count; i++)
                {
                    if (i == m_Waypoints.Count - 1)
                    {
                        Gizmos.DrawLine(m_Waypoints[m_Waypoints.Count - 1].transform.position, m_Waypoints[0].transform.position);
                    }
                    else
                    {
                        Gizmos.DrawLine(m_Waypoints[i].transform.position, m_Waypoints[i + 1].transform.position);
                    }
                }
            }
        }
    }
}