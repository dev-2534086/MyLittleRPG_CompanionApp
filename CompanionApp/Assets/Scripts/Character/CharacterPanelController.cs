using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class CharacterPanelController : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text levelText;
    public TMP_Text hpText;
    public TMP_Text xpText;

    private string apiBaseUrl = "https://localhost:7029/api";

    private async void Start()
    {
        if (!SessionManager.Instance.IsLoggedIn())
        {
            Debug.LogError("No session email found");
            return;
        }

        string email = SessionManager.Instance.PlayerEmail;
        Debug.Log("Fetching character for: " + email);

        var character = await FetchCharacter(email);
        if (character != null)
            DisplayCharacter(character);
    }

    private async Task<CharacterData> FetchCharacter(string email)
    {
        string url = apiBaseUrl + "/Characters/" + email;

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            var op = req.SendWebRequest();
            while (!op.isDone) await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Fetch failed: " + req.error);
                return null;
            }

            Debug.Log("Raw character JSON: " + req.downloadHandler.text);

            CharacterResponse response =
                JsonUtility.FromJson<CharacterResponse>(req.downloadHandler.text);

            return response.character;
        }
    }

    public void DisplayCharacter(CharacterData character)
    {
        Debug.Log("Displaying character: " + JsonUtility.ToJson(character));

        nameText.text = character.name;
        levelText.text = "Level " + character.level;
        hpText.text = $"HP {character.Hp}/{character.MaxHp}";
        xpText.text = "XP " + character.xp;
    }

    [System.Serializable]
    public class CharacterData
    {
        public int characterId;
        public string name;
        public int level;
        public int xp;
        public int Hp;
        public int MaxHp;
        public int Attack;
        public int Defense;
        public int PositionX;
        public int PositionY;
        public int UserId;
    }

    [System.Serializable]
    private class CharacterResponse
    {
        public string message;
        public CharacterData character;
    }
}
