using UnityEngine;

public class IncreaseSpeed : MonoBehaviour
{
    public MoveBetweenPoints betweenPoints;
    public ScoreKeeper scoreKeeper;
    public float increaseAmount = 1f;

    public void AddSpeed()
    {
        betweenPoints.movementSpeed += increaseAmount;
    }
}
