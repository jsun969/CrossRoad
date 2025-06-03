using System.Collections;
using System.Collections.Generic;
using BNG;
using UnityEngine;

public class InstantiateObjects : MonoBehaviour
{
    public GameObject prefabObject;

    [Tooltip("Controls how many balls will spawn.")]
    [Range(1, 500)]
    public int spawnAmount = 100;
    [Tooltip("Controls how quickly balls will spawn.")]
    [Range(0, .5f)]
    public float spawnSpeed = .08f;
    [Tooltip("Controls how fast balls will be shot out.")]
    [Range(0, 1000f)]
    public int thrust = 200;
    [Tooltip("Controls size of the arc balls can be thrown in.")]
    [Range(0, 90)]
    public int directionRange = 10;
    [Tooltip("Controls size of the balls.")]
    [Range(.1f, 15)]
    public float scale = 1;

    public bool endless = false;
    
    
    private bool _spawning = false;

    private float _timer = 0.0f;
    private int _spawned = 0;

    private Vector3 _startRotation;
    
    // Start is called before the first frame update
    void Start()
    {
        _startRotation = transform.localEulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if (_spawning && (_spawned < spawnAmount || endless))
        {
            _timer += Time.deltaTime;
            if (_timer > spawnSpeed)
            {
                //Randomises the direction of the spawner to give the balls an arc the can be shot from
                Vector3 changeRot = _startRotation;
                changeRot.x = Random.Range(changeRot.x + directionRange, changeRot.x - directionRange);
                changeRot.y = Random.Range(changeRot.y + directionRange, changeRot.y - directionRange);
                transform.localEulerAngles = changeRot;
                
                //Spawns the ball
                GameObject ball = Instantiate(prefabObject, this.transform.position, new Quaternion(0,0,0,0));
                
                //Scales the ball
                ball.transform.localScale *= scale;
                
                //Shoots the ball
                ball.GetComponent<Rigidbody>().AddForce(transform.forward * thrust);
                
                //Changes the pitch of the ball bounce based on size
                var num = 1f;
                if (scale > 1)
                {
                    num = Map(1, 15, 1f, .5f, scale);
                    ball.GetComponent<PlayAudioOnCollision>().scaleValue = num;
                }
                else if(scale < 1f)
                {
                    num = Map(.1f, 1, 2.5f, 1f, scale);
                    ball.GetComponent<PlayAudioOnCollision>().scaleValue = num;
                }
                _spawned++;
                _timer = 0;
            }
        }
        else if (_spawning && !endless)
        {
            _spawned = 0;
            _timer = 0;
            _spawning = false;
        }
    }

    public void SpawnObjects()
    {
        _spawning = true;
    }

    public float Map(float OldMin, float OldMax, float NewMin, float NewMax, float OldValue){
 
        float OldRange = (OldMax - OldMin);
        float NewRange = (NewMax - NewMin);
        float NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;
 
        return(NewValue);
    }
}
