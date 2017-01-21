using UnityEngine;

public class DestroyOnAwake : MonoBehaviour
{
	protected void Awake()
    {
        Destroy(gameObject);
	}
}