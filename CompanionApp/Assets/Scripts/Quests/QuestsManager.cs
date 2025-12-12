using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

[System.Serializable]
public class QuestResponse
{
    public string message;
    public QuestsWrapper quests;
}

[System.Serializable]
public class QuestsWrapper
{
    public List<QuestTile> questTiles;
    public List<QuestLevel> questLevels;
    public List<QuestMonster> questMonsters;
}

[System.Serializable]
public class QuestBase
{
    public string title;
    public string description;
    public bool isActive;
    public bool isCompleted;
    public int goalLevel;
    public string monsterType;
    public int nbMonsterKilled;
    public int goalMonster;
    public int tilePositionX;
    public int tilePositionY;
}

[System.Serializable]
public class QuestMonster
{
    public int questMonsterId;
    public string title;
    public string description;
    public string monsterType;
    public int nbMonsterKilled;
    public int goalMonster;
    public bool isActive;
    public bool isCompleted;
    public int characterId;
}

[System.Serializable]
public class QuestLevel
{
    public int questLevelId;
    public string title;
    public string description;
    public int goalLevel;
    public bool isActive;
    public bool isCompleted;
}

[System.Serializable]
public class QuestTile
{
    public int questTileId;
    public string title;
    public string description;
    public int tilePositionX;
    public int tilePositionY;
    public bool isActive;
    public bool isCompleted;
}

public static class QuestMapper
{
    public static QuestBase FromMonster(QuestMonster qm) => new QuestBase
    {
        title = qm.title,
        description = qm.description,
        monsterType = qm.monsterType,
        nbMonsterKilled = qm.nbMonsterKilled,
        goalMonster = qm.goalMonster,
        isActive = qm.isActive,
        isCompleted = qm.isCompleted
    };

    public static QuestBase FromLevel(QuestLevel ql) => new QuestBase
    {
        title = ql.title,
        description = ql.description,
        goalLevel = ql.goalLevel,
        isActive = ql.isActive,
        isCompleted = ql.isCompleted
    };

    public static QuestBase FromTile(QuestTile qt) => new QuestBase
    {
        title = qt.title,
        description = qt.description,
        tilePositionX = qt.tilePositionX,
        tilePositionY = qt.tilePositionY,
        isActive = qt.isActive,
        isCompleted = qt.isCompleted
    };
}

public class QuestsManager : MonoBehaviour
{
    public Transform contentParent;
    public GameObject questCardPrefab;
    public TextMeshProUGUI questCountText;
    public string playerEmail = "test@test.test";
    public int refreshDelaySeconds = 600;
    public NotificationManager notificationManager;

    private int timer;
    private HashSet<string> existingQuestTitles = new HashSet<string>();

    void Start()
    {
        timer = refreshDelaySeconds;
        StartCoroutine(FetchAndRefreshQuestsRoutine());
    }

    IEnumerator FetchAndRefreshQuestsRoutine()
    {
        while (true)
        {
            yield return StartCoroutine(FetchQuests());
            while (timer > 0)
            {
                int minutes = timer / 60;
                int seconds = timer % 60;
                if (questCountText != null)
                    questCountText.text = $"{minutes:00}:{seconds:00}";
                yield return new WaitForSeconds(1f);
                timer--;
            }
            timer = refreshDelaySeconds;
        }
    }

    public IEnumerator FetchQuests()
    {
        string url = $"https://localhost:7029/api/Quests/{playerEmail}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
                yield break;

            QuestResponse response = JsonUtility.FromJson<QuestResponse>(request.downloadHandler.text);
            if (response == null || response.quests == null)
                yield break;

            List<QuestBase> finalQuests = new List<QuestBase>();
            foreach (var qm in response.quests.questMonsters)
                finalQuests.Add(QuestMapper.FromMonster(qm));
            foreach (var ql in response.quests.questLevels)
                finalQuests.Add(QuestMapper.FromLevel(ql));
            foreach (var qt in response.quests.questTiles)
                finalQuests.Add(QuestMapper.FromTile(qt));

            foreach (Transform child in contentParent)
                Destroy(child.gameObject);

            foreach (var quest in finalQuests)
            {
                GameObject card = Instantiate(questCardPrefab, contentParent);
                QuestCardUI ui = card.GetComponent<QuestCardUI>();
                ui.titleText.text = quest.title;
                ui.descriptionText.text = quest.description;

                if (quest.goalLevel > 0)
                {
                    ui.progressText.text = $"Reach Level {quest.goalLevel}";
                    ui.progressFill.fillAmount = 0f;
                }
                else if (quest.goalMonster > 0)
                {
                    int killed = quest.nbMonsterKilled;
                    int goal = quest.goalMonster;
                    ui.progressText.text = $"{killed}/{goal}";
                    ui.progressFill.fillAmount = goal > 0 ? (float)killed / goal : 0f;
                }
                else
                {
                    ui.progressText.text = $"Tile: {quest.tilePositionX}, {quest.tilePositionY}";
                    ui.progressFill.fillAmount = 1f;
                }

                if (!existingQuestTitles.Contains(quest.title))
                {
                    existingQuestTitles.Add(quest.title);
                    if (notificationManager != null)
                        notificationManager.SendQuestNotification("Nouvelle quête !", quest.title);
                }
            }

            if (questCountText != null)
                questCountText.text = finalQuests.Count.ToString();
        }
    }
}
