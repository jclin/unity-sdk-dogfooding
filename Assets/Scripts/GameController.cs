using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private uint score;

    private void Start()
    {
        score = 0;
    }

    public void Random()
    {
        Debug.Log($"Random val={SkillzCrossPlatform.Random.Value()}");
    }

    public void PlusOne()
    {
        score += 1;
        SkillzCrossPlatform.UpdatePlayersCurrentScore(score);
    }

    public void Abort()
    {
        SkillzCrossPlatform.AbortMatch();
    }
}
