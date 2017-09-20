using UnityEngine;
using System.Collections;
using System.Linq;

public class SceneController : MonoBehaviour
{
    public GameController gameContoller;
    public Camera mainCamera;
    public GameObject sun;
    public Orbit[] orbits;
    public GameObject spaceshipPrefab;
    public GameObject explosionPrefab;

    private Planet[] planets;

    private Planet selectedPlanet;

	void Start()
	{
        planets = new Planet[orbits.Length];
        for (int i = 0; i < orbits.Length; i++)
        {
            planets[i] = orbits[i].gameObject.GetComponentInChildren<Planet>(true);

            CrystalFactory factory = orbits[i].gameObject.GetComponentInChildren<CrystalFactory>(true);
            gameContoller.OnGameTick += factory.OnGameTick;
            factory.OnAmountChanged += gameContoller.OnCrystalsAmountChanged;
            factory.Stock = gameContoller.Stock;
        }
	}

	void Update()
	{
        if (gameContoller.IsPlaying)
        {
            if (Input.GetMouseButtonDown(0))
            {
                this.selectedPlanet = SelectPlanet();
            }

            if (this.selectedPlanet != null && Input.GetMouseButton(0))
            {
                Planet planet = SelectPlanet();
                if (planet != null && planet != this.selectedPlanet)
                {
                    SendSpaceship(this.selectedPlanet, planet);
                    this.selectedPlanet = null;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                this.selectedPlanet = null;
            }
        }
        else
        {
            this.selectedPlanet = null;
        }
	}

    private Planet SelectPlanet()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.SphereCast(ray, 0.5f, out hit))
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
            Destroy(obj);
            return;
        }

        spaceship.fromPlanet = fromPlanet;
        spaceship.toPlanet = toPlanet;
        spaceship.sun = sun;
        spaceship.explosion = explosionPrefab;
        MeshRenderer renderer = obj.GetComponentInChildren<MeshRenderer>();
        renderer.material = fromPlanet.shipMaterial;
    }
}
