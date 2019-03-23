using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

    private ColorStreamOne[] stream;

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
        this.stream = new ColorStreamOne[stream.Count];
        stream.CopyTo(this.stream, 0);
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
        int i = responseInfo.CurrentLevel - 1;
        if (!responseInfo.HasRespondedBefore)
        {
            sumHitResponseTime[i] += responseInfo.ResponseTime;
            countHit[i]++;
            responseData.AddLast(new ResponseDatum(responseInfo.RespondedStimulusIndex, Accuracy.Hit));
        }
        else
        {
            responseData.Last.Value.ResponseCount++;
        }
    }

    void OnPremature(ResponseInfo responseInfo)
    {
        int i = responseInfo.CurrentLevel - 1;
        if (!responseInfo.HasRespondedBefore)
        {
            sumPrematureResponseTime[i] += responseInfo.ResponseTime;
            countPremature[i]++;
            responseData.AddLast(new ResponseDatum(responseInfo.RespondedStimulusIndex, Accuracy.Premature));
        }
        else
        {
            responseData.Last.Value.ResponseCount++;
        }
    }

    void OnCommission(ResponseInfo responseInfo)
    {
        int i = responseInfo.CurrentLevel - 1;
        if (!responseInfo.HasRespondedBefore)
        {
            sumCommissionResponseTime[i] += responseInfo.ResponseTime;
            countCommission[i]++;
            responseData.AddLast(new ResponseDatum(responseInfo.RespondedStimulusIndex, Accuracy.Commission));
        }
        else
        {
            responseData.Last.Value.ResponseCount++;
        }
    }

    void OnOmission(int currentLevel, int lastStimulusIndex)
    {
        countOmission[currentLevel - 1]++;
        responseData.AddLast(new ResponseDatum(lastStimulusIndex, Accuracy.Omission));
    }

    void OnGameEnded()
    {
        if (shouldRecord)
        {
            Report();
        }

        StartCoroutine(WaitThenShowScoreCalculation());
    }

    IEnumerator WaitThenShowScoreCalculation()
    {
        yield return new WaitForSeconds(waitDuration);
        ScoreCalculated(new Score(countHit, countPremature, countCommission, countOmission));
    }

    void Report()
    {
        using (StreamWriter writer = new StreamWriter("report.txt", true, System.Text.Encoding.Unicode))
        {
            writer.WriteLine("********************************************************************");

            writer.WriteLine(System.DateTime.Now.ToString("F", new System.Globalization.CultureInfo("id-ID")));
            writer.WriteLine();

            writer.WriteLine("Response Count:");
            for (int i = 0; i < totalLevel; i++)
            {
                writer.WriteLine(
                    "Level {0}: [ Hit: {1,-3:D} Premature: {2,-3:D} Commission: {3,-3:D} Omission: {4,-3:D} ]",
                    i + 1,
                    countHit[i],
                    countPremature[i],
                    countCommission[i],
                    countOmission[i]);
            }
            writer.WriteLine();

            writer.WriteLine("Response Time:");
            for (int i = 0; i < totalLevel; i++)
            {
                writer.WriteLine(
                    "Level {0}: [ Hit: {1,-7:G5}s Premature: {2,-7:G5}s Commission: {3,-7:G5}s ]",
                    i + 1,
                    sumHitResponseTime[i] / countHit[i],
                    sumPrematureResponseTime[i] / countPremature[i],
                    sumCommissionResponseTime[i] / countCommission[i]);
            }
            writer.WriteLine();

            writer.Write("S: ");
            foreach (var color in stream)
            {
                writer.Write("{0} ", color == ColorStreamOne.Black ? 'B' : 'R');
            }
            writer.WriteLine();

            writer.Write("H: ");
            WriteResponse(writer, Accuracy.Hit);
            writer.Write("P: ");
            WriteResponse(writer, Accuracy.Premature);
            writer.Write("C: ");
            WriteResponse(writer, Accuracy.Commission);
            writer.Write("O: ");
            WriteResponse(writer, Accuracy.Omission);

            writer.WriteLine("********************************************************************");
            writer.WriteLine();
        }
    }

    void WriteResponse(StreamWriter writer, Accuracy accuracy)
    {
        int i = 0;
        foreach (var responseDatum in responseData)
        {
            if (responseDatum.Accuracy == accuracy)
            {
                for (; i < responseDatum.StimulusIndex; i++)
                {
                    writer.Write("  ");
                }
                writer.Write(responseDatum.ResponseCount);
                writer.Write(' ');
                i++;
            }
        }
        writer.WriteLine();
    }
}
