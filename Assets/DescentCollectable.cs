using System;
using UnityEngine;

public class DescentCollectable : MonoBehaviour
{   
    public Action OnPLayerPickup;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OnPLayerPickup?.Invoke();
        }
    }

}
