using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInstance : MonoBehaviour
{
    [SerializeField]
    private Transform m_playerSpawn;

    [SerializeField]
    private Transform m_ascendPlatformParent;

    [SerializeField]
    private Transform m_descendPlatformParent;

    [SerializeField]
    private DescentCollectable m_collectable;

    [SerializeField]
    private float m_timeToAscend;

    public Vector3 PlayerStartLocation { get => m_playerSpawn.position; }
    public float TimeToAscend { get => m_timeToAscend; }

    private void OnEnable()
    {
        m_collectable.OnPLayerPickup += OnCollectableObtained;
        EnableAscentPlatformParent(false);
        EnableDescentPlatformParent(true);
    }

    private void OnDisable()
    {
        m_collectable.OnPLayerPickup -= OnCollectableObtained;
    }

    public void EnableAscentPlatformParent(bool a_enable)
    {
        m_ascendPlatformParent.gameObject.SetActive(a_enable);
    }

    public void EnableDescentPlatformParent(bool a_enable)
    {
        m_descendPlatformParent.gameObject.SetActive(a_enable);
    }

    private void OnCollectableObtained()
    {
        Destroy(m_collectable.gameObject);
        GameManager.Instance.PlayerController.EnableInput(false);
        GameManager.Instance.DialogueRunner.StartDialogue("Intro");
        StartCoroutine(DialogueWaitCR());
    }

    private IEnumerator DialogueWaitCR()
    {
        yield return new WaitUntil(() => !GameManager.Instance.DialogueRunner.IsDialogueRunning);

        SwapLevel();

        //TODO: screen shake

        AudioManager.Instance.PlayMusic(GameManager.Instance.AscentMusic);

        yield return new WaitForSeconds(2);

        GameManager.Instance.PlayerController.EnableInput(false);

        //Activate timer
        var phase = GameManager.Instance.CurrentPhase as PlayPhase;
        if(phase != null)
        {
            phase.EnableTimer();
        }
    }

    private void SwapLevel()
    {
        EnableAscentPlatformParent(true);
        EnableDescentPlatformParent(false);
    }
}
