using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class GameReporter : MonoBehaviour
{
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

    private enum Accuracy
    {
        Hit,
        Premature,
        Commission,
        Omission
    }

    private class ResponseDatum
    {
        public int StimulusIndex;
        public Accuracy Accuracy;
        public int ResponseCount;

        public ResponseDatum(int stimulusIndex, Accuracy accuracy)
        {
            StimulusIndex = stimulusIndex;
            Accuracy = accuracy;
            ResponseCount = 1;
        }
    }

    public event Action<Score> ScoreCalculated;

    [SerializeField] private bool shouldRecord = false;
    [SerializeField] private float waitDuration = 1;

    private GameManager gameManager;

    private float[] sumHitResponseTime;
    private float[] sumPrematureResponseTime;
    private float[] sumCommissionResponseTime;

    private int[] countHit;
    private int[] countPremature;
    private int[] countCommission;
    private int[] countOmission;

    private LinkedList<ResponseDatum> responseData = null;

    private int totalLevel;

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        gameManager.GameStarted += OnGameStarted;
        gameManager.Hit += OnHit;
        gameManager.Commission += OnCommission;
        gameManager.Premature += OnPremature;
        gameManager.Omission += OnOmission;
        gameManager.GameEnded += OnGameEnded;

        totalLevel = gameManager.TotalLevel;
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
        sumHitResponseTime = new float[totalLevel];
        sumPrematureResponseTime = new float[totalLevel];
        sumCommissionResponseTime = new float[totalLevel];
        countHit = new int[totalLevel];
        countPremature = new int[totalLevel];
        countCommission = new int[totalLevel];
        countOmission = new int[totalLevel];
        responseData = new LinkedList<ResponseDatum>();
    }

    void OnHit(ResponseInfo responseInfo)
    {
        int currentLevel = responseInfo.CurrentLevel;
        if (!responseInfo.HasRespondedBefore)
        {
            sumHitResponseTime[currentLevel] += responseInfo.ResponseTime;
            countHit[currentLevel]++;
            responseData.AddLast(new ResponseDatum(responseInfo.RespondedStimulusIndex, Accuracy.Hit));
        }
        else
        {
            responseData.Last.Value.ResponseCount++;
        }
    }

    void OnPremature(ResponseInfo responseInfo)
    {
        int currentLevel = responseInfo.CurrentLevel;
        if (!responseInfo.HasRespondedBefore)
        {
            sumPrematureResponseTime[currentLevel] += responseInfo.ResponseTime;
            countPremature[currentLevel]++;
            responseData.AddLast(new ResponseDatum(responseInfo.RespondedStimulusIndex, Accuracy.Premature));
        }
        else
        {
            responseData.Last.Value.ResponseCount++;
        }

    }

    void OnCommission(ResponseInfo responseInfo)
    {
        int currentLevel = responseInfo.CurrentLevel;
        if (!responseInfo.HasRespondedBefore)
        {
            sumCommissionResponseTime[currentLevel] += responseInfo.ResponseTime;
            countCommission[currentLevel]++;
            responseData.AddLast(new ResponseDatum(responseInfo.RespondedStimulusIndex, Accuracy.Premature));
        }
        else
        {
            responseData.Last.Value.ResponseCount++;
        }
    }

    void OnOmission(int currentLevel, int lastStimulusIndex)
    {
        countOmission[currentLevel]++;
        responseData.AddLast(new ResponseDatum(lastStimulusIndex, Accuracy.Omission));
    }

    void OnGameEnded()
    {
        float[] avgHitResponseTime = new float[totalLevel];
        float[] avgPrematureResponseTime = new float[totalLevel];
        float[] avgCommissionResponseTime = new float[totalLevel];
        for (int i = 0; i < totalLevel; i++)
        {
            avgHitResponseTime[i] = sumHitResponseTime[i] / countHit[i];
            avgPrematureResponseTime[i] = sumPrematureResponseTime[i] / countPremature[i];
            avgCommissionResponseTime[i] = sumCommissionResponseTime[i] / countCommission[i];
        }

        if (shouldRecord)
        {
            // TODO: Report
        }

        StartCoroutine(WaitThenShowScoreCalculation());
    }

    IEnumerator WaitThenShowScoreCalculation()
    {
        yield return new WaitForSeconds(waitDuration);
        ScoreCalculated(new Score(countHit, countPremature, countCommission, countOmission));
    }
}
