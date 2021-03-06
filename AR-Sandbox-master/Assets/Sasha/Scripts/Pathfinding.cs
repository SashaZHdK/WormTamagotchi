﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using StateManagement;
using UnityEngine.UI;
using FMOD;
using FMODUnity;

public class Pathfinding: MonoBehaviour {

	StudioEventEmitter soundScript;


	public Text score;

	public GameObject sandTrail;
	public ParticleSystem boom;

	[HideInInspector] public bool DodoOnSand = true;

	Grid grid;
	public float speed = 10f;
	public float attackCooldown = 0.5f;
	public bool TestGrid = false;
	public GameObject lost;
	public GameObject found;

	public Transform  targetDodo;
	[HideInInspector] public Vector3 targetWandering;
	protected Transform seeker;

	public GameObject scorpioCry;


	public bool onWandering = false;
	public bool stayOnPlace = false;
	public bool WormNotAtSand = false;

	public bool followDodo = false;
	private float timeCount = 0f;
	public bool deadDodo = false;
	public bool deadScorpio = false;

	void Awake(){
		soundScript = GetComponent<StudioEventEmitter> ();
		RandomWanderingTarget ();
		seeker = transform;
		grid = GameObject.Find("A*").GetComponent<Grid> ();
	}

	void Update(){
		//FillBoxSand Sand (
		if (TestGrid){
		FillBoxSand (0, 0, 40, 30);
		FillBoxWater(0, 10,20, 20);
		}

		if (!deadScorpio){

		if (WormNotAtSand){
		if (grid.PositionTarget (transform.position).layer == Node.TerrainLayer.Sand) {  //worm is at the sand layer

			onWandering = true;
			WormNotAtSand = false;
			GetComponent<StateMachine> ().ChangeState ("Wandering");
		  }
		}

		if (!stayOnPlace) {
			if (!GetComponent<StudioEventEmitter> ().IsPlaying ()) {
				sandTrail.SetActive (true);
				GetComponent<StudioEventEmitter> ().Play ();
			}
			if (onWandering) {
				FindPath (seeker.position, targetWandering);
			}



		
		} else
			sandTrail.SetActive (false);

		if (DodoOnSand && !deadDodo) {
			FindPath (seeker.position, targetDodo.position);
			if (onWandering) {
				//Debug.Log ("Searching of two paths");
			}
		}
	
		if (!WormNotAtSand && grid.PositionTarget (transform.position).layer != Node.TerrainLayer.Sand ) {
			WormNotAtSand = true;
			GetComponent<StateMachine> ().ChangeState ("WormNotAtSand");

		} else {
			
			//WormNotAtSand = false;

		}

		// Count time
		if (timeCount <= attackCooldown)
			timeCount += Time.deltaTime;
	  }
	}
    
	void FillBoxGrass(int x1, int y1, int x2, int y2) {
		for (int i = x1; i <= x2; ++i)
			for (int j = y1; j <= y2; ++j)
				grid.SetNode (i, j, Node.TerrainLayer.Grass);
	}

	void FillBoxWater(int x1, int y1, int x2, int y2) {
		for (int i = x1; i <= x2; ++i)
			for (int j = y1; j <= y2; ++j)
				grid.SetNode (i, j, Node.TerrainLayer.Water);
	}

	void FillBoxMountains(int x1, int y1, int x2, int y2) {
		for (int i = x1; i <= x2; ++i)
			for (int j = y1; j <= y2; ++j)
				grid.SetNode (i, j, Node.TerrainLayer.Mountain);
	}
	void FillBoxSand(int x1, int y1, int x2, int y2) {
		for (int i = x1; i <= x2; ++i)
			for (int j = y1; j <= y2; ++j)
				grid.SetNode (i, j, Node.TerrainLayer.Sand);
	}

	public void DodoIsDead(){
		deadDodo = true;
		DodoOnSand = false;
		followDodo = false;
		GetComponent<StateMachine> ().ChangeState ("DodoLost");  
		stayOnPlace = true;
		onWandering = true;
	}





