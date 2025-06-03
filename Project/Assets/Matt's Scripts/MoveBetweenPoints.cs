using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class MoveBetweenPoints : MonoBehaviour
{
    public enum Mode {
        [Tooltip("Will stop moving upon reaching an end")]
        Stop,
        [Tooltip("Will teleport to other end of the line upon reaching an end")]
        Wrap,
        [Tooltip("Will change direction upon reaching an end")]
        PingPong
    }

    public Mode movementMode = Mode.PingPong;
    public float movementSpeed = 1f;
    public bool movingTowardsB;
    public Transform pointA;
    public Transform pointB;

    public UnityEvent OnReachEnd;

    private void Update()
    {
        float distanceCanMove = Time.deltaTime * movementSpeed;
        var P0 = pointA.position;
        var P1 = pointB.position;
        var P = transform.position;

        float length = (P1 - P0).magnitude;
        float d = ClosestDistanceAlongLine(P0, P1, P);

        do
        {
            float target = movingTowardsB ? length : 0;
            float distanceToEnd = Mathf.Abs(target - d);

            // check if we can reach an end
            if (distanceCanMove >= distanceToEnd)
            {
                if (distanceToEnd > Mathf.Epsilon)
                {
                    OnReachEnd.Invoke();
                }
                
                switch (movementMode)
                {
                    case Mode.Stop:
                        d = Mathf.MoveTowards(d, target, distanceCanMove);
                        distanceCanMove = 0;
                        break;
                    case Mode.Wrap:
                        d = Mathf.Repeat(d + (movingTowardsB ? distanceCanMove : -distanceCanMove), length);
                        distanceCanMove = 0;
                        break;
                    case Mode.PingPong:
                        d = target;
                        distanceCanMove -= distanceToEnd;
                        movingTowardsB = !movingTowardsB;
                        break;
                }
            }
            else
            {
                d = Mathf.MoveTowards(d, target, distanceCanMove);
                distanceCanMove -= distanceCanMove;
            }
        } while (distanceCanMove > 0);

        transform.position = P0 + (P1 - P0).normalized * d;
    }
    
    // closest point to P on the line defined between P0 and P1
    private Vector3 ClosestPointOnLine(in Vector3 P0, in Vector3 P1, in Vector3 P)
    {
        Vector3 lineVector = P1 - P0;
        float lineSqrMag = Vector3.Dot(lineVector, lineVector);
        float t = Vector3.Dot(P - P0, lineVector) / lineSqrMag;
        return P0 + t * lineVector;
    }
    
    // distance along line (towards P0) of closest point on line from P0
    private float ClosestDistanceAlongLine(in Vector3 P0, in Vector3 P1, in Vector3 P)
    {
        Vector3 lineVector = P1 - P0;
        float lineSqrMag = Vector3.Dot(lineVector, lineVector);
        float t = Vector3.Dot(P - P0, lineVector) / lineSqrMag;
        return t * Mathf.Sqrt(lineSqrMag);
    }

    // percentage along line the closest point from P is from P0 to P1
    private float ClosestPercentageAlongLine(in Vector3 P0, in Vector3 P1, in Vector3 P)
    {
        Vector3 lineVector = P1 - P0;
        float lineSqrMag = Vector3.Dot(lineVector, lineVector);
        float t = Vector3.Dot(P - P0, lineVector) / lineSqrMag;
        return t;
    }

    public void SetMoveToA()
    {
        movingTowardsB = false;
    }

    public void SetMoveToB()
    {
        movingTowardsB = true;
    }

    public void SetOppositeDirection()
    {
        movingTowardsB = !movingTowardsB;
    }

    private void OnDrawGizmos()
    {
        if (!(pointA && pointB))
            return;
        
        if (Application.isPlaying)
        {
            var p = ClosestPointOnLine(pointA.position, pointB.position, transform.position);
            if (movingTowardsB)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawLine(p, pointA.position);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(p, pointB.position);
            }
            else
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(p, pointA.position);
                Gizmos.color = Color.black;
                Gizmos.DrawLine(p, pointB.position);
            }
        }
        else
            Gizmos.DrawLine(pointA.position, pointB.position);
    }
}
