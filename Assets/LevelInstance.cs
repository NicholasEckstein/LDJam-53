using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInstance : MonoBehaviour
{
    [SerializeField]
    private Transform m_playerSpawn;

    public Vector3 PlayerStartLocation { get => m_playerSpawn.position; }
}
