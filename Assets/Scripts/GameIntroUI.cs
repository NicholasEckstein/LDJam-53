using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameIntroUI : UIPrefab
{
    [SerializeField]
    private Image m_fadeImage;

    [SerializeField]
    private float m_fadeInSpeed;

    [SerializeField]
    private float m_fadeOutSpeed;

    private Coroutine m_fadeInCR;
    private bool m_fadeOutDone = false;

    private void Start()
    {
        StartCoroutine(FadeOutCR());
    }


    private void Update()
    {
        if (m_fadeOutDone && !GameManager.Instance.DialogueRunner.IsDialogueRunning)
        {
            if (m_fadeInCR == null)
                m_fadeInCR = StartCoroutine(FadeInCR());
        }
    }

    private IEnumerator FadeInCR()
    {
        var c = m_fadeImage.color;

        for (c.a = 0; c.a < 1; c.a += Time.deltaTime * m_fadeInSpeed)
        {
            m_fadeImage.color = c;
            yield return null;
        }

        c.a = 1;
        m_fadeImage.color = c;

        yield return new WaitForSeconds(1f);
        GameManager.Instance.SetNextPhase(new MainMenuPhase(GameManager.Instance.CurrentPhase));
    }


    private IEnumerator FadeOutCR()
    {
        var c = m_fadeImage.color;

        for (c.a = 1; c.a > 0; c.a -= Time.deltaTime * m_fadeOutSpeed)
        {
            m_fadeImage.color = c;
            yield return null;
        }

        c.a = 0;
        m_fadeImage.color = c;

        yield return new WaitForSeconds(1f);
        GameManager.Instance.DialogueRunner.StartDialogue(GameManager.Instance.IntroDialogue);
        m_fadeOutDone = true;
    }
}
