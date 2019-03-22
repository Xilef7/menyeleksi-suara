using UnityEngine;

public class IncorrectCountUI : CountUI
{
    private GameManager gameManager;

    protected override void Start()
    {
        base.Start();
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        gameManager.Commission += IncrementCount;
        gameManager.Omission += IncrementCount;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        gameManager.Commission -= IncrementCount;
        gameManager.Omission -= IncrementCount;
    }
}
