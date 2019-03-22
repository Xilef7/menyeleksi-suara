using UnityEngine;

public class CorrectCountUI : CountUI
{
    private GameManager gameManager;

    protected override void Start()
    {
        base.Start();
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        gameManager.Hit += IncrementCount;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        gameManager.Hit -= IncrementCount;
    }
}
