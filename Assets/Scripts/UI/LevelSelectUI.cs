using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectUI : UIPrefab
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (GameManager.Instance.DialogueRunner.IsDialogueRunning)
                return;

            LoadingUI.ShowLoadingScreen();
            GameManager.Instance.SetNextPhase(new PlayPhase());
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LoadingUI.ShowLoadingScreen();
            GameManager.Instance.SetNextPhase(new MainMenuPhase(GameManager.Instance.CurrentPhase));
        }
    }
}
