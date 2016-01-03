using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Planet : MonoBehaviour
{
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

    public SceneController sceneController;
	public GameObject sun;
	public Texture planetTexture;

	private Material groundMaterial;

	private float innerRadius; // Radius of the ground sphere
	private float outerRadius; // Radius of the sky sphere
	private Vector3 invWaveLength4 = new Vector3(1.0f / Mathf.Pow(0.65f, 4.0f), 1.0f / Mathf.Pow(0.57f, 4.0f), 1.0f / Mathf.Pow(0.475f, 4.0f));

	public void Start() 
	{
		orbit = this.GetComponentInParent<Orbit>();
		if (orbit != null)
		{
			currentPosition = startPosition;
	
			float angle = this.currentPosition * 2.0f * Mathf.PI;
			this.transform.position = new Vector3 (orbit.radius * Mathf.Cos (angle), 0.0f, orbit.radius * Mathf.Sin (angle));
		}

		Vector2 rnd = axisDeviation * Random.insideUnitCircle;
		axis = new Vector3(rnd.x, 1.0f, rnd.y);
		axis.Normalize();

		string GroundObjectName = "Ground";
		const float scale = 1.025f;
		this.innerRadius = this.planetRadius;
		this.outerRadius = scale * innerRadius;
		foreach (Transform t in this.GetComponentsInChildren<Transform>(true)) 
		{
			if (t.gameObject.name == GroundObjectName)
				t.localScale = new Vector3(this.planetRadius, this.planetRadius, this.planetRadius);
		}
		foreach (MeshRenderer r in this.GetComponentsInChildren<MeshRenderer>(true)) 
		{
			if (r.gameObject.name == GroundObjectName) 
			{
				this.groundMaterial = new Material(Shader.Find("Space/PlanetGround"));
				this.groundMaterial.mainTexture = planetTexture;
				r.material = this.groundMaterial;
			} 
		}

        SphereCollider collider = this.GetComponent<SphereCollider>();
        collider.radius = this.planetRadius;
			
		if (groundMaterial != null)
			UpdateMaterial(groundMaterial);
	}

	public void Update() 
	{
		this.transform.RotateAround(Vector3.zero, axis, Mathf.Deg2Rad * angularSpeed * Time.deltaTime);

		if (orbit != null)
		{
			float speedRadians = Mathf.Deg2Rad * speed;
			this.currentPosition += (speedRadians * Time.deltaTime / (2.0f * Mathf.PI));
			while (this.currentPosition >= 1.0f)
				this.currentPosition -= 1.0f;
			float angle = this.currentPosition * 2.0f * Mathf.PI;
			this.transform.position = new Vector3 (orbit.radius * Mathf.Cos (angle), 0.0f, orbit.radius * Mathf.Sin (angle));
		}

		if (groundMaterial != null)
			UpdateMaterial(groundMaterial);
	}

	private void UpdateMaterial(Material mat)
	{
		if (sun == null)
			return;
		
		const float hdrExposure = 0.8f;
		const float ESun = 20.0f; // Sun brightness constant
		const float kr = 0.0025f; // Rayleigh scattering constant
		const float km = 0.0010f; // Mie scattering constant
		const float g = -0.990f; // The Mie phase asymmetry factor, must be between 0.999 to -0.999
		const float scaleDepth = 0.25f; // The scale depth (i.e. the altitude at which the atmosphere's average density is found)

		float scale = 1.0f / (outerRadius - innerRadius);
		Vector3 lightDir = sun.transform.position - this.transform.position;
		lightDir.Normalize();

		mat.SetVector("v3LightPos", lightDir);
		mat.SetVector("v3Translate", this.transform.position);
		mat.SetVector("v3InvWavelength", invWaveLength4);
		mat.SetFloat("fOuterRadius", outerRadius);
		mat.SetFloat("fOuterRadius2", outerRadius * outerRadius);
		mat.SetFloat("fInnerRadius", innerRadius);
		mat.SetFloat("fInnerRadius2", innerRadius * innerRadius);
		mat.SetFloat("fKrESun", kr * ESun);
		mat.SetFloat("fKmESun", km * ESun);
		mat.SetFloat("fKr4PI", kr * 4.0f * Mathf.PI);
		mat.SetFloat("fKm4PI", km * 4.0f * Mathf.PI);
		mat.SetFloat("fScale", scale);
		mat.SetFloat("fScaleDepth", scaleDepth);
		mat.SetFloat("fScaleOverScaleDepth", scale / scaleDepth);
		mat.SetFloat("fHdrExposure", hdrExposure);
		mat.SetFloat("g", g);
		mat.SetFloat("g2", g * g);
	}
}
