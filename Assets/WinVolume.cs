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
        PlayerPrefs.SetInt(GameManager.Level1Str, GameManager.LevelComplete);
        PlayerPrefs.SetInt(GameManager.Level2Str, GameManager.LevelUnlocked);
        LoadingUI.ShowLoadingScreen();
        GameManager.Instance.SetNextPhase(new PostGamePhase(win));
    }
}
