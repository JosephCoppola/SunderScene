using UnityEngine;
using System.Collections;

public class WaterScript : MonoBehaviour
{
	public GameObject splashPrefab;

	void OnTriggerEnter(Collider col)
	{
		RaycastHit hit;
		if (Physics.Raycast(col.transform.position, -Vector3.up, out hit))
		{
			Instantiate(splashPrefab, hit.point, splashPrefab.transform.rotation);
			//Debug.Log("Point of contact: "+hit.point);
		}

		if(col.tag == "Enemy")
		{
			col.gameObject.GetComponent<EnemyScript>().Die();
		}
		else if(col.tag == "Player")
		{
			col.gameObject.GetComponent<PlayerScript>().Die();
		}
	}
}
