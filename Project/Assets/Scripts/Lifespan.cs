using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lifespan : MonoBehaviour
{
    private float _lifeTime = 7f;

    private float _timer = 0f;

    // Update is called once per frame
    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer > _lifeTime)
        {
            Destroy(this.gameObject);
        }
    }
}
