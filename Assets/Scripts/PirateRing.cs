using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class PirateRing : MonoBehaviour
{
  private static int pointsCount = 64;

  public float radius = 1.0f;
  private bool initialized = false;

  void Start()
  {
    ApplyRadius(radius);
  }

  void Update()
  {
    if (!initialized)
    {
      ApplyRadius(radius);
      initialized = true;
    }

    float k = Mathf.Abs(Mathf.Cos(Time.time));
    this.transform.localScale = new Vector3(k, 1.0f, k);
  }

  public void ApplyRadius(float r)
  {
    radius = r;

    LineRenderer lineRenderer = GetComponent<LineRenderer>();
    Vector3[] points = new Vector3[pointsCount + 1];
    float angle = 2.0f * Mathf.PI / pointsCount;
    for (int i = 0; i < pointsCount; i++)
      points[i] = new Vector3(radius * Mathf.Cos(i * angle), 0.0f, radius * Mathf.Sin(i * angle));
    points[pointsCount] = points[0];

    lineRenderer.positionCount = pointsCount + 1;
    lineRenderer.SetPositions(points);
  }
}
