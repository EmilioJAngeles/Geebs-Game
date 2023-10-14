using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject geebsPlayer = null;
    [SerializeField] private GameObject gameInstructions = null;
    [SerializeField] private GameObject sprintInstructions = null;
    [SerializeField] private GameObject endGameUI = null;
    [SerializeField] private GameObject ballCounter = null;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("Environment");
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button1) && gameInstructions.activeInHierarchy)
        {
            geebsPlayer.SetActive(true);
            gameInstructions.SetActive(false);
            sprintInstructions.SetActive(true);
            ballCounter.SetActive(true);

            StartCoroutine(DisableSprintInstructions());
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button1) && endGameUI.activeInHierarchy)
        {
            SceneManager.LoadScene("Environment");
        }
    }

    private IEnumerator DisableSprintInstructions()
    {
        yield return new WaitForSeconds(5);
        sprintInstructions.SetActive(false);
    }
}
