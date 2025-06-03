using UnityEngine;

public class LookAt : MonoBehaviour 
{
	public Transform target;
	//For exact snap rotation:
	//comment out 'public float speed = 5f;', remove comment on the '//transform.rotation = rotation;', then
	//comment out 'transform.rotation - Quaternion.Lerp(transform.rotation, rotation, speed * Time.deltatime);'

	//Speed indicates snap speed - see object component for option
	public float speed = 5f;

	private void Update()
	{
		Vector3 direction = target.position - transform.position;
		Quaternion rotation = Quaternion.LookRotation(direction);
		//transform.rotation = rotation;
		transform.rotation = Quaternion.Lerp(transform.rotation, rotation, speed * Time.deltaTime);

	}
}
// This script will make objects rotate to the target object.