using UnityEngine;
using System.Collections;

public class JoeScript : EnemyScript {

	public bool attacking;
	public EnemyWeaponScript weapon;
	private AudioSource audioSource;
	
	void Start()
	{
		health = 100;
		tooClose = 3.0f;
		range = 3.0f;
		maxSpeed = 5.0f;
		maxForce = 100.0f;
		radius = 1.0f;
		mass = 1.0f;
		speed = 2.0f;

		separationWt = 30.0f;
		seekWt = 10.0f;
		attacking = false;
		scorePts = 10;

		audioSource = GetComponent<AudioSource>();
	}

	void Update()
	{
		if(attacking)
		{
			weapon.DetectCollision(5);
		}

		if(!flinch && alive && !attacking && IsGrounded())
		{
			if(Vector3.Distance(transform.position, target.position) > range)
			{
				CalcSteeringForce();
				ApplyAcceleration();
				ApplyToRigidbody();

				//reset acceleration for next cycle
				acceleration = Vector3.zero;
			}
			else
			{
				Attack();
			}
		}
	}

	private void Attack()
	{
		if(!attacking)
		{
			StartCoroutine(AttackDelay());
			transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
			attacking = true;
			int num = Random.Range(1,4);
			animationController.SetTrigger("Attack" + num);
		}
	}

	private IEnumerator AttackDelay()
	{
		yield return new WaitForSeconds(0.2f);
		if(attacking)
		{
			weapon.StartAttack();
		}
	}

	private void EndAttack()
	{
		attacking = false;
		weapon.EndAttack();
	}

	public override void Flinch()
	{
		if(!flinch)
		{
			attacking = false;
			animationController.SetBool("Flinch", true);
			flinch = true;
		}
	}

	private void EndFlinch()
	{
		flinch = false;
		animationController.SetBool("Flinch", false);
	}

	public override void Die()
	{
		audioSource.Play();
		weapon.enabled = false;
		mngr.RemoveEnemy(transform);
		animationController.SetBool("Death", true);
		alive = false;
		StartCoroutine(DisableDelay());
	}
}
