using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkullReset : MonoBehaviour
{
    private bool _firstOn = true;
    private List<Vector3> _positions = new List<Vector3>();
    private List<Quaternion> _rotations = new List<Quaternion>();
    private List<GameObject> _pieces = new List<GameObject>();

    private void OnEnable()
    {
        if (_firstOn)
        {
            Transform[] ts = gameObject.GetComponentsInChildren<Transform>();
            foreach (var transform1 in ts)
            {
                if (transform1.GetComponent<Rigidbody>())
                {
                    _pieces.Add(transform1.gameObject);
                    _positions.Add(transform1.position);
                    _rotations.Add(transform1.rotation);
                }
            }

            _firstOn = false;
        }
        else
        {
            var i = 0;
            foreach (var piece in _pieces)
            {
                piece.transform.position = _positions[i];
                piece.transform.rotation = _rotations[i];
                piece.GetComponent<Rigidbody>().Sleep();
                i++;
            }
        }
    }
}
