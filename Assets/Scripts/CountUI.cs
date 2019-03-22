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

        UpdateUI();
    }

    protected virtual void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        gameManager.GameStarted += OnGameStarted;
    }

    protected virtual void OnDestroy()
    {
        gameManager.GameStarted -= OnGameStarted;
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
    }

    void UpdateUI()
    {
        text.text = prefix + count;
    }
}
