using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(CanvasGroup))]
public class EndGameUI : MonoBehaviour
{
    [SerializeField] private GameObject scoreRow = null;
    [SerializeField] private Text scoreText = null;
    [SerializeField] private Transform scoreCalculationTransform = null;

    private AudioSource audioSource;
    private CanvasGroup canvasGroup;
    private GameReporter gameReporter;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0;
    }

    void Start()
    {
        gameReporter = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameReporter>();
        gameReporter.ScoreCalculated += OnScoreCalculated;
    }

    void OnDestroy()
    {
        gameReporter.ScoreCalculated -= OnScoreCalculated;
    }

    void OnScoreCalculated(GameReporter.Score score)
    {
        scoreText.text = score.TotalHit.ToString();

        for (int i = 0; i < score.LevelScores.Length; i++)
        {
            GameObject scoreRowInstance = Instantiate(scoreRow, scoreCalculationTransform);
            scoreRowInstance.GetComponent<ScoreRowUI>().Initialize(i + 1, score.LevelScores[i]);
            scoreRowInstance.transform.SetSiblingIndex(i + 1);
        }

        GameObject totalRowInstance = Instantiate(scoreRow, scoreCalculationTransform);
        totalRowInstance.GetComponent<ScoreRowUI>().Initialize(score.TotalHit, score.TotalCommission, score.TotalOmission);
        totalRowInstance.transform.SetSiblingIndex(score.LevelScores.Length + 1);

        audioSource.Play();
        canvasGroup.alpha = 1;
    }
}
