using System;
using UnityEngine;

public class DescentCollectable : MonoBehaviour
{   
    public Action OnPLayerPickup;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.CompareTag("Player"))
        {
            OnPLayerPickup?.Invoke();
        }
    }
}
