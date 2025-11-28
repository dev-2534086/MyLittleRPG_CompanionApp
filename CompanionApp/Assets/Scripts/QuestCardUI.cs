using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestCardUI : MonoBehaviour
{
    [Header("Texts")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI timerText;

    [Header("Progress Bar")]
    public Image progressFill;

    // Methode pratique pour remplir facilement
    public void Setup(string title, string description, string progress, float fill, string timer = "")
    {
        if (titleText) titleText.text = title;
        if (descriptionText) descriptionText.text = description;
        if (progressText) progressText.text = progress;
        if (progressFill) progressFill.fillAmount = fill;
        if (timerText) timerText.text = timer;
    }
}
