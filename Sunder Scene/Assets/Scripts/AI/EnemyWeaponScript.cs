using UnityEngine;
using System.Collections;

public class EnemyWeaponScript : MonoBehaviour
{
	private float length;
	private bool canHitPlayer;
	private bool top;

	void Start ()
	{
		top = false;
		canHitPlayer = false;
		length = transform.GetComponent<MeshFilter>().mesh.bounds.size.z;
	}

	public void DetectCollision(int dam)
	{
		RaycastHit hit;
		//Debug.DrawRay(transform.position, transform.right * length, Color.red, 100);
		if(Physics.Raycast(transform.position, top ? transform.right : transform.forward, out hit, length))
		{
			if(hit.collider.tag == "Player" && canHitPlayer)
			{
				PlayerScript ps = hit.collider.gameObject.GetComponent<PlayerScript>();
				if(ps.Alive)
				{
					ps.DoDamage(dam);
					
					Vector3 force = hit.collider.transform.position - transform.position;
					
					/*GameObject sparks = Instantiate(hitSparks) as GameObject;
					sparks.transform.position = hit.point;
					sparks.transform.eulerAngles = transform.position - hit.collider.transform.position;*/
					
					force.Normalize();
					hit.collider.rigidbody.AddForce(force * 10 * dam);

					if(!top)
					{
						canHitPlayer = false;
					}
				}
			}
		}
	}

	public void StartAttack()
	{
		canHitPlayer = true;
	}

	public void EndAttack()
	{
		canHitPlayer = false;
	}

	public void MakeTop()
	{
		top = true;
		length = transform.GetComponent<MeshFilter>().mesh.bounds.size.x;
	}
}