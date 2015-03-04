using UnityEngine;
using System.Collections;

public class HammerScript : EnemyScript {

	public bool attacking;
	public bool bash;
	public Transform feet;
	public Transform hammerEnd;
	public Transform shield;
	private float minRange;
	private Animator feetAnimator;

	public GameObject sandPrefab;
	private AudioSource deathSound;
	private AudioSource slamSound;

	void Start()
	{
		health = 1000;
		tooClose = 3.0f;
		range = 6.0f;
		minRange = 4.0f;
		maxSpeed = 5.0f;
		maxForce = 100.0f;
		radius = 5.0f;
		mass = 1.0f;
		speed = 1.0f;
		
		separationWt = 20.0f;
		seekWt = 10.0f;

		feetAnimator = feet.GetComponent<Animator>();

		bash = false;
		attacking = false;

		scorePts = 100;

		AudioSource[] audioSources = GetComponents<AudioSource>();
		slamSound = audioSources[0];
		deathSound = audioSources[1];
	}
	
	void Update()
	{
		if(flinch || !alive)
		{
			return;
		}
		
		if(Vector3.Distance(transform.position, target.position) > range && bash == false && attacking == false)
		{
			CalcSteeringForce();
			ApplyAcceleration();
			ApplyToRigidbody();
			
			//reset acceleration for next cycle
			acceleration = Vector3.zero;
		}
		else if(Vector3.Distance(transform.position, target.position) < minRange && !attacking)
		{
				transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
				bash = true;
				ShieldBash();
				feetAnimator.SetBool("Moving", false);
		}
		else
		{
			Attack();
			feetAnimator.SetBool("Moving", false);
		}
	}
	
	private void Attack()
	{
		if(!attacking || animationController.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
		{
			transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
			attacking = true;
			animationController.SetTrigger("Attack");
		}
	}

	private void HammerSmash()
	{
		GameObject sand = Instantiate(sandPrefab) as GameObject;
		sand.transform.position = hammerEnd.position;
		slamSound.Play();

		Collider[] hits = Physics.OverlapSphere (hammerEnd.position, 5.0f);

		for(int i = 0; i < hits.Length; i++)
		{
			Vector3 hammerEndForce = new Vector3(hammerEnd.position.x, hits[i].transform.position.y,hammerEnd.position.z);
			
			Vector3 hammerForce = hits[i].transform.position - hammerEndForce;

			if(hits[i].gameObject == gameObject || hits[i].gameObject.tag == "Environment" || hits[i].name == "BDshield")
			{
				continue;
			}
			else if(hits[i].gameObject.tag == "Enemy")
			{
				hits[i].gameObject.GetComponent<EnemyScript>().Flinch();
			}
			else if(hits[i].gameObject.tag == "Player")
			{
				hits[i].gameObject.GetComponent<PlayerScript>().DoDamage(Mathf.RoundToInt(1/hammerForce.magnitude * 100));
			}

			hammerForce = hammerForce.normalized/hammerForce.magnitude;

			hammerForce *= 3500.0f;

			hammerForce.y = 250.0f;

			hits[i].collider.rigidbody.AddForce(hammerForce);
		}
	}

	private void ShieldBash()
	{
		animationController.SetTrigger("Shield Bash");

		Collider[] hits =  Physics.OverlapSphere (shield.position, 5.0f);
		
		for(int i = 0; i < hits.Length; i++)
		{
			if(hits[i].gameObject == gameObject || hits[i].gameObject.tag == "Environment" || hits[i].name == "BDshield")
			{
				continue;
			}
			else if(hits[i].gameObject.tag == "Enemy")
			{
				hits[i].gameObject.GetComponent<EnemyScript>().Flinch();
			}
			else if(hits[i].gameObject.tag == "Player")
			{
				hits[i].gameObject.GetComponent<PlayerScript>().Flinch();
			}
			
			//Debug.Log(hits[i].name);
			
			Vector3 shieldVector = new Vector3(shield.position.x,hits[i].transform.position.y,shield.position.z);
			
			Vector3 shieldForce = hits[i].transform.position - shieldVector;
			
			shieldForce = shieldForce.normalized/shieldForce.magnitude;
			
			shieldForce *= 1000.0f;
			
			hits[i].collider.rigidbody.AddForce(shieldForce);
		}
	}

	private void EndAttack()
	{
		attacking = false;
		bash = false;
		feetAnimator.SetBool ("Moving", true);
	}
	
	private void setIdle()
	{
		feetAnimator.SetTrigger("Moving");
	}

	public override void Flinch()
	{

	}

	public void EndFlinch()
	{
		flinch = false;
	}

	public override void Die()
	{
		deathSound.Play();
		mngr.RemoveEnemy(transform);
		alive = false;
		StartCoroutine (DisableDelay ());
		shield.gameObject.collider.enabled = false;
		feetAnimator.SetTrigger("Death");
		animationController.SetTrigger("Death");
	}
}