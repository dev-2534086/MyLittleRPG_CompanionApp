using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class CharacterPanelController : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text nameText;
    public TMP_Text levelText;
    public TMP_Text hpText;
    public TMP_Text xpText;
    public TMP_Text attackText;
    public TMP_Text defenseText;
    public GameObject characterPanel;
    public GameObject leaderboardPanel;
    public GameObject badgesPanel;

    private const string apiBaseUrl = "https://localhost:7029/api";

    private Coroutine fetchCoroutine;

    #region Unity lifecycle

    private void OnEnable()
    {
        fetchCoroutine = StartCoroutine(Init());
    }

    private void OnDisable()
    {
        if (fetchCoroutine != null)
        {
            StopCoroutine(fetchCoroutine);
            fetchCoroutine = null;
        }
    }

    #endregion

    #region Init

    private IEnumerator Init()
    {
        // Attendre que la session soit prête
        while (SessionManager.Instance == null || !SessionManager.Instance.IsLoggedIn())
            yield return null;

        string email = SessionManager.Instance.PlayerEmail;
        Debug.Log("Fetching character for: " + email);

        yield return FetchCharacter(email);
    }

    #endregion

    #region Network

    private IEnumerator FetchCharacter(string email)
    {
        string url = $"{apiBaseUrl}/Characters/{email}";

        using UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Fetch failed: " + req.error);
            yield break;
        }

        Debug.Log("Raw character JSON: " + req.downloadHandler.text);

        CharacterResponse response =
            JsonUtility.FromJson<CharacterResponse>(req.downloadHandler.text);

        if (response == null || response.character == null)
        {
            Debug.LogError("Invalid character response");
            yield break;
        }

        DisplayCharacter(response.character);
    }

    #endregion

    #region UI

    private void DisplayCharacter(CharacterData character)
    {
        nameText.text = $"Name : {character.name}";
        levelText.text = $"Level : {character.level}";

        hpText.text = $"HP : {character.hp}/{character.maxHp}";
        xpText.text = $"XP : {character.xp}";

        attackText.text = $"{character.attack} Attack";
        defenseText.text = $"{character.defense} Defense";
    }
    
    public void ShowLeaderboard()
    {
        characterPanel.SetActive(false);
        leaderboardPanel.SetActive(true);
    }

    public void ShowBadges()
    {
        characterPanel.SetActive(false);
        badgesPanel.SetActive(true);
    }

    #endregion

    #region DTOs

    [System.Serializable]
    public class CharacterData
    {
        public int characterId;
        public string name;
        public int level;
        public int xp;

        public int hp;
        public int maxHp;

        public int attack;
        public int defense;

        public int positionX;
        public int positionY;

        public int userId;
    }

    [System.Serializable]
    private class CharacterResponse
    {
        public string message;
        public CharacterData character;
    }

    #endregion
}
