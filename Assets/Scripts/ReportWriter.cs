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
    private struct ResponseString
    {
        public readonly string Hit;
        public readonly string Premature;
        public readonly string Commission;
        public readonly string Omission;

        public ResponseString(string hit, string premature, string commission, string omission)
        {
            Hit = hit;
            Premature = premature;
            Commission = commission;
            Omission = omission;
        }
    }

    private ColorStreamOne[] stream;
    private IEnumerable<ResponseOfStimulus> responseOfStimuli;
    private ResponseStatistic responseStatistic;
    private int[] startingIndexes;

    public ReportWriter(ReadOnlyCollection<ColorStreamOne> stream, IEnumerable<ResponseOfStimulus> responseOfStimuli, ResponseStatistic responseStatistic, int[] startingIndexes)
    {
        this.stream = new ColorStreamOne[stream.Count];
        stream.CopyTo(this.stream, 0);
        this.responseOfStimuli = responseOfStimuli;
        this.responseStatistic = responseStatistic;
        this.startingIndexes = startingIndexes;
    }

    public void WriteStreamReport(string fileName, int width)
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
                    "Level {0}: [ Hit: {1,-5:G4} ms Premature: {2,-5:G4} ms Commission: {3,-5:G4} ms ]",
                    level,
                    responseStatistic.GetHitCount(level) > 0 ? responseStatistic.GetAverageHitResponseTime(level) * 1000 : 0,
                    responseStatistic.GetPrematureCount(level) > 0 ? responseStatistic.GetAveragePrematureResponseTime(level) * 1000 : 0,
                    responseStatistic.GetCommissionCount(level) > 0 ? responseStatistic.GetAverageCommissionResponseTime(level) * 1000 : 0);
            }
            writer.WriteLine();

            // Write response on stimulus
            var totalLevel = startingIndexes.Length;
            var stringStartingIndexes = new int[totalLevel];
            for (int i = 0; i < stringStartingIndexes.Length; i++)
            {
                stringStartingIndexes[i] = startingIndexes[i] * 2;
            }

            var streamPerLevel = SplitStreamPerLevel(BuildStreamString('B', 'R'), stringStartingIndexes);
            var responsePerLevel = SplitResponsePerLevel(BuildResponseString(), stringStartingIndexes);

            int partLength = width - 2;
            for (int level = 1; level <= totalLevel; level++)
            {
                writer.WriteLine("Level {0}", level);

                var levelStream = streamPerLevel[level - 1];
                var levelResponse = responsePerLevel[level - 1];
                
                for (int i = 0; i < levelStream.Length; i += width)
                {
                    writer.WriteLine("S: {0}", levelStream.Substring(i, System.Math.Min(partLength, levelStream.Length - i)));
                    writer.WriteLine("H: {0}", i < levelResponse.Hit.Length ? levelResponse.Hit.Substring(i, System.Math.Min(partLength, levelResponse.Hit.Length - i)) : "");
                    writer.WriteLine("P: {0}", i < levelResponse.Premature.Length ? levelResponse.Premature.Substring(i, System.Math.Min(partLength, levelResponse.Premature.Length - i)) : "");
                    writer.WriteLine("C: {0}", i < levelResponse.Commission.Length ? levelResponse.Commission.Substring(i, System.Math.Min(partLength, levelResponse.Commission.Length - i)) : "");
                    writer.WriteLine("O: {0}", i < levelResponse.Omission.Length ? levelResponse.Omission.Substring(i, System.Math.Min(partLength, levelResponse.Omission.Length - i)) : "");
                    writer.WriteLine();
                }
            }

            // Write separator
            writer.WriteLine(new string('*', width));
            writer.WriteLine();
        }
    }

    public void WriteTotalCountReport(string fileName)
    {
        using (StreamWriter writer = new StreamWriter(fileName, true))
        {
            writer.WriteLine("# {0}", System.DateTime.Now.ToString("F", new System.Globalization.CultureInfo("id-ID")));

            var totalHit = 0;
            var totalPremature = 0;
            var totalCommission = 0;
            var totalOmission = 0;

            for (int level = 1; level <= responseStatistic.TotalLevel; level++)
            {
                totalHit += responseStatistic.GetHitCount(level);
                totalPremature += responseStatistic.GetPrematureCount(level);
                totalCommission += responseStatistic.GetCommissionCount(level);
                totalOmission += responseStatistic.GetOmissionCount(level);
            }

            writer.WriteLine("Hit {0}", totalHit);
            writer.WriteLine("Premature {0}", totalPremature);
            writer.WriteLine("Commission {0}", totalCommission);
            writer.WriteLine("Omission {0}", totalOmission);
            
            writer.WriteLine();
        }
    }

    private string[] SplitStreamPerLevel(string stream, int[] startingIndexes)
    {
        var lastIndex = startingIndexes.Length - 1;
        var streamPerLevel = new string[startingIndexes.Length];

        for (int i = 0; i < lastIndex; i++)
        {
            int startingIndex = startingIndexes[i];
            int length = startingIndexes[i + 1] - startingIndex;

            streamPerLevel[i] = stream.Substring(startingIndex, length);
        }

        streamPerLevel[lastIndex] = stream.Substring(startingIndexes[lastIndex]);

        return streamPerLevel;
    }

    private ResponseString[] SplitResponsePerLevel(ResponseString response, int[] startingIndexes)
    {
        var lastIndex = startingIndexes.Length - 1;
        var responsePerLevel = new ResponseString[startingIndexes.Length];

        for (int i = 0; i < lastIndex; i++)
        {
            int startingIndex = startingIndexes[i];
            int length = startingIndexes[i + 1] - startingIndex;

            var hit = response.Hit;
            var premature = response.Premature;
            var commission = response.Commission;
            var omission = response.Omission;

            responsePerLevel[i] = new ResponseString(
                    startingIndex < hit.Length ? hit.Substring(startingIndex, System.Math.Min(length, hit.Length - startingIndex)) : "",
                    startingIndex < premature.Length ? premature.Substring(startingIndex, System.Math.Min(length, premature.Length - startingIndex)) : "",
                    startingIndex < commission.Length ? commission.Substring(startingIndex, System.Math.Min(length, commission.Length - startingIndex)) : "",
                    startingIndex < omission.Length ? omission.Substring(startingIndex, System.Math.Min(length, omission.Length - startingIndex)) : ""
                );
        }

        responsePerLevel[lastIndex] = new ResponseString(
                startingIndexes[lastIndex] < response.Hit.Length ? response.Hit.Substring(startingIndexes[lastIndex]) : "",
                startingIndexes[lastIndex] < response.Premature.Length ? response.Premature.Substring(startingIndexes[lastIndex]) : "",
                startingIndexes[lastIndex] < response.Commission.Length ? response.Commission.Substring(startingIndexes[lastIndex]) : "",
                startingIndexes[lastIndex] < response.Omission.Length ? response.Omission.Substring(startingIndexes[lastIndex]) : ""
            );

        return responsePerLevel;
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

    private ResponseString BuildResponseString()
    {
        var stringBuilders = new Dictionary<Accuracy, StringBuilder>();
        stringBuilders.Add(Accuracy.Hit, new StringBuilder());
        stringBuilders.Add(Accuracy.Premature, new StringBuilder());
        stringBuilders.Add(Accuracy.Commission, new StringBuilder());
        stringBuilders.Add(Accuracy.Omission, new StringBuilder());

        int currentStimulusIndex = -1;
        foreach (var responseOfStimulus in responseOfStimuli)
        {
            foreach (KeyValuePair<Accuracy, StringBuilder> entry in stringBuilders)
            {
                if (entry.Key == responseOfStimulus.Accuracy)
                {
                    entry.Value.AppendFormat(
                        "{0}{1,-2}",
                        new string(' ', (responseOfStimulus.StimulusIndex - currentStimulusIndex - 1) * 2),
                        responseOfStimulus.ResponseCount);
                }
                else
                {
                    entry.Value.Append(new string(' ', (responseOfStimulus.StimulusIndex - currentStimulusIndex) * 2));
                }
            }
            currentStimulusIndex = responseOfStimulus.StimulusIndex;
        }

        return new ResponseString(
            stringBuilders[Accuracy.Hit].ToString(),
            stringBuilders[Accuracy.Premature].ToString(),
            stringBuilders[Accuracy.Commission].ToString(),
            stringBuilders[Accuracy.Omission].ToString());
    }
}
