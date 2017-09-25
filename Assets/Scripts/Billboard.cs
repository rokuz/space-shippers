using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
  public float rotation = 90.0f;
  public float minOffset = 4.0f;
  public float maxOffset = 5.0f;
  public float offsetSpeed = 1.0f;

  private float lerpCoef = 0.0f;
  private bool forward = true;

	public void Update()
  {
    lerpCoef += offsetSpeed * Time.deltaTime;
    while (lerpCoef >= 1.0f)
    {
      lerpCoef -= 1.0f;
      forward = !forward;
    }
    float offset = Mathf.Lerp(forward ? minOffset : maxOffset, forward ? maxOffset : minOffset, lerpCoef);
    transform.LookAt(Camera.main.transform.position, -Vector3.up);
    transform.Rotate(new Vector3(0.0f, 0.0f, rotation));
    transform.position = new Vector3(transform.position.x, offset, transform.position.z);
	}
}
