using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Orbit : MonoBehaviour
{
	private static int pointsCount = 64;

	private LineRenderer lineRenderer;

	public float radius = 1.0f;

	void Start()
    {
		lineRenderer = GetComponent<LineRenderer>();

		Vector3[] points = new Vector3[pointsCount + 1];
		float angle = 2.0f * Mathf.PI / pointsCount;
		for (int i = 0; i < pointsCount; i++)
        {
			points[i] = new Vector3 (radius * Mathf.Cos(i * angle), 0.0f, radius * Mathf.Sin(i * angle));
		}
		points[pointsCount] = points[0];

		lineRenderer.SetVertexCount(pointsCount + 1);
		lineRenderer.SetPositions(points);
	}
}
