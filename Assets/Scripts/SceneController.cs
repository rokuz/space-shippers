using UnityEngine;
using System.Collections;
using System.Linq;

public class SceneController : MonoBehaviour
{
    public Camera mainCamera;
    public Orbit[] orbits;
    public GameObject spaceshipPrefab;

    private Planet[] planets;

    private Planet selectedPlanet;

	void Start()
	{
        planets = new Planet[orbits.Length];
        for (int i = 0; i < orbits.Length; i++)
        {
            planets[i] = orbits[i].gameObject.GetComponentInChildren<Planet>(true);
        }
	}

	void Update()
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

    private Planet SelectPlanet()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
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
        spaceship.fromPlanet = fromPlanet;
        spaceship.toPlanet = toPlanet;
    }
}
