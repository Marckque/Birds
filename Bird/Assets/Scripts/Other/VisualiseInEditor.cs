using UnityEngine;

public class VisualiseInEditor : MonoBehaviour
{
    [SerializeField, Range(0.5f, 3f)]
    private float m_Radius;

    protected void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, m_Radius);
    }
}