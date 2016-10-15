using UnityEngine;

public class TargetMovement : MonoBehaviour
{
    [Header("Circle"), SerializeField, Range(0.01f, 1f)]
    private float m_Amplitude = 0.5f;
    private float m_Angle = 0;

    private enum Movement { None, Circle };
    private Movement m_CurrentMovement;

    protected void Start()
    {
        m_CurrentMovement = Movement.Circle;
    }

	protected void Update()
    {
        switch(m_CurrentMovement)
        {
            case Movement.Circle:
                Circle();
                break;

            case Movement.None:
            default:
                break;
        }	
	}

    private void Circle()
    {
        m_Angle += 0.01f;

        // Polar coordinates
        float x = m_Amplitude * Mathf.Sin(m_Angle) * Mathf.Rad2Deg;
        float z = m_Amplitude * Mathf.Cos(m_Angle) * Mathf.Rad2Deg;

        // TO DO: Remove the magic number for the Y value
        transform.position = new Vector3(x, 5, z);
    }
}