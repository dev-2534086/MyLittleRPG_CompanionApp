using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BadgeItemUI : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    public void Setup(BadgeData badge, bool unlocked)
    {
        if (titleText != null)
            titleText.text = badge.title;

        if (descriptionText != null)
            descriptionText.text = badge.description;

        if (iconImage != null)
        {
            iconImage.sprite = badge.icon;
            iconImage.color = unlocked ? Color.white : new Color(1f, 1f, 1f, 0.35f);
        }
    }
}

[System.Serializable]
public class BadgeData
{
    public string id;
    public string title;
    public string description;
    public Sprite icon;
}
