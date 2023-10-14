using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Invector.vCharacterController;

public class UIController : MonoBehaviour
{
    [SerializeField] private vThirdPersonController playerController = null;
    [SerializeField] private TextMeshProUGUI bBallCounterText = null;
    private int bBallCounter = 0;

    private void Awake()
    {
        playerController.onBallCollected.AddListener(IncreaseBBallCounter);
    }

    private void IncreaseBBallCounter()
    {
        bBallCounter++;
        bBallCounterText.text = $"{bBallCounter}/5";
    }
}
