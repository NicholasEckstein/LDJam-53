using System;
using UnityEngine;

public abstract class SingletonBase : MonoBehaviour { }

public abstract class SingletonBase<T> : SingletonBase where T : SingletonBase
{
    private static bool Compare<T>(T x, T y) where T : class
    {
        return x == y;
    }

    #region Singleton

    private static T _instance = default(T);

    public static T Instance
    {
        get
        {
            if (!Compare<T>(default(T), _instance)) return _instance;

            InitInstance(true);
            return _instance;
        }
    }

    #endregion

    protected virtual void Awake()
    {
        InitInstance(false);
    }

    public static void InitInstance(bool shouldInitManager)
    {
        Type thisType = typeof(T);

        _instance = FindObjectOfType<T>();

        if (Compare<T>(default(T), _instance))
        {
            _instance = new GameObject(thisType.Name).AddComponent<T>();
        }

        //Won't call InitManager from Awake
        if (shouldInitManager)
        {
            (_instance as SingletonBase<T>).InitManager();
        }
    }

    public virtual void InitManager() { }
}