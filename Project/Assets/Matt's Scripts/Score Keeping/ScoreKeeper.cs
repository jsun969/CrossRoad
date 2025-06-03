using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ScoreKeeper : MonoBehaviour
{
    [Serializable]
    public class ScoreEvent
    {
        [Tooltip("Passing this score will cause the event to be invoked")]
        public int scoreRequired;
        [Tooltip("Whether this event will be retriggered if it is passes again")]
        public bool canBePassedMultipleTimes;
        [NonSerialized] public bool hasBeenPassed;
        public UnityEvent OnScorePassed;
    }
    
    public int score = 0;
    public bool mustBePositive = true;
    public UnityEvent OnScoreChange;
    public List<ScoreEvent> ScoreEvents;

    [Header("Optional")]
    [Tooltip("Text will be changed to say the score if provided")]
    public TMP_Text optionalText;
    [Min(0)] public int minDigits = 0;

    public void ResetScore(int newScore)
    {
        if (newScore == score)
            return;

        score = newScore;

        if (optionalText)
        {
            optionalText.text = score.ToString().PadLeft(minDigits, '0');
        }
        
        OnScoreChange.Invoke();

        for (int i=0; i<ScoreEvents.Count; i++)
        {
            ScoreEvents[i].hasBeenPassed = false;
        }
    }

    // invoke this function through another script with the amount of score you wish to add
    public void AddToScore(int amount)
    {
        if (amount == 0)
            return;
        
        int previousScore = score;
        score += amount;

        if (mustBePositive && score < 0)
            score = 0;

        if (optionalText)
        {
            optionalText.text = score.ToString().PadLeft(minDigits, '0');
        }
        
        OnScoreChange.Invoke();

        for (int i=0; i<ScoreEvents.Count; i++)
        {
            var e = ScoreEvents[i];
            
            if (score >= e.scoreRequired && previousScore < e.scoreRequired)
            {
                if (!e.canBePassedMultipleTimes && e.hasBeenPassed)
                    continue;

                e.hasBeenPassed = true;
                e.OnScorePassed.Invoke();
            }
        }
        
    }
}
