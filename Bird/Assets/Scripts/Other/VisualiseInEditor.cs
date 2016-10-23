using UnityEngine;

public class VisualiseInEditor : MonoBehaviour
{
    [SerializeField, Range(0.5f, 3f)]
    private float m_Radius;

    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, m_Radius);
    }
}