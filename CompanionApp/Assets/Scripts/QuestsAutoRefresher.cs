using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class QuestsAutoRefresher : MonoBehaviour
{
    public QuestsManager questsManager;
    public TextMeshProUGUI globalTimerText;
    public int refreshDelaySeconds = 600; // 10 minutes

    private DateTime nextRefreshTime;

    private void Start()
    {
        nextRefreshTime = DateTime.UtcNow.AddSeconds(refreshDelaySeconds);
    }

    private void Update()
    {
        // Temps restant
        TimeSpan remaining = nextRefreshTime - DateTime.UtcNow;

        if (remaining.TotalSeconds <= 0)
        {
            Debug.Log("⏳ Timer terminé → Mise à jour des quêtes...");

            StartCoroutine(RefreshQuests());
            nextRefreshTime = DateTime.UtcNow.AddSeconds(refreshDelaySeconds);

            remaining = TimeSpan.FromSeconds(refreshDelaySeconds);
        }

        // UI du timer
        globalTimerText.text = $"{remaining.Minutes:00}:{remaining.Seconds:00}";
    }

    IEnumerator RefreshQuests()
    {
        yield return StartCoroutine(questsManager.FetchQuests());
    }
}
