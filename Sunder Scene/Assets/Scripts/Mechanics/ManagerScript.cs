using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ManagerScript : MonoBehaviour {

	// References Set in Inspector
	public Transform player;

	// Prefabs
	public GameObject joePrefab;
	public GameObject topPrefab;
	public GameObject hammerPrefab;

	// Enemy Managerment variables
	private List<Transform> enemies;
	private int spawnCap;
	private bool canSpawn;

	private bool startGame;
	private bool showHowTo;
	private string instructions;
	private bool gameOver;

	void Start()
	{
		startGame = false;
		showHowTo = false;
		canSpawn = true;
		gameOver = false;
		spawnCap = 20;
		enemies = new List<Transform>();

		CreateInstructions();
	}

	void Update()
	{
		if(startGame)
		{
			if(enemies.Count < spawnCap && canSpawn)
			{
				int num = Random.Range(0, spawnPoints.Length);
				StartCoroutine(SpawnDelay(spawnPoints[num].position));
			}
		}
	}

	void OnGUI()
	{
		if(!startGame)
		{
			GUI.Box(new Rect(Screen.width * 0.4f, Screen.height * 0.25f, Screen.width * 0.2f, Screen.height * 0.5f), "Sunder Scene");
			if(!showHowTo)
			{
				if(GUI.Button(new Rect(Screen.width * 0.45f, Screen.height * 0.5f - Screen.width * 0.1f, Screen.width * 0.1f, Screen.width * 0.05f), "Play"))
				{
					startGame = true;
				}
				if(GUI.Button(new Rect(Screen.width * 0.45f, Screen.height * 0.5f - Screen.width * 0.025f, Screen.width * 0.1f, Screen.width * 0.05f), "How To Play"))
				{
					showHowTo = true;
				}
				if(Application.platform != RuntimePlatform.OSXWebPlayer && Application.platform != RuntimePlatform.WindowsWebPlayer)
				{
					if(GUI.Button(new Rect(Screen.width * 0.45f, Screen.height * 0.5f + Screen.width * 0.05f, Screen.width * 0.1f, Screen.width * 0.05f), "Quit"))
					{
						Application.Quit();
					}
				}
			}
			else
			{
				GUI.Label(new Rect(Screen.width * 0.42f, Screen.height * 0.3f, Screen.width * 0.8f, Screen.height * 0.6f), instructions);
				if(GUI.Button(new Rect(Screen.width * 0.45f, Screen.height * 0.5f + Screen.width * 0.05f, Screen.width * 0.1f, Screen.width * 0.05f), "Back"))
				{
					showHowTo = false;
				}
			}
		}
		else if(gameOver)
		{
			//GUI.Box(new Rect(Screen.width * 0.4f, Screen.height * 0.25f, Screen.width * 0.2f, Screen.height * 0.5f), "Game Over");
			if(GUI.Button(new Rect(Screen.width * 0.45f, Screen.height * 0.5f - Screen.width * 0.025f, Screen.width * 0.1f, Screen.width * 0.05f), "Retry"))
			{
				Application.LoadLevel(0);
			}
		}
	}

	public bool GameStarted
	{
		get { return startGame; }
	}

	public List<Transform> Enemies
	{
		get { return enemies; }
	}

	private void SpawnAJoe(Vector3 pos)
	{
		GameObject betterJoe = Instantiate(joePrefab,
		                                   pos,
		                                   Quaternion.identity) as GameObject;
		betterJoe.GetComponent<JoeScript>().Initialize(this, player);
		enemies.Add(betterJoe.transform);
	}

	private void SpawnATop(Vector3 pos)
	{
		GameObject betterJoe = Instantiate(topPrefab,
		                                   pos,
		                                   Quaternion.identity) as GameObject;
		betterJoe.GetComponent<TopScript>().Initialize(this, player);
		enemies.Add(betterJoe.transform);
	}
	private void SpawnAHammer(Vector3 pos)
	{
		GameObject betterJoe = Instantiate(hammerPrefab,
		                                   pos,
		                                   Quaternion.identity) as GameObject;
		betterJoe.GetComponent<HammerScript>().Initialize(this, player);
		enemies.Add(betterJoe.transform);
	}

	public void RemoveEnemy(Transform enemy)
	{
		enemies.Remove(enemy);
	}

	public Transform[] spawnPoints;

	private IEnumerator SpawnDelay(Vector3 pos)
	{
		canSpawn = false;
		yield return new WaitForSeconds(.25f);
		SpawnAJoe(pos);
		canSpawn = true;
	}

	public void UpdateSpawnCap(int kills)
	{
		if(kills % 20 == 0)
		{
			int num = Random.Range(0, spawnPoints.Length);
			SpawnAHammer(spawnPoints[num].position);
			spawnCap++;
		}
		else if(kills % 5 == 0)
		{
			int num = Random.Range(0, spawnPoints.Length);
			SpawnATop(spawnPoints[num].position);

			int chance = Random.Range(0, 100);

			if(chance < 40)
			{
				SpawnATop(spawnPoints[(num+2)%4].position);
			}
			if(chance < 20)
			{
				SpawnATop(spawnPoints[(num+1)%4].position);
			}
			if(chance < 10)
			{
				SpawnATop(spawnPoints[(num+3)%4].position);
			}
		}
	}

	public void GameOver()
	{
		gameOver = true;
		Screen.lockCursor = false;
	}

	private void CreateInstructions()
	{
		instructions = "WASD/Arrow Keys to move";
		instructions += "\n";
		instructions += "\nClick and move the mouse to swing your sword";
		instructions += "\n";
		instructions += "\nP to pause";
		instructions += "\n";
		instructions += "\nEsc to unlock the cursor";
		instructions += "\n";
		instructions += "\nHit enemies with your sword to kill them";
		instructions += "\n";
		instructions += "\nHitting enemies also increases your combo";
		instructions += "\n";
		instructions += "\nHigh combos get you mad points";
		instructions += "\n";
		instructions += "\nDon't Die";
	}
}
