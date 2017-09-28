using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlanetSelection : MonoBehaviour
{
  private static int pointsCount = 64;

  public enum Mode
  {
    X, Y, Z
  }

  public float radius = 1.0f;
  public Mode mode = Mode.Y;
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
  }

  public void ApplyRadius(float r)
  {
    radius = r;

    LineRenderer lineRenderer = GetComponent<LineRenderer>();
    Vector3[] points = new Vector3[pointsCount + 1];
    float angle = 2.0f * Mathf.PI / pointsCount;
    for (int i = 0; i < pointsCount; i++)
    {
      if (mode == Mode.X)
        points[i] = new Vector3(0.0f, radius * Mathf.Cos(i * angle), radius * Mathf.Sin(i * angle));
      else if (mode == Mode.Y)
        points[i] = new Vector3(radius * Mathf.Cos(i * angle), 0.0f, radius * Mathf.Sin(i * angle));
      else if (mode == Mode.Z)
        points[i] = new Vector3(radius * Mathf.Cos(i * angle), radius * Mathf.Sin(i * angle), 0.0f);
    }
    points[pointsCount] = points[0];

    lineRenderer.positionCount = pointsCount + 1;
    lineRenderer.SetPositions(points);
  }
}
