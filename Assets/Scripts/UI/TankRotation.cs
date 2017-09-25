using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankRotation : MonoBehaviour
{
  public GameController gameController;
  public float speed = 360.0f;
  private RectTransform rectTransform;

	public void Awake()
  {
    rectTransform = this.gameObject.GetComponent<RectTransform>();
	}

  public void Update()
  {
    if (gameController.Paused)
      return;
    rectTransform.Rotate(new Vector3(0.0f, 0.0f, speed * Time.deltaTime));
	}
}
