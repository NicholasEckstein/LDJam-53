using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuUI : UIPrefab
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (GameManager.Instance.TogglePaused())
            {
                GameManager.CloseUI(this);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LoadingUI.ShowLoadingScreen();
            GameManager.Instance.SetNextPhase(new MainMenuPhase(GameManager.Instance.CurrentPhase));
        }
    }
}
