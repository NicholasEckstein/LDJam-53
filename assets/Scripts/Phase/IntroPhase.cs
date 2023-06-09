using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroPhase : GamePhase
{
    private GameIntroUI m_introUI;

    public override bool Initialize()
    {
        GameManager.OpenUI<GameIntroUI>(GameManager.Instance.IntroUI);
        return true;
    }

    public override void Start()
    {
    }

    public override bool Uninitialize()
    {       
        return true;
    }

    public override void Update()
    {
    }
}
