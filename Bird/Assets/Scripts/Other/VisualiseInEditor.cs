﻿using UnityEngine;

public class VisualiseInEditor : MonoBehaviour
{
    [SerializeField, Range(0.5f, 3f)]
    private float m_Radius;
    [SerializeField]
    private Color m_Color;

    protected void OnDrawGizmosSelected()
    {
        Gizmos.color = m_Color;
        Gizmos.DrawWireSphere(transform.position, m_Radius);
    }
}