using UnityEngine;
using System.Collections;

public class TopScript : EnemyScript 
{
	public Transform head;
	public Transform body;

	private float spinWt;
	private bool charge;

	public GameObject deathParticle;
	public EnemyWeaponScript[] weapons;

	private AudioSource audioSource;

	void Start()
	{
		health = 250;
		tooClose = 3.0f;
		range = 20.0f;
		maxSpeed = 4.0f;
		maxForce = 100.0f;
		radius = 1.0f;
		mass = 1.0f;
		speed = 2.0f;

		charge = false;
			
		separationWt = 20.0f;
		seekWt = 3.0f;
		spinWt = 17.0f;
		scorePts = 50;

		audioSource = GetComponent<AudioSource>();
	}

	void Update()
	{
		if(flinch || !alive)
		{
			return;
		}

		head.LookAt(new Vector3(target.position.x, head.position.y, target.position.z));

		if(IsGrounded())
		{
			if(Vector3.Distance(transform.position, target.position) > range && charge == false && transform.position.y < 2.0f)
			{
				CalcTopSteeringForce();
				ApplyAcceleration();
				ApplyToRigidbody();

				acceleration = Vector3.zero;
			}
			else
			{
				if(transform.position.y < 2.0f)
				{
					Charge();
				}
				//head.LookAt(new Vector3(target.position.x, head.position.y, target.position.z));
				CalcSteeringForce();
				ApplyAcceleration();
				ApplyToRigidbody();

				for(int i = 0; i < weapons.Length; i++)
				{
					weapons[i].DetectCollision(2);
				}

				//reset acceleration for next cycle
				acceleration = Vector3.zero;
			}
		}
	}

	Vector3 Orbit(Vector3 target)
	{
		dv = target - transform.position; 
		dv = dv.normalized;
			
		return Vector3.Cross(dv,transform.up).normalized;
	}

	void CalcTopSteeringForce()
	{
		Vector3 force = Vector3.zero;
			
		// Don't run into others
		force += -separationWt * (Separate(mngr.Enemies, tooClose) - velocity);

			// Seek Target
		if(target != null) 
		{
			force += seekWt * Seek(target.position);
		}

			//Spin Around Target
		if(target != null)
		{
			force += spinWt * Orbit(target.position);
			
			force = Vector3.ClampMagnitude(force, maxForce);
			ApplyForce(force);
		}
	}

	private void Charge()
	{
		if(!charge)
		{
			audioSource.Play();
			charge = true;
			maxSpeed = 10.0f;
			spinWt = 10.0f;
			seekWt = 15.0f;
			animationController.SetBool("Charge", true);

			for(int i = 0; i < weapons.Length; i++)
			{
				weapons[i].MakeTop();
				weapons[i].StartAttack();
			}
		}
	}

	public override void Flinch()
	{
		if(!flinch)
		{
			animationController.SetBool("Flinch",true);
			flinch = true;
			//rigidbody.AddForce(swordHeading.normalized * 10);
		}
	}

	private void EndFlinch()
	{
		animationController.SetBool("Flinch",false);
		flinch = false;
	}

	public override void Die()
	{
		for(int i = 0; i < weapons.Length; i++)
		{
			weapons[i].enabled = false;
		}
		mngr.RemoveEnemy(transform);
		alive = false;
		animationController.SetBool ("Charge", false);
		animationController.SetBool ("Death", true);
		GameObject death = Instantiate(deathParticle) as GameObject;
		death.transform.parent = body;
		death.transform.position = body.position + new Vector3 (0.0f, 2.0f, 0.0f);
		head.renderer.enabled = false;
		animationController.SetTrigger("Death");
		StartCoroutine (DisableDelay ());
	}
}

