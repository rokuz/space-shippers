using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
public class Planet : MonoBehaviour
{
  public GameController gameController;
	private Orbit orbit;
	private float currentPosition;
	private Vector3 axis = Vector3.up;

	public float planetRadius = 1.0f;

	[Range(0.0f, 1.0f)]
	public float startPosition = 0.0f;

	public float speed = 30.0f;

	public float angularSpeed = 60.0f;

	[Range(0.0f, 1.0f)]
	public float axisDeviation = 0.0f;
    
  public Material shipMaterial;
  public GameObject tutorialPivot;

  [HideInInspector]
  public GameObject tutorialPointer;

  public void Awake()
  {
    orbit = this.GetComponentInParent<Orbit>();
  }
       
	public void Start() 
	{
    tutorialPointer = tutorialPivot.transform.Find("TutorialPointer").gameObject;
    ApplyInitialTransform();
	}

  public Orbit PlanetOrbit
  {
    get { return orbit; }
  }

  public void SetSelected(bool isSelected)
  {
    this.transform.Find("Selection").gameObject.SetActive(isSelected);
  }

  public void ApplyInitialTransform()
  {
    if (orbit != null)
    {
      currentPosition = startPosition;

      float angle = this.currentPosition * 2.0f * Mathf.PI;
      this.transform.position = new Vector3(orbit.radius * Mathf.Cos(angle), 0.0f, orbit.radius * Mathf.Sin(angle));
      tutorialPivot.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + this.planetRadius, this.transform.position.z);
    }

    Vector2 rnd = axisDeviation * Random.insideUnitCircle;
    axis = new Vector3(rnd.x, 1.0f, rnd.y);
    axis.Normalize();

    string kGroundObjectName = "Ground";
    foreach (Transform t in this.GetComponentsInChildren<Transform>(true)) 
    {
      if (t.gameObject.name == kGroundObjectName)
        t.localScale = new Vector3(this.planetRadius, this.planetRadius, this.planetRadius);
    }

    SphereCollider collider = this.GetComponent<SphereCollider>();
    collider.radius = this.planetRadius;
  }

	public void Update() 
	{
    if (gameController.Paused)
      return;
    
		this.transform.RotateAround(Vector3.zero, axis, Mathf.Deg2Rad * angularSpeed * Time.deltaTime);

		if (orbit != null)
		{
			float speedRadians = Mathf.Deg2Rad * speed;
			this.currentPosition += (speedRadians * Time.deltaTime / (2.0f * Mathf.PI));
			while (this.currentPosition >= 1.0f)
				this.currentPosition -= 1.0f;
			float angle = this.currentPosition * 2.0f * Mathf.PI;
			this.transform.position = new Vector3(orbit.radius * Mathf.Cos (angle), 0.0f, orbit.radius * Mathf.Sin(angle));
      tutorialPivot.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + this.planetRadius, this.transform.position.z);
		}
	}
}
