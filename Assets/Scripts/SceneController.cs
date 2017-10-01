using UnityEngine;
using System.Collections;
using System.Linq;

public class SceneController : MonoBehaviour
{
  public GameController gameContoller;
  public MissionController missionController;
  public Camera mainCamera;
  public GameObject sun;
  public Orbit[] orbits;
  public GameObject spaceshipPrefab;
  public GameObject explosionPrefab;

  private Planet[] planets;

  private Planet selectedPlanet;

	void Start()
	{
    RestartScene();
	}

  public void RestartScene()
  {
    gameContoller.ClearGameTickSubscribers();

    planets = new Planet[orbits.Length];
    for (int i = 0; i < orbits.Length; i++)
    {
      planets[i] = orbits[i].gameObject.GetComponentInChildren<Planet>(true);

      CrystalFactory factory = orbits[i].gameObject.GetComponentInChildren<CrystalFactory>(true);
      gameContoller.OnGameTick += factory.OnGameTick;
      factory.OnAmountChanged = gameContoller.OnCrystalsAmountChanged;
      factory.OnProgress = gameContoller.OnProgress;
      factory.OnWaiting = gameContoller.OnWaiting;
      factory.Stock = gameContoller.Stock;
      factory.Reset();
    }

    var objects = GameObject.FindGameObjectsWithTag("Spaceship");
    foreach (var obj in objects)
      GameObject.Destroy(obj);

    var effects = GameObject.FindGameObjectsWithTag("Effect");
    foreach (var effect in effects)
      GameObject.Destroy(effect);
  }

	void Update()
	{
    if (gameContoller.Paused)
      return;
    
    if (gameContoller.IsPlaying)
    {
      if (Input.GetMouseButtonDown(0))
      {
        this.selectedPlanet = SelectPlanet();
        if (this.selectedPlanet != null)
          this.selectedPlanet.SetSelected(true);
        missionController.OnSelectPlanet(this.selectedPlanet);
      }

      if (this.selectedPlanet != null && Input.GetMouseButton(0))
      {
        Planet planet = SelectPlanet();
        if (planet != null && planet != this.selectedPlanet)
        {
          SendSpaceship(this.selectedPlanet, planet);
          this.selectedPlanet.SetSelected(false);
          this.selectedPlanet = null;
          missionController.OnSelectPlanet(this.selectedPlanet);
        }
      }

      if (Input.GetMouseButtonUp(0))
      {
        if (this.selectedPlanet != null)
          this.selectedPlanet.SetSelected(false);
        this.selectedPlanet = null;
        missionController.OnSelectPlanet(this.selectedPlanet);
      }
    }
    else
    {
      if (this.selectedPlanet != null)
          this.selectedPlanet.SetSelected(false);
      this.selectedPlanet = null;
      missionController.OnSelectPlanet(this.selectedPlanet);
    }
	}

  private Planet SelectPlanet()
  {
    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;
    if (Physics.SphereCast(ray, 0.75f, out hit))
    {
      int hitId = hit.collider.gameObject.GetInstanceID();
      return (from p in planets where p.gameObject.GetInstanceID() == hitId select p).SingleOrDefault();
    }
    return null;
  }

  private void SendSpaceship(Planet fromPlanet, Planet toPlanet)
  {
    GameObject obj = Instantiate(spaceshipPrefab) as GameObject;
    Spaceship spaceship = obj.GetComponent<Spaceship>();

    CrystalFactory factory = fromPlanet.gameObject.GetComponent<CrystalFactory>();
    CrystalPack pack = factory.TakeCrystals(spaceship.crystalCarrying);

    if (pack.amount != 0)
    {
      spaceship.Cargo(pack);
    }
    else
    {
      obj.SetActive(false);
      return;
    }

    spaceship.gameController = gameContoller;
    spaceship.fromPlanet = fromPlanet;
    spaceship.toPlanet = toPlanet;
    spaceship.sun = sun;
    spaceship.explosion = explosionPrefab;
    spaceship.speed = missionController.GetSpaceshipSpeed();
    spaceship.OnApplyCargo += OnApplyCargo;
    MeshRenderer renderer = obj.GetComponentInChildren<MeshRenderer>();
    renderer.material = fromPlanet.shipMaterial;
  }

  public void OnApplyCargo()
  {
    GetComponent<AudioSource>().Play();
  }
}
