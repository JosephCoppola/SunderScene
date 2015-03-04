using UnityEngine;
using System.Collections;

public class WeaponScript : MonoBehaviour
{
	private float length;
	public static int combo;
	private GameObject lastHit;
	private float fontSize;

	public GUIText scoreText;
	public PlayerScript playerScript;
	public GameObject hitSparks;
	public GameObject dustParticles;
	public GameObject rockParticles;

	private AudioSource audioSource;
	
	void Start()
	{
		combo = 1;
		length = transform.GetComponent<MeshFilter>().mesh.bounds.size.z;
		audioSource = GetComponent<AudioSource>();
	}
	
	void Update()
	{
		if(scoreText.fontSize > 40)
		{
			fontSize -= 10 * Time.deltaTime;
			scoreText.fontSize = Mathf.RoundToInt(fontSize);
		}
		else if(combo > 1)
		{
			combo--;
			UpdateComboText();
		}
	}
	
	public void DetectCollision(int dam)
	{
		RaycastHit hit;
		if(Physics.Raycast(transform.position, transform.forward, out hit, length))
		{
			if(hit.collider.tag == "Enemy")
			{
				EnemyScript es = hit.collider.gameObject.GetComponent<EnemyScript>();
				if(es.Alive && dam > 0)
				{
					es.DoDamage(dam);

					Vector3 force = hit.collider.transform.position - transform.position;

					GameObject sparks = Instantiate(hitSparks) as GameObject;
					sparks.transform.position = hit.point;
					sparks.transform.eulerAngles = force;

					force.Normalize();
					hit.collider.rigidbody.AddForce(force * 20 * dam);
					audioSource.Play();
					if(lastHit != hit.collider.gameObject)
					{
						lastHit = hit.collider.gameObject;
						combo++;
						UpdateComboText();
					}
				}
			}
			else if(hit.collider.tag == "Environment")
			{
				if(dam > 0 || playerScript.rigidbody.velocity != Vector3.zero)
				{
					GameObject dust = Instantiate(dustParticles) as GameObject;
					dust.transform.position = hit.point;
					dust.transform.eulerAngles = hit.normal;
				}
			}
			else if(hit.collider.tag == "Shield")
			{
				if(dam > 0)
				{
					GameObject rocks = Instantiate(rockParticles) as GameObject;
					rocks.transform.position = hit.point;
					rocks.transform.eulerAngles = hit.normal;
				}
			}
		}
		else
		{
			lastHit = null;
		}
	}
	
	private void UpdateComboText()
	{
		scoreText.text = "x" + combo;
		scoreText.fontSize = 60;
		fontSize = 60;
	}

	public void ResetCombo()
	{
		combo = 1;
		UpdateComboText();
	}
}