using System;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TimerUI : MonoBehaviour
{
    private Text text;
    private GameManager gameManager;

    private float timeLeft;

    void Awake()
    {
        text = GetComponent<Text>();
        text.enabled = true;
    }

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        gameManager.GameStarted += OnGameStarted;
        gameManager.GameEnded += OnGameEnded;

        text.text = TimeSpan.FromSeconds(gameManager.GameDuration).ToString(@"mm\:ss");
    }

    void OnDestroy()
    {
        gameManager.GameStarted -= OnGameStarted;
        gameManager.GameEnded -= OnGameEnded;
    }

    void OnGameStarted(ReadOnlyCollection<ColorStreamOne> stream)
    {
        timeLeft = gameManager.GameDuration;
        InvokeRepeating("Tick", 0, 1);
        text.enabled = true;
    }

    void OnGameEnded()
    {
        text.enabled = false;
    }

    void Tick()
    {
        if (timeLeft < 0)
        {
            CancelInvoke("Tick");
            return;
        }

        text.text = TimeSpan.FromSeconds(timeLeft).ToString(@"mm\:ss");
        timeLeft--;
    }
}
