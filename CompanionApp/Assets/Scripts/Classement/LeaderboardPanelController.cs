using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LeaderboardPanelController : MonoBehaviour
{
    [Header("UI")]
    public Transform contentParent;
    public GameObject rowPrefab;

    [Header("API")]
    public string apiUrl = "https://localhost:7029/api/Characters/Leaderboard";

    private List<LeaderboardEntry> entries = new();

    private enum SortType
    {
        Level,
        MaxHp,
        Attack,
        Defense
    }

    private SortType currentSort = SortType.Level;

    private void OnEnable()
    {
        StartCoroutine(FetchLeaderboard());
    }

    public void ShowByLevel()
    {
        currentSort = SortType.Level;
        Populate();
    }

    public void ShowByMaxHp()
    {
        currentSort = SortType.MaxHp;
        Populate();
    }

    public void ShowByAttack()
    {
        currentSort = SortType.Attack;
        Populate();
    }

    public void ShowByDefense()
    {
        currentSort = SortType.Defense;
        Populate();
    }

    // 🌐 API
    private IEnumerator FetchLeaderboard()
    {
        using UnityWebRequest req = UnityWebRequest.Get(apiUrl);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Leaderboard fetch failed: " + req.error);
            yield break;
        }

        string json = "{ \"items\": " + req.downloadHandler.text + " }";
        LeaderboardResponse response =
            JsonUtility.FromJson<LeaderboardResponse>(json);

        entries = response.items;

        Populate();
    }

    private void Populate()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        List<LeaderboardEntry> sorted = new(entries);
        sorted.Sort((a, b) => GetValue(b).CompareTo(GetValue(a)));

        for (int i = 0; i < sorted.Count; i++)
        {
            GameObject go = Instantiate(rowPrefab, contentParent);
            LeaderboardRowUI ui = go.GetComponent<LeaderboardRowUI>();

            ui.Setup(
                i + 1,
                sorted[i].name,
                GetValue(sorted[i]),
                GetLabel()
            );
        }
    }

    private int GetValue(LeaderboardEntry e)
    {
        return currentSort switch
        {
            SortType.Level => e.level,
            SortType.MaxHp => e.maxHp,
            SortType.Attack => e.attack,
            SortType.Defense => e.defense,
            _ => 0
        };
    }

    private string GetLabel()
    {
        return currentSort switch
        {
            SortType.Level => "Level",
            SortType.MaxHp => "HP",
            SortType.Attack => "Attack",
            SortType.Defense => "Defense",
            _ => ""
        };
    }

    // DTO
    [System.Serializable]
    private class LeaderboardResponse
    {
        public List<LeaderboardEntry> items;
    }

    [System.Serializable]
    private class LeaderboardEntry
    {
        public int characterId;
        public string name;
        public int level;
        public int maxHp;
        public int attack;
        public int defense;
    }
}
