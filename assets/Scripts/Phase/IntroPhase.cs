using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroPhase : GamePhase
{
    private GameIntroUI m_introUI;

    public override bool Initialize()
    {
        m_introUI = GameManager.OpenUI<GameIntroUI>(GameManager.Instance.IntroUI);
        return true;
    }

    public override void Start()
    {
    }

    public override bool Uninitialize()
    {
        if(m_introUI != null)
        { 
            GameManager.CloseUI(m_introUI);
            m_introUI = null;
        }
        
        return true;
    }

    public override void Update()
    {
    }
}
