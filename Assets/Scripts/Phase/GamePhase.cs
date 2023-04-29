using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GamePhase
{
    public virtual bool Initialize()
    {
        return true;
    }
    public abstract void Start();
    public abstract void Update();
    public abstract bool Uninitialize();
}
