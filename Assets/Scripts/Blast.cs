using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blast : MonoBehaviour
{
  public GameController gameController;
	public Vector3 direction;

	void Update()
  {
    if (gameController != null && gameController.Paused)
      return;

    this.transform.forward = direction;
    this.transform.Translate(0, 0, 100.0f * Time.deltaTime);

    if (this.transform.position.magnitude >= 30.0f)
      this.gameObject.SetActive(false);
	}
}
