using UnityEngine;
using System.Collections;

public class Spaceship : MonoBehaviour
{
    public GameObject sun;
    public GameObject explosion;

    public Planet fromPlanet;
    public Planet toPlanet;
    public float speed = 5.0f;
    public uint crystalCarrying = 1;

    private CrystalPack cargo;

	public void Start()
    {
        Vector3 fromPlanetPos = fromPlanet.transform.position;
        Vector3 toPlanetPos = toPlanet.transform.position;
        Vector3 dir = toPlanetPos - fromPlanetPos;
        dir.Normalize();
        this.transform.forward = dir;

        float shipRadius = this.GetComponentInChildren<CapsuleCollider>(true).height * 0.5f;
        this.transform.position = fromPlanetPos + (fromPlanet.planetRadius + shipRadius) * dir;
	}
	
	public void Update()
    {
        Vector3 dir = toPlanet.transform.position - this.transform.position;
        dir.Normalize();
        this.transform.forward = dir;

        this.transform.Translate(0, 0, speed * Time.deltaTime);
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
        }
        else if (id == sun.GetInstanceID())
        {
            Vector3 pos = this.transform.position;

            GameObject obj = Instantiate(explosion) as GameObject;
            obj.transform.position = pos;
            obj.GetComponent<ParticleSystem>().Play();

            this.gameObject.SetActive(false);
        }
    }
}
