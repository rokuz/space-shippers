using UnityEngine;
using System.Collections;

public class Sun : MonoBehaviour
{
	public float angularSpeed = 180.0f;
    
	void Update()
	{
		this.transform.RotateAround(Vector3.zero, Vector3.up, Mathf.Deg2Rad * angularSpeed * Time.deltaTime);
	}
}
