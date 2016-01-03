using UnityEngine;
using System.Collections;

public class Sun : MonoBehaviour
{
	private LensFlare lensFlares;
	public float angularSpeed = 180.0f;

	void Start()
	{
		lensFlares = GetComponent<LensFlare>();
	}

	void Update()
	{
		this.transform.RotateAround(Vector3.zero, Vector3.up, Mathf.Deg2Rad * angularSpeed * Time.deltaTime);

		if (lensFlares != null)
			lensFlares.brightness = 0.5f + 0.02f * Mathf.Sin(Time.realtimeSinceStartup);
	}
}
