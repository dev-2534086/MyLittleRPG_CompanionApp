using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

// ---------------------------------------------------------
// 🔥 MODÈLES (compatibles avec ton JSON API)
// ---------------------------------------------------------

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

    // Level quest
    public int goalLevel;

    // Monster quest
    public string monsterType;
    public int nbMonsterKilled;
    public int goalMonster;

    // Tile quest
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

// ---------------------------------------------------------
// 🔥 MAPPER (convertit les 3 types → QuestBase)
// ---------------------------------------------------------

public static class QuestMapper
{
    public static QuestBase FromMonster(QuestMonster qm)
    {
        return new QuestBase
        {
            title = qm.title,
            description = qm.description,
            monsterType = qm.monsterType,
            nbMonsterKilled = qm.nbMonsterKilled,
            goalMonster = qm.goalMonster,
            isActive = qm.isActive,
            isCompleted = qm.isCompleted
        };
    }

    public static QuestBase FromLevel(QuestLevel ql)
    {
        return new QuestBase
        {
            title = ql.title,
            description = ql.description,
            goalLevel = ql.goalLevel,
            isActive = ql.isActive,
            isCompleted = ql.isCompleted
        };
    }

    public static QuestBase FromTile(QuestTile qt)
    {
        return new QuestBase
        {
            title = qt.title,
            description = qt.description,
            tilePositionX = qt.tilePositionX,
            tilePositionY = qt.tilePositionY,
            isActive = qt.isActive,
            isCompleted = qt.isCompleted
        };
    }
}

// ---------------------------------------------------------
// 🔥 QUEST MANAGER (FETCH + BUILD CARDS)
// ---------------------------------------------------------

public class QuestsManager : MonoBehaviour
{
    public Transform contentParent;     // Content du ScrollView
    public GameObject questCardPrefab;  // Prefab quest card
    public string playerEmail = "test@test.test";

    void Start()
    {
        StartCoroutine(FetchQuests());
    }

    IEnumerator FetchQuests()
    {
        string url = $"https://localhost:7029/api/Quests/{playerEmail}";
        Debug.Log("🌐 Envoi requête : " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ Erreur API : " + request.error);
                yield break;
            }

            string json = request.downloadHandler.text;
            Debug.Log("📩 JSON reçu : " + json);

            QuestResponse response = JsonUtility.FromJson<QuestResponse>(json);

            if (response == null || response.quests == null)
            {
                Debug.LogError("❌ Impossible de désérialiser le JSON !");
                yield break;
            }

            List<QuestBase> finalQuests = new List<QuestBase>();

            // ---------------------------------------------------------
            // 🟦 Fusionne les 3 catégories de quêtes
            // ---------------------------------------------------------
            foreach (var qm in response.quests.questMonsters)
                finalQuests.Add(QuestMapper.FromMonster(qm));

            foreach (var ql in response.quests.questLevels)
                finalQuests.Add(QuestMapper.FromLevel(ql));

            foreach (var qt in response.quests.questTiles)
                finalQuests.Add(QuestMapper.FromTile(qt));

            Debug.Log("🟢 Nombre total de quêtes : " + finalQuests.Count);

            // ---------------------------------------------------------
            // 🟩 Instantiation UI
            // ---------------------------------------------------------
            foreach (var quest in finalQuests)
            {
                GameObject card = Instantiate(questCardPrefab, contentParent);
                QuestCardUI ui = card.GetComponent<QuestCardUI>();

                if (ui == null)
                {
                    Debug.LogError("❌ QuestCardUI manquant sur le prefab !");
                    continue;
                }

                Debug.Log("🟦 Carte instanciée : " + quest.title);

                // -----------------------------------------------------
                // 📝 Textes simples
                // -----------------------------------------------------
                ui.titleText.text = quest.title;
                ui.descriptionText.text = quest.description;

                // -----------------------------------------------------
                // 📊 Logique de progression selon le type de quête
                // -----------------------------------------------------
                if (quest.goalLevel > 0)
                {
                    // Quête de niveau
                    ui.progressText.text = $"Reach Level {quest.goalLevel}";
                    ui.progressFill.fillAmount = 0f;  
                }
                else if (quest.goalMonster > 0)
                {
                    // Quête tuer des monstres
                    int killed = quest.nbMonsterKilled;
                    int goal = quest.goalMonster;

                    ui.progressText.text = $"{killed}/{goal}";
                    ui.progressFill.fillAmount = goal > 0 ? (float)killed / goal : 0f;
                }
                else
                {
                    // Quête tuile
                    ui.progressText.text = $"Tile: {quest.tilePositionX}, {quest.tilePositionY}";
                    ui.progressFill.fillAmount = 1f;
                }

                // -----------------------------------------------------
                // ⏳ Timer (pas encore utilisé)
                // -----------------------------------------------------
                ui.timerText.text = "";
            }
        }
    }
}
