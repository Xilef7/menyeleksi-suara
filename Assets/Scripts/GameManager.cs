using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ColorStreamOne : byte
{
    Black,
    Red
}

public struct Range<T> where T : System.IComparable<T>
{
    public T Min;
    public T Max;

    public Range(T min, T max)
    {
        if (max.CompareTo(min) > 0)
        {
            Min = min;
            Max = max;
        }
        else
        {
            Min = max;
            Max = min;
        }
    }

    public bool Contains(T value)
    {
        return value.CompareTo(Min) >= 0 && value.CompareTo(Max) <= 0;
    }

    public override string ToString()
    {
        return "[" + Min + "-" + Max + "]";
    }
}

public struct ResponseInfo
{
    public int CurrentLevel;
    public int RespondedStimulusIndex;
    public float ResponseTime;
    public bool HasRespondedBefore;

    public ResponseInfo(int currentLevel, int respondedStimulusIndex, float responseTime, bool hasRespondedBefore)
    {
        CurrentLevel = currentLevel;
        RespondedStimulusIndex = respondedStimulusIndex;
        ResponseTime = responseTime;
        HasRespondedBefore = hasRespondedBefore;
    }
}

public class GameManager : MonoBehaviour
{
    private enum State : byte
    {
        None,
        Black,
        RedTarget,
        RedNonTarget
    }

    public event Action<ReadOnlyCollection<ColorStreamOne>> GameStarted;
    public event Action GameEnded;
    public event Action<ResponseInfo> Hit;
    public event Action<ResponseInfo> Commission;
    public event Action<ResponseInfo> Premature;
    public event Action<int, int> Omission;

    public float GameDuration
    {
        get
        {
            return levelDuration * pauseDuration.Length;
        }
    }

    public int TotalLevel
    {
        get
        {
            return pauseDuration.Length;
        }
    }

    [SerializeField] private AudioClip blackAudioClip = null;
    [SerializeField] private AudioClip redAudioClip = null;
    [SerializeField] private AudioClip[] distractionAudioClips = null;

    [SerializeField] private AudioSource audioStreamOne = null;
    [SerializeField] private AudioSource audioStreamTwo = null;

    [SerializeField] private float waitDuration = 3;
    [SerializeField] private float levelDuration = 420;
    [SerializeField] private float wordDuration = 0.8f;
    [SerializeField] private float[] pauseDuration = { 2, 1, 0.5f };
    [SerializeField] private float offsetDuration = 0.2f;
    [SerializeField] private TextAsset streamsTextAsset = null;

    private ColorStreamOne[][] streams = null;
    private int usedStreamIndex = 0;
    private int currentLevel = 0;
    private int currentStimulusIndex = 0;
    private State currentState = State.None;
    private bool isPrevStimulusResponded = false;
    private Range<float> hitRange = new Range<float>(0, 0);
    private Range<float> prematureRange = new Range<float>(0, 0);
    private float lastStimulusTime = 0;
    private bool gameStarted = false;

    void Awake()
    {
        string[] separatingChars = {"\r\n"};
        string[] streamTexts = streamsTextAsset.text.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);

        streams = new ColorStreamOne[streamTexts.Length][];
        for (int i = 0; i < streams.Length; i++)
        {
            streams[i] = streamTexts[i].Split(null).Select(s => s.Equals("0") ? ColorStreamOne.Black : ColorStreamOne.Red).ToArray();
        }
    }

    void Start()
    {
        StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        // Initialization
        usedStreamIndex = UnityEngine.Random.Range(0, streams.Length);
        currentLevel = 0;
        currentStimulusIndex = 0;
        currentState = State.None;
        isPrevStimulusResponded = false;
        hitRange = new Range<float>(0, 0);
        prematureRange = new Range<float>(0, 0);
        lastStimulusTime = 0;
        gameStarted = false;

        // Wait some seconds before starting game
        yield return new WaitForSeconds(waitDuration);

        // Start game
        gameStarted = true;
        if (GameStarted != null)
        {
            GameStarted(Array.AsReadOnly(streams[usedStreamIndex]));
        }

        // Gameplay
        InvokeRepeating("PlayStreamTwo", 0, wordDuration);
        for (int i = 0; i < pauseDuration.Length; i++)
        {
            InvokeRepeating("PlayStreamOne", 0, wordDuration + pauseDuration[i]);

            yield return new WaitForSeconds(levelDuration);

            CancelInvoke("PlayStreamOne");
            currentLevel++;
        }
        CancelInvoke("PlayStreamTwo");

        // End game
        gameStarted = false;
        if (GameEnded != null)
        {
            GameEnded();
        }
    }

    void Update()
    {
        // Cache current time
        float now = Time.time;

        if (Input.GetKeyDown(KeyCode.Space) && gameStarted)
        {
            float responseTime = now - lastStimulusTime;
            int lastStimulusIndex = currentStimulusIndex - 1;

            ResponseInfo responseInfo = new ResponseInfo(currentLevel, lastStimulusIndex, responseTime, isPrevStimulusResponded);

            if (hitRange.Contains(now)) // If response timing falls between hit range
            {
                if (Hit != null)
                {
                    Hit(responseInfo);
                }
            }
            else if (prematureRange.Contains(now))  // If response timing falls between premature range
            {
                if (Premature != null)
                {
                    Premature(responseInfo);
                }
            }
            else  // If response timing falls between commission range
            {
                if (Commission != null)
                {
                    Commission(responseInfo);
                }
            }

            isPrevStimulusResponded = true;
        }
    }

    void PlayStreamOne()
    {
        // Cache current time
        float now = Time.time;

        if (currentState == State.RedTarget && !isPrevStimulusResponded) // If previous stimulus omitted
        {
            if (Omission != null)
            {
                Omission(currentLevel, currentStimulusIndex - 1);
            }
        }

        if (streams[usedStreamIndex][currentStimulusIndex] == ColorStreamOne.Red) // If current stimulus red
        {
            if (currentState == State.Black)
            {
                hitRange = new Range<float>(now, now + wordDuration + pauseDuration[currentLevel]);

                currentState = State.RedTarget;
            }
            else
            {
                currentState = State.RedNonTarget;
            }

            audioStreamOne.PlayOneShot(redAudioClip);
        }
        else // If current stimulus black
        {
            prematureRange = new Range<float>(now + wordDuration + pauseDuration[currentLevel] - offsetDuration, now + wordDuration + pauseDuration[currentLevel]);

            currentState = State.Black;

            audioStreamOne.PlayOneShot(blackAudioClip);
        }

        isPrevStimulusResponded = false;

        lastStimulusTime = now;

        currentStimulusIndex++;
    }

    void PlayStreamTwo()
    {
        audioStreamTwo.PlayOneShot(distractionAudioClips[UnityEngine.Random.Range(0, distractionAudioClips.Length)]);
    }
}