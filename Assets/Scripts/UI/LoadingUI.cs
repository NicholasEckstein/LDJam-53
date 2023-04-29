using System;
using System.Collections;
using UnityEngine;

public class LoadingUI : UIPrefab
{
    static LoadingUI m_inst;
    Action OnLoadingScreenClosed;
    private bool m_isClosing;

    public static bool IsClosing { get => m_inst != null ? m_inst.m_isClosing : false; }

    private void CloseLoadingScreenImpl(float a_delay)
    {
        m_isClosing = true;

        if (a_delay > 0)
        {
            StartCoroutine(CloseWithDelayCR(a_delay));
        }
        else
        {
            Close();
        }
    }

    private void Close()
    {
        OnLoadingScreenClosed?.Invoke();
        GameManager.CloseUI(m_inst);
        m_inst = null;
    }

    private IEnumerator CloseWithDelayCR(float a_delay)
    {
        yield return new WaitForSeconds(a_delay);
        Close();
    }

    public static void ShowLoadingScreen()
    {
        if (m_inst != null)
            m_inst = null;

        m_inst = GameManager.OpenUI<LoadingUI>(GameManager.Instance.LoadingUI);
    }

    public static void CloseLoadingScreen(float a_delay = 0, Action a_onLoadingScreenClose = null)
    {
        if (m_inst != null)
        {
            if (m_inst.m_isClosing)
                return;

            m_inst.OnLoadingScreenClosed = a_onLoadingScreenClose;
            m_inst.CloseLoadingScreenImpl(a_delay);
        }
    }

}