using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

public enum Accuracy
{
    Hit,
    Premature,
    Commission,
    Omission
}

public class ResponseStatistic
{
    public int TotalLevel
    {
        get;
    }

    private float[] sumHitResponseTime;
    private float[] sumPrematureResponseTime;
    private float[] sumCommissionResponseTime;

    private int[] countHit;
    private int[] countPremature;
    private int[] countCommission;
    private int[] countOmission;

    public ResponseStatistic(int totalLevel)
    {
        TotalLevel = totalLevel;
        sumHitResponseTime = new float[totalLevel];
        sumPrematureResponseTime = new float[totalLevel];
        sumCommissionResponseTime = new float[totalLevel];
        countHit = new int[totalLevel];
        countPremature = new int[totalLevel];
        countCommission = new int[totalLevel];
        countOmission = new int[totalLevel];
    }

    public void SetHit(float responseTime, int level)
    {
        int i = level - 1;
        sumHitResponseTime[i] += responseTime;
        countHit[i]++;
    }

    public void SetPremature(float responseTime, int level)
    {
        int i = level - 1;
        sumPrematureResponseTime[i] += responseTime;
        countPremature[i]++;
    }

    public void SetCommision(float responseTime, int level)
    {
        int i = level - 1;
        sumCommissionResponseTime[i] += responseTime;
        countCommission[i]++;
    }

    public void SetOmission(int level)
    {
        countOmission[level - 1]++;
    }

    public int GetHitCount(int level)
    {
        return countHit[level - 1];
    }

    public int GetPrematureCount(int level)
    {
        return countPremature[level - 1];
    }

    public int GetCommissionCount(int level)
    {
        return countCommission[level - 1];
    }

    public int GetOmissionCount(int level)
    {
        return countOmission[level - 1];
    }

    public float GetAverageHitResponseTime(int level)
    {
        int i = level - 1;
        return sumHitResponseTime[i] / countHit[i];
    }

    public float GetAveragePrematureResponseTime(int level)
    {
        int i = level - 1;
        return sumPrematureResponseTime[i] / countPremature[i];
    }

    public float GetAverageCommissionResponseTime(int level)
    {
        int i = level - 1;
        return sumCommissionResponseTime[i] / countCommission[i];
    }
}

public class ResponseOfStimulus
{
    public int StimulusIndex;
    public Accuracy Accuracy;
    public int ResponseCount;

    public ResponseOfStimulus(int stimulusIndex, Accuracy accuracy)
    {
        StimulusIndex = stimulusIndex;
        Accuracy = accuracy;
        ResponseCount = 1;
    }
}

public class ReportWriter
{
    private ColorStreamOne[] stream;
    private IEnumerable<ResponseOfStimulus> responseOfStimuli;
    private ResponseStatistic responseStatistic;

    public ReportWriter(ReadOnlyCollection<ColorStreamOne> stream, IEnumerable<ResponseOfStimulus> responseOfStimuli, ResponseStatistic responseStatistic)
    {
        this.stream = new ColorStreamOne[stream.Count];
        stream.CopyTo(this.stream, 0);
        this.responseOfStimuli = responseOfStimuli;
        this.responseStatistic = responseStatistic;
    }

    public void WriteReport(string fileName, int width)
    {
        using (StreamWriter writer = new StreamWriter(fileName, true, Encoding.Unicode))
        {
            // Write separator
            writer.WriteLine(new string('*', width));

            // Write current date and time
            writer.WriteLine(System.DateTime.Now.ToString("F", new System.Globalization.CultureInfo("id-ID")));
            writer.WriteLine();

            // Write response count
            writer.WriteLine("Response Count:");
            for (int level = 1; level <= responseStatistic.TotalLevel; level++)
            {
                writer.WriteLine(
                    "Level {0}: [ Hit: {1,-3:D} Premature: {2,-3:D} Commission: {3,-3:D} Omission: {4,-3:D} ]",
                    level,
                    responseStatistic.GetHitCount(level),
                    responseStatistic.GetPrematureCount(level),
                    responseStatistic.GetCommissionCount(level),
                    responseStatistic.GetOmissionCount(level));
            }
            writer.WriteLine();

            // Write response time
            writer.WriteLine("Response Time:");
            for (int level = 1; level <= responseStatistic.TotalLevel; level++)
            {
                writer.WriteLine(
                    "Level {0}: [ Hit: {1,-7:G5}s Premature: {2,-7:G5}s Commission: {3,-7:G5}s ]",
                    level,
                    responseStatistic.GetAverageHitResponseTime(level),
                    responseStatistic.GetAveragePrematureResponseTime(level),
                    responseStatistic.GetAverageCommissionResponseTime(level));
            }
            writer.WriteLine();

            // Write response on stimulus
            var stream = BuildStreamString('B', 'R');
            var hitResponse = BuildResponseString(Accuracy.Hit);
            var prematureResponse = BuildResponseString(Accuracy.Premature);
            var commissionResponse = BuildResponseString(Accuracy.Commission);
            var omissionResponse = BuildResponseString(Accuracy.Omission);

            int partLength = width - 2;
            for (int i = 0; i < stream.Length; i += width)
            {
                writer.WriteLine("S: {0}", stream.Substring(i, System.Math.Min(partLength, stream.Length - i)));
                writer.WriteLine("H: {0}", i < hitResponse.Length ? hitResponse.Substring(i, System.Math.Min(partLength, hitResponse.Length - i)) : "");
                writer.WriteLine("P: {0}", i < prematureResponse.Length ? prematureResponse.Substring(i, System.Math.Min(partLength, prematureResponse.Length - i)) : "");
                writer.WriteLine("C: {0}", i < commissionResponse.Length ? commissionResponse.Substring(i, System.Math.Min(partLength, commissionResponse.Length - i)) : "");
                writer.WriteLine("O: {0}", i < omissionResponse.Length ? omissionResponse.Substring(i, System.Math.Min(partLength, omissionResponse.Length - i)) : "");
                writer.WriteLine();
            }

            // Write separator
            writer.WriteLine(new string('*', width));
            writer.WriteLine();
        }
    }

    private string BuildStreamString(char black, char red)
    {
        var sb = new StringBuilder();
        foreach (var color in stream)
        {
            sb.AppendFormat("{0} ", color == ColorStreamOne.Black ? black : red);
        }
        return sb.ToString();
    }

    private string BuildResponseString(Accuracy accuracy)
    {
        var sb = new StringBuilder();
        int i = 0;
        foreach (var responseOfStimulus in responseOfStimuli)
        {
            if (responseOfStimulus.Accuracy == accuracy)
            {
                while (i < responseOfStimulus.StimulusIndex)
                {
                    sb.Append(new string(' ', 2));
                    i++;
                }
                sb.AppendFormat("{0} ", responseOfStimulus.ResponseCount);
                i++;
            }
        }
        return sb.ToString();
    }
}
