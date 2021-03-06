﻿namespace StateManagement
{
	// This is a demo state
	// Wrap the state inside a 'StateManagement' namespace or include it with 'using StateManagement;'

	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using FMOD;
	using FMODUnity;
	// Define the type(s) of the state
	// This state will be available for the following object types: 'Worm' and 'Dodo'
	[StateType(State.Worm | State.Dodo)]
	public class WormNotAtSand : BaseState   // <- You must inherit from 'BaseState'  //Aye, Captain!
	{
		public float DeadTimer = 4.5f;
		private float timer = 0f;
		Grid grid;
		// Calling base constructor
		public WormNotAtSand(object caller) : base(caller) { }   // <- Base constructor must be called!
		// Initialization happens before rendering first frame

		// Override update method
		public override void Start()
		{
			// Do some dead timer
			grid = GameObject.Find("A*").GetComponent<Grid> ();

			baseObject.GetComponent<Pathfinding> ().speed = 6f;
			baseObject.GetComponentInChildren<Animator> ().SetBool ("Walk", false);
			baseObject.GetComponentInChildren<Animator> ().SetBool ("Chase", false);
			baseObject.GetComponentInChildren<Animator> ().SetBool ("Die", true);


	
		}



		public override void Update ()
		{
			timer += Time.deltaTime;


			UnityEngine.Debug.Log ("scorpio dies");
			if (timer > 0.8f){

			if (!baseObject.GetComponent<Pathfinding> ().scorpioCry.GetComponent<StudioEventEmitter> ().IsPlaying()) {
				baseObject.GetComponent<Pathfinding> ().scorpioCry.GetComponent<StudioEventEmitter> ().Play ();
			}

			}

			if (timer > DeadTimer) {
				
				UnityEngine.Debug.Log ("Game Over Skorpion!");
				baseObject.GetComponent<StateMachine> ().ChangeState ("ScorpioDead");
			}
 

			/*if (grid.PositionTarget (baseObject.transform.position).layer == Node.TerrainLayer.Sand) {  //worm is at the sand layer

				baseObject.GetComponent<Pathfinding> ().onWandering = true;
				baseObject.GetComponent<Pathfinding> ().WormNotAtSand = false;

			}*/
		}


		public override void OnDisable ()
		{
			baseObject.GetComponentInChildren<Animator> ().SetBool ("Die", false);
			baseObject.GetComponent<Pathfinding> ().scorpioCry.GetComponent<StudioEventEmitter> ().Stop ();
			timer = 0f;
		}

		// You can also override other 'MonoBehaviour' methods:
		/*
        public override void Awake() {}         // Called on loading state
        public override void Start() {}         // Called in the first frame this state is active
        public override void LateUpdate() {}    // Called at the end of every frame
        public override void FixedUpdate() {}   // Called every physics step
        public override void OnDisable() {}     // Called before this state gets unloaded
        */
	}
}
