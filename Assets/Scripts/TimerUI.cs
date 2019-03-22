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
    }

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        gameManager.GameStarted += OnGameStarted;

        text.text = TimeSpan.FromSeconds(gameManager.GameDuration).ToString(@"mm\:ss");
    }

    void OnDestroy()
    {
        gameManager.GameStarted -= OnGameStarted;
    }

    void OnGameStarted(ReadOnlyCollection<ColorStreamOne> stream)
    {
        timeLeft = gameManager.GameDuration;
        InvokeRepeating("Tick", 0, 1);
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
