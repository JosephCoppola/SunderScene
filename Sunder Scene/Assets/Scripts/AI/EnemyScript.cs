using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class EnemyScript : MonoBehaviour
{
	// References
	protected ManagerScript mngr;
	protected Transform target;

	// Weights
	protected float separationWt;
	protected float seekWt;

	// Calculation vars
	protected float gravity;
	protected float tooClose;
	protected float range;
	protected float maxSpeed;
	protected float maxForce;
	protected float radius; // radius of the agent
	protected float mass; // could be useful for the big guys
	protected float speed;
	
	public bool flinch;
	
	protected Vector3 dv;
	protected Vector3 acceleration;
	protected Vector3 velocity;

	protected Animator animationController;

	protected int health;
	protected bool alive;
	protected float distToGround;
	protected int scorePts;

	// Initializes references and set initial values
	public void Initialize(ManagerScript _mngr, Transform _player)
	{
		animationController = GetComponent<Animator>();

		mngr = _mngr;
		target = _player;

		alive = true;
		gravity = 100.0f;
		acceleration = Vector3.zero;
		velocity = transform.forward;
		distToGround = collider.bounds.extents.y;
	}

	public bool Alive
	{
		get { return alive; }
	}

	// Finds the vector toward the target
	protected Vector3 Seek(Vector3 targetPos)
	{
		dv = targetPos - transform.position; //find dv, desired velocity
		dv = dv.normalized * maxSpeed; //scale by maxSpeed
		dv -= rigidbody.velocity;
		dv.y = 0; // only steer in the x/z plane
		return dv;
	}

	// Finds the vector away from others
	protected Vector3 Separate(List<Transform> others, float tooClose) {
		dv = Vector3.zero;
		foreach(Transform other in others) {
			if(other != transform) {
				float dist = Vector3.Distance(transform.position, other.position);
				if(dist < tooClose) {
					Vector3 targetPos =  Seek(other.position);
					targetPos.Normalize();
					targetPos *= 1 / dist;
					dv += targetPos;
				}
			}
		}
		dv.Normalize();
		dv *= maxSpeed;
		return dv;
	}

	// Might be good for the dragon
	/*protected Vector3 FollowLeader(Transform leader) {
		Vector3 followPos = leader.position - (leader.forward * tooClose);
		return Arrive(followPos);
	}*/

	// Calculates the steering force
	// Might end up being different for each enemy type
	protected void CalcSteeringForce()
	{
		Vector3 force = Vector3.zero;
		
		// Don't run into others
		force += -separationWt * (Separate(mngr.Enemies, tooClose) - velocity);
		
		// Seek Target
		if(target != null) {
			force += seekWt * Seek(target.position);
		}
		
		force = Vector3.ClampMagnitude(force, maxForce);
		ApplyForce(force);
	}

	// Adds the force to the acceleration
	protected void ApplyForce(Vector3 steeringForce)
	{
		acceleration += steeringForce / mass;
	}

	// Adds the acceleration to the velocity
	protected void ApplyAcceleration()
	{
		velocity += acceleration * Time.deltaTime;
		//velocity.y = 0;
		velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
		
		//orient the transform to face where we going
		if(velocity != Vector3.zero)
			transform.forward = velocity.normalized;
	}

	protected void ApplyToRigidbody()
	{
		rigidbody.velocity = new Vector3(velocity.x, rigidbody.velocity.y, velocity.z);
	}

	// Removes the enemy
	/*public void Die()
	{
		mngr.RemoveEnemy(transform);
		//Destroy(gameObject);
	}*/

	public abstract void Flinch();
	public abstract void Die();

	public void DoDamage(int amt)
	{
		if(alive)
		{
			health -= amt;
			if(health > 0)
			{
				Flinch();
			}
			else
			{
				AddToScore();
				Die();
			}
		}
	}

	protected IEnumerator DisableDelay()
	{
		yield return new WaitForSeconds(1.0f);
		if(IsGrounded())
		{
			rigidbody.isKinematic = true;
			collider.enabled = false;
			this.enabled = false;
		}
		else
		{
			StartCoroutine(DisableDelay());
		}
	}

	protected bool IsGrounded()
	{
		//return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.3f);
		RaycastHit hit;
		return Physics.SphereCast(transform.position, radius, -Vector3.up, out hit, distToGround + 0.3f);  
	}

	protected void AddToScore()
	{
		target.GetComponent<PlayerScript>().AddToScore(scorePts * WeaponScript.combo);
	}
}
