using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaypointsManager : MonoBehaviour
{
    [SerializeField]
    private bool m_UpdateList;

    private List<Transform> m_Waypoints = new List<Transform>();

    public List<Transform> Waypoints { get { return m_Waypoints; } }

    protected void Awake()
    {
        UpdateList();
    }
    
    protected void OnValidate()
    {
        if (m_UpdateList)
        {
            UpdateList();
            m_UpdateList = false;
        }
    }

    private void UpdateList()
    {
        m_Waypoints.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            m_Waypoints.Add(transform.GetChild(i).GetComponent<Transform>());
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