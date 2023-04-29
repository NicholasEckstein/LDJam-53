using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class GameManager : SingletonBase<GameManager>
{
    private enum PhaseSubSection
    {
        None = 0,
        InitializePhase,
        Start,
        Update,
        UninitializePhase
    }

    [SerializeField]
    private GameObject m_playerPrefab;

    [SerializeField]
    private List<GameObject> m_levels;

    [SerializeField]
    private CameraController m_cameraController;

    [Header("UI")]

    [SerializeField]
    private GameObject m_mainMenu;
    
    [SerializeField]
    private GameObject m_levelSelect;


    [SerializeField]
    private GameObject m_loadingUI;

    [SerializeField]
    private GameObject m_gameUI;

    [SerializeField]
    private float m_loadingScreenDelayTime = 0.5f;

    [Header("Dialogue")]

    [SerializeField]
    private DialogueRunner m_dialogueRunner;

    [SerializeField]
    private string m_introDialogueNode;

    [Header("Audio")]

    [SerializeField, Range(0, 1)]
    private float m_musicVolume = 0.5f;

    [SerializeField, Range(0, 1)]
    private float m_sfxVolume = 0.5f;

    [SerializeField]
    private AudioClip m_mainMenuMusic;

    [SerializeField]
    private AudioClip m_descentMusic;

    [SerializeField]
    private AudioClip m_playerHitSFX;

    [SerializeField]
    private AudioClip m_playerDeadSFX;


    private PhaseSubSection m_currentSubPhase;
    private PlayerController m_playerController;
    private LevelInstance m_currentLevel;

    public AudioClip MainMenuMusic { get => m_mainMenuMusic; }
    public GamePhase CurrentPhase { get; private set; }
    public GamePhase NextPhase { get; private set; }

    public GameObject MainMenuUI { get => m_mainMenu; }
    public GameObject LoadingUI { get => m_loadingUI; }
    public GameObject LevelSelect { get => m_levelSelect; }
    public PlayerController PlayerController { get => m_playerController; }
    public float LoadingScreenDelayTime { get => m_loadingScreenDelayTime; }
    public AudioClip DescentMusic { get => m_descentMusic; }
    public DialogueRunner DialogueRunner { get => m_dialogueRunner; }
    public string IntroDialogue { get => m_introDialogueNode; }
    public float MusicVolume { get => m_musicVolume; }
    public float SFXVolume { get => m_sfxVolume; }
    public GameObject GameUI { get => m_gameUI; }
    public AudioClip PlayerDeadSFX { get => m_playerDeadSFX; }
    public AudioClip PlayerHitSFX { get => m_playerHitSFX; }
    public LevelInstance CurrentLevel { get => m_currentLevel; }

    protected override void Awake()
    {
        base.Awake();

        AudioManager.InitInstance(true);

        CurrentPhase = null;
        NextPhase = null;
        m_currentSubPhase = PhaseSubSection.None;

        SetCurrentPhase(new MainMenuPhase(null));
    }

    private void Update()
    {
        if (CurrentPhase == null)
            return;

        bool complete = false;
        switch (m_currentSubPhase)
        {
            case PhaseSubSection.InitializePhase:
                complete = CurrentPhase.Initialize();
                if (complete)
                {
                    m_currentSubPhase = PhaseSubSection.Start;
                }
                break;
            case PhaseSubSection.Start:
                CurrentPhase.Start();
                m_currentSubPhase = PhaseSubSection.Update;
                break;
            case PhaseSubSection.Update:
                CurrentPhase.Update();
                if (NextPhase != null)
                {
                    m_currentSubPhase = PhaseSubSection.UninitializePhase;
                }
                break;
            case PhaseSubSection.UninitializePhase:
                complete = CurrentPhase.Uninitialize();
                if (complete)
                {
                    SetCurrentPhase(NextPhase);
                    NextPhase = null;
                }
                break;
            default:
                break;
        }
    }

    public void SetNextPhase(GamePhase a_phase)
    {
        if (m_currentSubPhase > PhaseSubSection.Update)
            return;

        NextPhase = a_phase;
    }

    public void SetCurrentPhase(GamePhase a_phase)
    {
        if(a_phase != null)
        {
            CurrentPhase = a_phase;
            m_currentSubPhase = PhaseSubSection.InitializePhase;
        }
    }

    public void CreatePlayer()
    {
        if (m_playerPrefab == null || m_playerController != null)
            return;

        //TODO: spawn player at proper location in the level
        var pObj = Instantiate(m_playerPrefab);
        var comp = pObj.GetComponent<PlayerController>();
        if (comp == null)
            return;

        m_playerController = comp;
        if (m_playerController != null && m_cameraController != null)
        {
            m_playerController.EnableGravity(false);

            m_cameraController.enabled = true;
            m_cameraController.SetTarget(m_playerController.transform);
        }
    }

    public void DestroyPlayer()
    {
        if (m_playerController != null)
        {
            Destroy(m_playerController.gameObject);
            m_playerController = null;
        }
        
        if(m_cameraController != null)
        {
            m_cameraController.enabled = false;
        }
    }

    public void LoadLevel(int a_index)
    {
        if (a_index > m_levels.Count - 1)
            return;

        var obj = Instantiate(m_levels[a_index]);

        if(obj != null)
        {
            var comp = obj.GetComponent<LevelInstance>();
            if(comp != null)
            {
                m_currentLevel = comp;
                PlayerController.transform.position = m_currentLevel.PlayerStartLocation;
            }
        }
    }

    public bool IsValidLevel(int a_index)
    {
        return a_index >= 0 && a_index < m_levels.Count;
    }


    public static T OpenUI<T>(GameObject a_uiObj, Transform a_parent = null)
    {
        if (a_uiObj == null)
            return default(T);

        if (a_parent == null)
        {
            var obj = GameObject.FindGameObjectWithTag("MainCanvas");
            if (obj != null)
            {
                a_parent = obj.transform;
            }
        }

        var ui = Instantiate(a_uiObj, a_parent);
        if(ui != null)
        {
            var comp = ui.GetComponent<T>();
            if (comp != null)
                return comp;
        }

        return default(T);
    }

    public static void CloseUI(UIPrefab a_ui)
    {
        if (a_ui == null)
            return;

        Destroy(a_ui.gameObject);
    }

    public bool HasSeenIntro()
    {
        var vs = m_dialogueRunner.VariableStorage;
        bool seenIntro;
        vs.TryGetValue("$seenIntro", out seenIntro);
        return seenIntro;
    }


    public Coroutine RunCoroutine(IEnumerator a_enumerator)
    {
        return StartCoroutine(a_enumerator);
    }
}
