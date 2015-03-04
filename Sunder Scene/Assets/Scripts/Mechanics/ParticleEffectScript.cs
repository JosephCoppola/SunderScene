using UnityEngine;
using System.Collections;

public class ParticleEffectScript : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		StartCoroutine (endParticle ());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private IEnumerator endParticle()
	{
		yield return new WaitForSeconds (3.0f);
		Destroy (gameObject);
	}
}
