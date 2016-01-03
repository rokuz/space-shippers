using UnityEngine;
using System.Collections;

public class Spaceship : MonoBehaviour
{
    public Planet fromPlanet;
    public Planet toPlanet;
    public float speed = 5.0f;

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

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetInstanceID() == toPlanet.gameObject.GetInstanceID())
        {
            Destroy(this.gameObject);
        }
    }
}
