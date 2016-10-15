using UnityEngine;

public class collisiontest : MonoBehaviour
{
    protected void OnCollisionStay(Collision other)
    {
        Debug.DrawLine(transform.position, other.contacts[0].point, Color.red);
    }
}