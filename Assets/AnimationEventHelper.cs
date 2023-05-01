using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEventHelper : MonoBehaviour
{
    [SerializeField]
    private UnityEvent m_animationEvent1;

    public void AnimationEvent1()
    {
        m_animationEvent1?.Invoke();
    }
}
