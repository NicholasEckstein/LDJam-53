using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectUI : UIPrefab
{
    [SerializeField]
    private List<Image> m_levels;

    [SerializeField]
    private Image m_lockedLevelImage;


    private int m_levelSelected = 0;

    private void Awake()
    {
        InitLevelUnlocks();
        EnableSelectForLevel(m_levelSelected);
    }

    private void InitLevelUnlocks()
    {
        var i = PlayerPrefs.GetInt(GameManager.Level2Str, -1);
        m_lockedLevelImage.gameObject.SetActive(i != GameManager.LevelUnlocked);
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (GameManager.Instance.DialogueRunner.IsDialogueRunning || LoadingUI.IsOpen)
            return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (GameManager.Instance.IsValidLevel(m_levelSelected))
            {
                LoadingUI.ShowLoadingScreen();
                GameManager.Instance.SetNextPhase(new PlayPhase(m_levelSelected));
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LoadingUI.ShowLoadingScreen();
            GameManager.Instance.SetNextPhase(new MainMenuPhase(GameManager.Instance.CurrentPhase));
        }

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (m_levelSelected > 0)
                m_levelSelected -= 1;

            EnableSelectForLevel(m_levelSelected);
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (m_levelSelected < m_levels.Count - 1)
                m_levelSelected += 1;

            EnableSelectForLevel(m_levelSelected);
        }
    }

    public void EnableSelectForLevel(int a_index)
    {
        for (int i = 0; i < m_levels.Count; i++)
        {
            m_levels[i].enabled = m_levelSelected == i ? true : false;
        }
    }
}
