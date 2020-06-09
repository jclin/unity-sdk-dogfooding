using System.Collections;
using System.Collections.Generic;
using SkillzSDK;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkillzLauncher : MonoBehaviour, SkillzMatchDelegate
{
    public void Launch()
    {
        SkillzCrossPlatform.LaunchSkillz(this);
    }

    public void OnMatchWillBegin(Match matchInfo)
    {
        SceneManager.LoadScene("GameScene");

    }

    public void OnSkillzWillExit()
    {
    }
}
