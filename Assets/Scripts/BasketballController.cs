using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BasketballController : MonoBehaviour
{
    [HideInInspector] public UnityEvent ballCollected = new UnityEvent();

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("PLAYER COLLECTED BALL");
            Destroy(this.gameObject);
        }
    }
}
