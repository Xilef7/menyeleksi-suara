using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class GameReporter : MonoBehaviour
{
    public class Score
    {
        public LevelScore[] LevelScores;

        public Score(int[] countHit, int[] countPremature, int[] countCommission, int[] countOmission)
        {
            LevelScores = new LevelScore[countHit.Length];
            for (int i = 0; i < LevelScores.Length; i++)
            {
                LevelScores[i] = new LevelScore(countHit[i], countPremature[i], countCommission[i], countOmission[i]);
            }
        }

        public int TotalHit
        {
            get
            {
                int sum = 0;
                foreach (var levelScore in LevelScores)
                {
                    sum += levelScore.CountHit;
                }
                return sum;
            }
        }

        public int TotalPremature
        {
            get
            {
                int sum = 0;
                foreach (var levelScore in LevelScores)
                {
                    sum += levelScore.CountPremature;
                }
                return sum;
            }
        }

        public int TotalCommission
        {
            get
            {
                int sum = 0;
                foreach (var levelScore in LevelScores)
                {
                    sum += levelScore.CountCommission;
                }
                return sum;
            }
        }

        public int TotalOmission
        {
            get
            {
                int sum = 0;
                foreach (var levelScore in LevelScores)
                {
                    sum += levelScore.CountOmission;
                }
                return sum;
            }
        }
    }
    
    public class LevelScore
    {
        public int CountHit;
        public int CountPremature;
        public int CountCommission;
        public int CountOmission;

        public LevelScore(int countHit, int countPremature, int countCommission, int countOmission)
        {
            CountHit = countHit;
            CountPremature = countPremature;
            CountCommission = countCommission;
            CountOmission = countOmission;
        }
    }

    public event Action<Score> ScoreCalculated;

    [SerializeField] private bool shouldRecord = false;
    [SerializeField] private float waitDuration = 1;

    private int TotalLevel {
        get {
            return startingIndexes.Length;
        }
    }

    private GameManager gameManager;

    private ReportWriter reportWriter;
    private LinkedList<ResponseOfStimulus> responseOfStimuli;
    private ResponseStatistic responseStatistic;

    private int[] startingIndexes;

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        gameManager.GameStarted += OnGameStarted;
        gameManager.Hit += OnHit;
        gameManager.Commission += OnCommission;
        gameManager.Premature += OnPremature;
        gameManager.Omission += OnOmission;
        gameManager.GameEnded += OnGameEnded;

        startingIndexes = new int[gameManager.TotalLevel];
        for (int i = 0; i < startingIndexes.Length; i++)
        {
            startingIndexes[i] = gameManager.GetStartingIndex(i + 1);
        }
    }

    void OnDestroy()
    {
        gameManager.GameStarted -= OnGameStarted;
        gameManager.Hit -= OnHit;
        gameManager.Commission -= OnCommission;
        gameManager.Premature -= OnPremature;
        gameManager.Omission -= OnOmission;
        gameManager.GameEnded -= OnGameEnded;
    }

    void OnGameStarted(ReadOnlyCollection<ColorStreamOne> stream)
    {
        responseOfStimuli = new LinkedList<ResponseOfStimulus>();
        responseStatistic = new ResponseStatistic(TotalLevel);
        reportWriter = new ReportWriter(stream, responseOfStimuli, responseStatistic, startingIndexes);
    }

    void OnHit(ResponseInfo responseInfo)
    {
        if (!responseInfo.HasRespondedBefore)
        {
            responseStatistic.SetHit(responseInfo.ResponseTime, responseInfo.CurrentLevel);
            responseOfStimuli.AddLast(new ResponseOfStimulus(responseInfo.RespondedStimulusIndex, Accuracy.Hit));
        }
        else
        {
            responseOfStimuli.Last.Value.ResponseCount++;
        }
    }

    void OnPremature(ResponseInfo responseInfo)
    {
        if (!responseInfo.HasRespondedBefore)
        {
            responseStatistic.SetPremature(responseInfo.ResponseTime, responseInfo.CurrentLevel);
            responseOfStimuli.AddLast(new ResponseOfStimulus(responseInfo.RespondedStimulusIndex, Accuracy.Premature));
        }
        else
        {
            responseOfStimuli.Last.Value.ResponseCount++;
        }
    }

    void OnCommission(ResponseInfo responseInfo)
    {
        if (!responseInfo.HasRespondedBefore)
        {
            responseStatistic.SetCommision(responseInfo.ResponseTime, responseInfo.CurrentLevel);
            responseOfStimuli.AddLast(new ResponseOfStimulus(responseInfo.RespondedStimulusIndex, Accuracy.Commission));
        }
        else
        {
            responseOfStimuli.Last.Value.ResponseCount++;
        }
    }

    void OnOmission(int currentLevel, int lastStimulusIndex)
    {
        responseStatistic.SetOmission(currentLevel);
        responseOfStimuli.AddLast(new ResponseOfStimulus(lastStimulusIndex, Accuracy.Omission));
    }

    void OnGameEnded()
    {
        if (shouldRecord)
        {
            reportWriter.WriteReport("report.txt", 66);
        }

        StartCoroutine(WaitThenShowScoreCalculation());
    }

    IEnumerator WaitThenShowScoreCalculation()
    {
        yield return new WaitForSeconds(waitDuration);

        int[] countHit = new int[TotalLevel];
        int[] countPremature = new int[TotalLevel];
        int[] countCommission = new int[TotalLevel];
        int[] countOmission = new int[TotalLevel];

        for (int i = 0; i < TotalLevel; i++)
        {
            int level = i + 1;
            countHit[i] = responseStatistic.GetHitCount(level);
            countPremature[i] = responseStatistic.GetPrematureCount(level);
            countCommission[i] = responseStatistic.GetCommissionCount(level);
            countOmission[i] = responseStatistic.GetOmissionCount(level);
        }

        ScoreCalculated(new Score(countHit, countPremature, countCommission, countOmission));
    }
}
