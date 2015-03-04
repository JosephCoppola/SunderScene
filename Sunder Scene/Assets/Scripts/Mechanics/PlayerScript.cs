using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour
{
	public ManagerScript mngr;
	public WeaponScript weaponScript;
	public Transform swordPivot;
	public Transform sword;
	public Transform body;
	public Transform feet;
	public Camera calcCam;
	public GUIText healthDisplay;
	public GUIText scoreDisplay;

	private Vector3 mousePos;
	private Vector3 swordVel;
	private Transform target;
	private int targetNum;

	private float swordVelMult = 50.0f;
	private float swordVelDamp = 0.25f;

	private Animator swordAnimator;
	private Animator bodyAnimator;
	private Animator feetAnimator;

	private bool alive;
	private int health;
	private bool flinch;
	private float distToGround;
	private int score;
	private int kills;

	private AudioSource audioSource;

	void Start()
	{
		kills = 0;
		score = 0;
		alive = true;
		flinch = false;
		health = 500;
		targetNum = 0;
		mousePos = new Vector3(Screen.width, Screen.height*0.5f, 100);
		swordVel = Vector3.zero;

		swordAnimator = sword.GetComponent<Animator>();
		bodyAnimator = body.GetComponent<Animator>();
		feetAnimator = feet.GetComponent<Animator>();

		distToGround = collider.bounds.extents.y;

		audioSource = GetComponent<AudioSource>();
	}

	void Update()
	{
		if(alive && !flinch && mngr.GameStarted)
		{
			HandleInput();

			if(target != null)
			{
				transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
			}
		}

		swordVel *= swordVelDamp;
	}

	public bool Alive
	{
		get { return alive; }
	}

	private bool IsGrounded()
	{
		return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
	}

	private void HandleInput()
	{
		if(Input.GetMouseButtonDown(0))
		{
			Screen.lockCursor = true;
		}

		DoMovement();

		if(Input.GetKeyDown(KeyCode.Tab))
		{
			if(mngr.Enemies.Count > 0)
			{
				targetNum = (targetNum+1)%mngr.Enemies.Count;
				target = mngr.Enemies[targetNum];
			}
		}

		if(Input.GetMouseButton(0) || Input.GetMouseButton(1))
		{
			DoSlash();
		}
		else
		{
			Vector3 mouseOffset = new Vector3(0, Input.GetAxis("Mouse X"), 0f);
			transform.Rotate(mouseOffset * 2, Space.Self);
		}
	}

	private void DoMovement()
	{
		float vert = Input.GetAxisRaw("Vertical");
		float hor = Input.GetAxisRaw("Horizontal");
		if(IsGrounded())
		{
			if(vert != 0 || hor != 0)
			{
				Vector3 velocity = new Vector3(0, rigidbody.velocity.y, 0);
				velocity += transform.forward * vert;
				velocity += transform.right * hor;
				rigidbody.velocity = velocity.normalized * 10;
				feetAnimator.SetBool("Moving", true);

				if(vert >= 0)
				{
					feet.localRotation = Quaternion.AngleAxis(Mathf.Rad2Deg * Mathf.Atan2(hor, vert), feet.up);
				}
				else
				{
					feet.localRotation = Quaternion.AngleAxis(Mathf.Rad2Deg * Mathf.Atan2(-hor, -vert), feet.up);
				}
			}
			else
			{
				rigidbody.velocity = new Vector3(0, rigidbody.velocity.y, 0);
				feetAnimator.SetBool("Moving", false);
			}
		}
		else
		{
			feetAnimator.SetBool("Moving", false);
		}

		feetAnimator.SetFloat("VertSpeed", vert);
		feetAnimator.SetFloat("HorSpeed", hor);
	}

	private void DoSlash()
	{
		Vector3 mouseOffset = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0f);
		swordVel += mouseOffset * swordVelMult;
		mousePos += swordVel;
		mousePos.x = Mathf.Clamp(mousePos.x, -Screen.width * 2, Screen.width * 2);
		mousePos.y = Mathf.Clamp(mousePos.y, -Screen.height, Screen.height * 2);

		if(mouseOffset.sqrMagnitude > 1f)
		{
			float angle = Mathf.Rad2Deg * Mathf.Atan2(mouseOffset.y, mouseOffset.x) + 90;
			sword.localEulerAngles = new Vector3(0, 0, angle);
		}

		int amt = Mathf.FloorToInt(swordVel.magnitude * 0.1f);
		weaponScript.DetectCollision(Mathf.FloorToInt(amt));

		swordPivot.LookAt(calcCam.ScreenToWorldPoint(mousePos));
	}

	public void Die()
	{
		alive = false;
		
		feetAnimator.SetBool("Moving", false);
		bodyAnimator.SetBool("Moving", false);

		swordPivot.localEulerAngles = new Vector3(0, 90, 0);
		swordAnimator.enabled = true;
		swordAnimator.SetTrigger("Death");
		feetAnimator.SetTrigger("Death");
		bodyAnimator.SetBool("Death", true);
		rigidbody.velocity = -Vector3.up;

		mngr.GameOver();

		StartCoroutine(DisableDelay());
	}

	public void Flinch()
	{
		if(!flinch)
		{
			audioSource.Play();
			flinch = true;
			feetAnimator.SetTrigger("Flinch");
			bodyAnimator.SetTrigger("Flinch");
			bodyAnimator.SetBool("Moving", false);
			feetAnimator.SetBool("Moving", false);

			StartCoroutine(FlinchDelay());
		}
	}

	private IEnumerator FlinchDelay()
	{
		yield return new WaitForSeconds(0.25f);
		flinch = false;
		bodyAnimator.SetBool("Flinch", false);
	}

	private IEnumerator DisableDelay()
	{
		yield return new WaitForSeconds(1.0f);
		if(IsGrounded())
		{
			rigidbody.isKinematic = true;
			collider.enabled = false;
		}
		else
		{
			StartCoroutine(DisableDelay());
		}
	}

	public void DoDamage(int amt)
	{
		health -= amt;
		UpdateHealth();
		weaponScript.ResetCombo();

		if(health <= 0)
		{
			health = 0;
			Die ();
		}
		else
		{
			Flinch();
		}
	}

	private void UpdateHealth()
	{
		healthDisplay.text = "Health: " + Mathf.RoundToInt((float)health / 500 * 100) + "%";
	}

	public void AddToScore(int amt)
	{
		score += amt;
		kills++;
		UpdateScore();
	}

	private void UpdateScore()
	{
		scoreDisplay.text = "Score: " + score;
		mngr.UpdateSpawnCap(kills);
	}
}