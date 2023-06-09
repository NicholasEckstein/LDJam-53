using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInstance : MonoBehaviour
{
	[SerializeField]
	private int m_levelNum;

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

	[SerializeField, Header("-1 to not spawn reaper")]
	private float m_timeToSpawnReaper = 120.0f;

	[SerializeField]
	Reaper m_reaper;
	
    public Vector3 PlayerStartLocation { get => m_playerSpawn.position; }
	public float TimeToAscend { get => m_timeToAscend; }
	public bool IsDescending { get => m_descendPlatformParent.gameObject.activeSelf; }
    public int LevelNum { get => m_levelNum; }

    private void OnEnable()
	{
		if(m_collectable != null)
			m_collectable.OnPLayerPickup += OnCollectableObtained;
		EnableAscentPlatformParent(false);
		EnableDescentPlatformParent(true);
	}

	private void OnDisable()
	{
		if (m_collectable != null)
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
		var gameMan = GameManager.Instance;
		AudioManager.Instance.PlaySFX(gameMan.GrabSFX);
		Destroy(m_collectable.transform.parent.gameObject);
		gameMan.PlayerController.EnableInput(false);

		gameMan.DialogueRunner.StartDialogue(gameMan.GetDialogueForLevel(m_levelNum - 1));

		StartCoroutine(DialogueWaitCR());
	}

	private IEnumerator DialogueWaitCR()
	{
		yield return new WaitUntil(() => !GameManager.Instance.DialogueRunner.IsDialogueRunning);

		SwapLevel();
		GameManager.Instance.PlayerController.EnableInput(true);
		var phase = GameManager.Instance.CurrentPhase as PlayPhase;
		if (phase != null)
			phase.EnableTimerText();

		GameManager.Instance.CameraController.AddTrauma(0.3f, 0.35f);

		yield return new WaitForSeconds(1f);

		AudioManager.Instance.PlayMusic(GameManager.Instance.AscentMusic);

		if (phase != null)
			phase.StartTimer();

		if (m_timeToSpawnReaper > 0.0f && m_reaper)
		{
			yield return new WaitForSeconds(m_timeToSpawnReaper);
			m_reaper.gameObject.SetActive(true);
		}
	}

	private void SwapLevel()
	{
		EnableAscentPlatformParent(true);
		EnableDescentPlatformParent(false);
	}
}
