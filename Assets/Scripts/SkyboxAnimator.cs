using UnityEngine;
using System.Collections;

public class SkyboxAnimator : MonoBehaviour
{
    public float angularSpeed = 1.0f;

    private float currentAngle = 0.0f;

	void Update()
    {
        currentAngle += angularSpeed * Time.deltaTime;
        while (currentAngle >= 360.0f)
            currentAngle -= 360.0f;

        RenderSettings.skybox.SetFloat("_Rotation", currentAngle);
	}
}
