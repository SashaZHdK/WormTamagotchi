﻿using UnityEngine;
using System.Collections;
using FMOD;
using FMODUnity;
using UnityEngine.UI;


public class TargetPositionController : MonoBehaviour
{
	public float speed = 10f;
	private float currentSpeed;
	Grid grid;
	public Pathfinding pathfinfingScript;

	public GameObject Sandfootprint;
	public GameObject Waterfootprints;

	public GameObject apple;
	private int appleCount = 0;
	public Text score;
	private GameObject currentApple;
	// Use this for initialization


	[HideInInspector] public bool onWater = false;

	public float timerForFootsteps = 0.8f;
	private float timer = 0f;



	void Start ()
	{
		
		grid = GameObject.Find ("A*").GetComponent<Grid> ();
		currentApple = Instantiate(apple, new Vector3 (Random.Range(10f, 150f), apple.transform.position.y, Random.Range (10f, 100f)), apple.transform.rotation) as GameObject;

	}
	
	// Update is called once per frame
	void Update ()
	{
		float moveHorizontal = Input.GetAxisRaw ("Horizontal");
		float moveVertical = Input.GetAxisRaw ("Vertical");

		Vector3 movement = new Vector3 (-moveHorizontal, 0.0f, -moveVertical);   //flip for the projector
		if (movement != Vector3.zero) {
			GetComponent<Rigidbody> ().rotation = Quaternion.LookRotation (movement);

			GetComponentInChildren<Animator> ().SetBool ("Walk", true);
			timer += Time.deltaTime;
			if (timer > timerForFootsteps) {
				if (!onWater) {
					GameObject foots =	Instantiate (Sandfootprint, gameObject.transform) as GameObject;
					foots.transform.SetParent (null);
					GetComponent<StudioEventEmitter> ().Play ();
				} else {
					GameObject foots =	Instantiate (Waterfootprints, gameObject.transform) as GameObject;
					foots.transform.SetParent (null);
				}
					
					timer = 0f;
				
			}
		} else {
			
			GetComponentInChildren<Animator> ().SetBool ("Walk", false);
			timer = 0f;
		
		}





		GetComponent<Rigidbody> ().velocity = movement * currentSpeed;




		SandCheck ();

		if (onWater)
		{

			GetComponentInChildren<Animator> ().SetBool ("OnWater", true);



		}
		else{

			GetComponentInChildren<Animator> ().SetBool ("OnWater", false);


		}


		 


	}








	void SandCheck ()
	{
		Node.TerrainLayer layer = grid.PositionTarget (transform.position).layer;
		if (layer == Node.TerrainLayer.Sand) {
			pathfinfingScript.DodoOnSand = true;
			currentSpeed = speed; 
			onWater = false;

		} else if (layer == Node.TerrainLayer.Water) {
			onWater = true;

			currentSpeed = speed / 2f;

		} else if (layer == Node.TerrainLayer.Grass) {

			currentSpeed = speed;
			onWater = false;

		} else if (layer == Node.TerrainLayer.Mountain) {

			currentSpeed = speed / 2f;
			onWater = false;
		}
	}


	void SetApple(){
		currentApple.transform.position = new Vector3 (Random.Range(10f, 150f), apple.transform.position.y, Random.Range (10f, 100f));
	}


	void OnTriggerEnter(Collider coll){
		
		if (coll.CompareTag("Apple")){
			if (appleCount < 3) {
				appleCount++;
				SetApple ();
				score.text = "Apples to eat \n \n"  + (4 - appleCount).ToString ();
			} else {
				score.text = "Dodo wins!";
				Destroy (currentApple);
			}
	}
			}
			
}
