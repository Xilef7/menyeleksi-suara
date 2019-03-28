using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public abstract class CountUI : MonoBehaviour
{
    [SerializeField] private string prefix = "";

    private Text text;
    private GameManager gameManager;
    private int count;

    protected virtual void Awake()
    {
        text = GetComponent<Text>();
        text.enabled = true;

        UpdateUI();
    }

    protected virtual void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        gameManager.GameStarted += OnGameStarted;
        gameManager.GameEnded += OnGameEnded;
    }

    protected virtual void OnDestroy()
    {
        gameManager.GameStarted -= OnGameStarted;
        gameManager.GameEnded -= OnGameEnded;
    }

    protected void IncrementCount(ResponseInfo responseInfo)
    {
        if (!responseInfo.HasRespondedBefore)
        {
            count++;
            UpdateUI();
        }
    }

    protected void IncrementCount(int currentLevel, int lastStimulusIndex)
    {
        count++;
        UpdateUI();
    }

    void OnGameStarted(ReadOnlyCollection<ColorStreamOne> stream)
    {
        count = 0;
        UpdateUI();
        text.enabled = true;
    }

    void OnGameEnded()
    {
        text.enabled = false;
    }

    void UpdateUI()
    {
        text.text = prefix + count;
    }
}