	void FindPath(Vector3 startPos, Vector3 targetPos){
		Node startNode = grid.PositionTarget (startPos);
		Node targetNode = grid.PositionTarget (targetPos);

        // Failsave - Julian 21/11/2017
        if((startNode == null) || (targetNode == null))
            return;

		HashSet<Node> openSet = new HashSet<Node> ();
		HashSet<Node> closedSet = new HashSet<Node> ();


		openSet.Add (startNode);


		int count1 = 0;
		int count2 = 0;


		while (openSet.Count > 0) {
			
			Node currentNode = null;
			foreach (Node cur in openSet ) {
				
				if (currentNode == null || cur.fCost < currentNode.fCost){// || cur.fCost == currentNode.fCost && cur.hCost < currentNode.hCost) {
					currentNode = cur;
				}
			}

			openSet.Remove (currentNode);
			closedSet.Add (currentNode);


			if (currentNode == targetNode) {  //found the path to target

				if (targetPos == targetDodo.position) {
					followDodo = true;
					onWandering = false;
					GetComponent<StateMachine> ().ChangeState ("FollowTarget");
					RetracePath (startNode, targetNode, true);

					return;
				}

				else {
					
					RetracePath (startNode, targetNode, false);

					return;
				}
			}

			foreach (Node neighbour in grid.GetNeighbours(currentNode)) {
				if (!(neighbour.layer == Node.TerrainLayer.Sand) || closedSet.Contains (neighbour)) {   //Moutain layer is set up for dubugging
					continue;
				}
				int newMovementCostToNeighbour = currentNode.gCost + GetDistance (currentNode, neighbour);

				if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains (neighbour)) {
					neighbour.gCost = newMovementCostToNeighbour;
					neighbour.hCost = GetDistance (neighbour, targetNode);

					neighbour.parent = currentNode;



					openSet.Add (neighbour);

				}

			}

		}

		//can't find the path to target

		if (onWandering && targetPos != targetDodo.position) {
			GetComponent<StateMachine> ().ChangeState ("OnWanderingIdle");
			stayOnPlace = true;

		} else if (!onWandering) {
			DodoOnSand = false;
			followDodo = false;
			GetComponent<StateMachine> ().ChangeState ("DodoLost");  
			stayOnPlace = true;
			onWandering = true;
		} 
		






	}

	public void RandomWanderingTarget(){
		
		targetWandering = new Vector3 (Random.Range(4f, 150f), transform.position.y, Random.Range (4f, 105f));
	}





	void RetracePath(Node startNode, Node endNode, bool dodoPath){

		List<Node> path = new List<Node>();
		Node currentNode = endNode;

		while (currentNode != startNode){
			path.Add (currentNode);
			currentNode = currentNode.parent;
		}

		path.Reverse ();

		if (dodoPath) {

			grid.pathForDodo = path;
		} else {
			grid.pathForWandering = path;
		}

		// Edit Julian - 3 grid nodes offset, so that scorpion attacks not only if it lies exactly over the dodo,
		// but a little bit earlier.
		if ((path.Count - 3) > 0) {
			seeker.position = Vector3.MoveTowards (seeker.position, new Vector3 (path [0].worldPosition.x, seeker.position.y, path [0].worldPosition.z), speed * Time.deltaTime); 
			Vector3 targetDir =new Vector3 (path [0].worldPosition.x, seeker.position.y, path [0].worldPosition.z) - transform.position;
			float step = 3f * Time.deltaTime;
			Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0F);
			transform.rotation = Quaternion.LookRotation(newDir);
		} else {   //found the target
			if (onWandering) {
				GetComponent<StateMachine> ().ChangeState ("OnWanderingIdle");
				stayOnPlace = true;


			} else {
				//Debug.Log ("The worm eats the dodo. Like, last worm has eaten the last dodo");

				if (timeCount >= attackCooldown)
				{
					GetComponentInChildren<Animator> ().SetTrigger ("Attack");
					timeCount = 0f;
				}
			}
		}
	}

	// Animation event - when scorpion has stung the dodo...
	public void Stung() {
		//score.text = "Scorpio rulezzzz!";
	}


	int GetDistance(Node nodeA, Node nodeB){

		int distX = Mathf.Abs (nodeA.gridX - nodeB.gridX);
		int distY = Mathf.Abs (nodeA.gridY - nodeB.gridY);

		if (distX > distY) 
			return 14 * distY + 10 * (distX - distY);   //1.4 is a square root of 2
		return 14 * distX + 10 * (distY - distX);
		



	}

}
