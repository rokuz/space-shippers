using UnityEngine;
using System.Collections;

public class Spaceship : MonoBehaviour
{
  public GameController gameController;
  public GameObject sun;
  public GameObject explosion;

  public Planet fromPlanet;
  public Planet toPlanet;
  public float speed = 5.0f;
  public uint crystalCarrying = 1;

  public delegate void OnApplyCargoEvent();
  public OnApplyCargoEvent OnApplyCargo;

  private CrystalPack cargo;

  private float lifeTime = 0.0f;

	public void Start()
  {
    Vector3 fromPlanetPos = fromPlanet.transform.position;
    Vector3 toPlanetPos = toPlanet.transform.position;
    Vector3 dir = toPlanetPos - fromPlanetPos;
    dir.Normalize();
    this.transform.forward = dir;

    float shipRadius = this.GetComponentInChildren<CapsuleCollider>(true).height * 0.5f;
    this.transform.position = fromPlanetPos + (fromPlanet.planetRadius + shipRadius) * dir;

    var audio = GetComponent<AudioSource>();
    audio.Play();
	}
	
	public void Update()
  {
    if (gameController != null && gameController.Paused)
      return;

    lifeTime += Time.deltaTime;
    if (lifeTime > 10.0f)
      Explode();
    
    Vector3 dir = toPlanet.transform.position - this.transform.position;
    dir.Normalize();
    this.transform.forward = dir;

    this.transform.Translate(0, 0, speed * Time.deltaTime);
	}

  private void Explode()
  {
    Vector3 pos = this.transform.position;

    GameObject obj = Instantiate(explosion) as GameObject;
    obj.transform.position = pos;
    obj.GetComponent<ParticleSystem>().Play();
    obj.GetComponent<AudioSource>().Play();

    this.gameObject.SetActive(false);
  }

  public void Cargo(CrystalPack pack)
  {
    cargo = pack;
  }
      
  public void OnTriggerEnter(Collider other)
  {
    int id = other.gameObject.GetInstanceID();
    if (id == toPlanet.gameObject.GetInstanceID())
    {
      CrystalFactory factory = toPlanet.gameObject.GetComponent<CrystalFactory>();
      factory.ApplyCargo(cargo);
      this.gameObject.SetActive(false);
      if (OnApplyCargo != null)
        OnApplyCargo();
    }
    else if (id == sun.GetInstanceID())
    {
      Explode();
    }
  }
}
