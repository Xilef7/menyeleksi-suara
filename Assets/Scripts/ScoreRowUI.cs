using UnityEngine;
using UnityEngine.UI;

public class ScoreRowUI : MonoBehaviour
{
    [SerializeField] private Text levelText = null;
    [SerializeField] private Text hitCountText = null;
    [SerializeField] private Text commissionCountText = null;
    [SerializeField] private Text omissionCountText = null;

    public void Initialize(int level, GameReporter.LevelScore levelScore)
    {
        levelText.text = level.ToString();
        hitCountText.text = levelScore.CountHit.ToString();
        commissionCountText.text = levelScore.CountCommission.ToString();
        omissionCountText.text = levelScore.CountOmission.ToString();
    }

    public void Initialize(int totalHit, int totalCommission, int totalOmission)
    {
        levelText.text = "Total";
        hitCountText.text = totalHit.ToString();
        commissionCountText.text = totalCommission.ToString();
        omissionCountText.text = totalOmission.ToString();
    }
}
