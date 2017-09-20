using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemCollector : MonoBehaviour
{
    public float lifetime = 1.0f;

	void Start()
    {
		StartCoroutine(DeferredDestroy());
	}
	
    private IEnumerator DeferredDestroy()
    {
        yield return new WaitForSeconds(lifetime);
        this.gameObject.SetActive(false);
    }
}
