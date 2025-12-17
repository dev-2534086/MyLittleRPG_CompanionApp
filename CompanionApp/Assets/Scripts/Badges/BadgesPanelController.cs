using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BadgesPanelController : MonoBehaviour
{
    [Header("UI")]
    public Transform gridParent;
    public GameObject badgeItemPrefab;

    [Header("Liste des badges à configurer dans l'inspecteur")]
    public List<BadgeData> allBadges;

    private const string apiBaseUrl = "https://localhost:7029/api";

    private void OnEnable()
    {
        StartCoroutine(UpdateBadges());
    }

    private IEnumerator UpdateBadges()
    {
        string email = SessionManager.Instance?.PlayerEmail;
        if (string.IsNullOrEmpty(email))
            yield break;

        // --- Fetch Character ---
        string charUrl = $"{apiBaseUrl}/Characters/{email}";
        using UnityWebRequest charReq = UnityWebRequest.Get(charUrl);
        yield return charReq.SendWebRequest();
        if (charReq.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Fetch character failed: " + charReq.error);
            yield break;
        }
        CharacterResponse charResp = JsonUtility.FromJson<CharacterResponse>(charReq.downloadHandler.text);
        var character = charResp.character;

        // --- Déblocage des badges ---
        var unlocked = new HashSet<string>();
        if (character.level >= 5) unlocked.Add("LEVEL_5");          // Badge 1
        if (character.level >= 10) unlocked.Add("LEVEL_10");        // Badge 2
        if (character.level >= 100) unlocked.Add("LEVEL_100");      // Badge 3
        if (character.hp >= 200) unlocked.Add("HIGH_HP");           // Badge 4
        if (character.attack >= 20) unlocked.Add("STRONG_ATTACK");  // Badge 5
        if (character.defense >= 20) unlocked.Add("HIGH_DEFENSE");  // Badge 6

        // --- Affichage badges ---
        Populate(unlocked);
    }

    private void Populate(HashSet<string> unlockedBadges)
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        foreach (var badge in allBadges)
        {
            GameObject go = Instantiate(badgeItemPrefab, gridParent);
            go.GetComponent<BadgeItemUI>().Setup(badge, unlockedBadges.Contains(badge.id));
        }
    }

    #region DTOs
    [System.Serializable]
    private class CharacterResponse
    {
        public string message;
        public CharacterData character;
    }

    [System.Serializable]
    private class CharacterData
    {
        public int characterId;
        public string name;
        public int level;
        public int xp;
        public int hp;
        public int maxHp;
        public int attack;
        public int defense;
    }
    #endregion
}
