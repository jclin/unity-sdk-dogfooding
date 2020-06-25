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

    public void ExecuteAPIs()
    {
        Debug.Log("[Skillz] SkillzCrossPlatform.AddMetadataForMatchInProgress");
        SkillzCrossPlatform.AddMetadataForMatchInProgress("{ \"derp\": \"derp\" }", true);

        // N/A: RTTB APIs
        // Debug.Log($"Connected Player count={SkillzCrossPlatform.GetConnectedPlayerCount()}");
        // Debug.Log($"Opponent ID={SkillzCrossPlatform.GetCurrentOpponentPlayerId()}");
        // var playerId = SkillzCrossPlatform.GetCurrentPlayerId();
        // Debug.Log($"Player ID={playerId}");

        Debug.Log($"Match info={SkillzCrossPlatform.GetMatchInfo()}");

        var matchRules = SkillzCrossPlatform.GetMatchRules();
        foreach (var rule in matchRules)
        {
            Debug.Log($"Match rule={rule.ToString()}");
        }

        Debug.Log($"Player info={SkillzCrossPlatform.GetPlayer()}");
        // Debug.Log($"Server time={SkillzCrossPlatform.GetServerTime()}");

        Debug.Log("Setting audio volume=0.9");
        SkillzCrossPlatform.setSFXVolume(0.9f);
        Debug.Log($"Audio volume={SkillzCrossPlatform.getSFXVolume()}");

        Debug.Log("Setting music volume=0.9");
        SkillzCrossPlatform.setSkillzMusicVolume(0.9f);
        Debug.Log($"Music volume={SkillzCrossPlatform.getSkillzMusicVolume()}");

        // Debug.Log($"Time left for reconnection={SkillzCrossPlatform.GetTimeLeftForReconnection(playerId)}");

        // Debug.Log($"Match completed?={SkillzCrossPlatform.IsMatchCompleted()}");
        Debug.Log($"Match completed?={SkillzCrossPlatform.IsMatchInProgress()}");

    }

    public void SubmitScore()
    {
        SkillzCrossPlatform.ReportFinalScore(score);
    }

    public void Abort()
    {
        SkillzCrossPlatform.AbortMatch();
    }
}
