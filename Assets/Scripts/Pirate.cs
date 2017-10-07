using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class Pirate : MonoBehaviour
{
  public GameController gameController;
  public Spaceship chasedSpaceship;
  public GameObject blastPrefab;
  public GameObject explosionPrefab;

  private bool startPositionDetermined = false;
  private bool fired = false;
  private double fireTime = 0.0;
  private Transform blastEmitter;
  private GameObject tutorialPointer;

	void Start()
  {
    blastEmitter = this.transform.Find("BlastEmitter");
    tutorialPointer = this.transform.Find("TutorialPivot/TutorialPointer").gameObject;
    tutorialPointer.SetActive(!Persistence.gameConfig.pirateOnceDestroyed);
	}

	void Update()
  {
    if (gameController != null && gameController.Paused)
      return;

    if (!startPositionDetermined)
    {
      Vector3 dir = chasedSpaceship.transform.position;
      dir.Normalize();
      this.transform.position = dir * 50;
      this.transform.forward = -dir;
      startPositionDetermined = true;
    }

    Vector3 d = chasedSpaceship.transform.position - this.transform.position;
    d.Normalize();
    this.transform.forward = d;
    if (this.transform.position.magnitude > 26.0)
    {
      this.transform.Translate(0, 0, 70.0f * Time.deltaTime);
    }
    else
    {
      if (!fired)
      {
        var quat = Quaternion.LookRotation(d) * Quaternion.Euler(new Vector3(0.0f, Random.Range(-20.0f, 20.0f), 0.0f));
        GameObject obj = Instantiate(blastPrefab) as GameObject;
        obj.transform.position = blastEmitter.transform.position;
        Blast blast = obj.GetComponent<Blast>();
        blast.gameController = gameController;
        blast.direction = quat * Vector3.forward;
        fired = true;
        fireTime = Time.time;
      }
      else
      {
        if (Time.time - fireTime > 0.5)
          fired = false;
      }
    }
	}

  public void Hit()
  {
    Explode();
    Analytics.CustomEvent("Pirate_Destroyed");
  }

  private void Explode()
  {
    Vector3 pos = this.transform.position;

    GameObject obj = Instantiate(explosionPrefab) as GameObject;
    obj.transform.position = pos;
    obj.GetComponent<ParticleSystem>().Play();
    obj.GetComponent<AudioSource>().Play();

    this.gameObject.SetActive(false);
  }
}
