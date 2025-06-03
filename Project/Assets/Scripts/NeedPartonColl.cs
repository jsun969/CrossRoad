using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedPartonColl : MonoBehaviour {

	public Transform spurt;

	// Use this for initialization
	void Start () {
		spurt.GetComponent<ParticleSystem> ().Stop ();
	}

	void OnTriggerEnter()
	{
		spurt.GetComponent<ParticleSystem> ().Play ();	
		//StartCoroutine (stopSpurts ());

	}

	void OnTriggerExit()
	{
		spurt.GetComponent<ParticleSystem> ().Stop ();
	}
	//IEnumerator stopSpurts()

	// Update is called once per frame
	void Update () {

	}
}
