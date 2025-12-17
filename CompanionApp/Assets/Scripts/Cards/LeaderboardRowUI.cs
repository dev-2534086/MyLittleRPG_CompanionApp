using TMPro;
using UnityEngine;

public class LeaderboardRowUI : MonoBehaviour
{
    public TMP_Text rankText;
    public TMP_Text nameText;
    public TMP_Text valueText;

    public void Setup(int rank, string playerName, int value, string criteriaLabel)
    {
        rankText.text = FormatRank(rank);
        nameText.text = playerName;
        valueText.text = $"{value} {criteriaLabel}";
    }

    private string FormatRank(int rank)
    {
        if (rank == 1) return "1er";
        return rank + "e";
    }
}
