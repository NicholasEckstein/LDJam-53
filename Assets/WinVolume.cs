using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinVolume : MonoBehaviour
{
    bool win = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (win)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            win = true;
            StartCoroutine(WinCR());
        }
    }


    private IEnumerator WinCR()
    {
        yield return new WaitForSeconds(1.5f);

        if(GameManager.Instance.CurrentLevel.LevelNum == 1)
        {
            PlayerPrefs.SetInt(GameManager.Level1Str, GameManager.LevelComplete);
            PlayerPrefs.SetInt(GameManager.Level2Str, GameManager.LevelUnlocked);
        }
        else if(GameManager.Instance.CurrentLevel.LevelNum == 2)
        {
            PlayerPrefs.SetInt(GameManager.Level2Str, GameManager.LevelComplete);
        }

        LoadingUI.ShowLoadingScreen();
        GameManager.Instance.SetNextPhase(new PostGamePhase(win));
    }
}
