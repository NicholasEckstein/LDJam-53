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
            StartCoroutine(LoseCR());
        }
    }


    private IEnumerator LoseCR()
    {
        yield return new WaitForSeconds(1.5f);
        LoadingUI.ShowLoadingScreen();
        GameManager.Instance.SetNextPhase(new PostGamePhase(win));
    }
}
