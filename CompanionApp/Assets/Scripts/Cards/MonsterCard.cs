using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class MonsterData
{
    public int id;
    public string name;
    public string spriteUrl;
    public string type1;
    public string type2;
    public bool isHunted;
}

public class MonsterCard : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text typeText;
    public Image icon;
    public GameObject huntedBadge;

    public void Setup(MonsterData data)
    {
        nameText.text = data.name;

        if (!string.IsNullOrEmpty(data.type2))
            typeText.text = $"{data.type1} / {data.type2}";
        else
            typeText.text = data.type1;

        huntedBadge.SetActive(data.isHunted);

        StartCoroutine(LoadImage(data.spriteUrl));
    }

    private System.Collections.IEnumerator LoadImage(string url)
    {
        if (string.IsNullOrEmpty(url))
            yield break;

        using (var req = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Texture2D tex = UnityEngine.Networking.DownloadHandlerTexture.GetContent(req);
                icon.sprite = Sprite.Create(tex, new Rect(0,0,tex.width,tex.height), new Vector2(0.5f,0.5f));
            }
        }
    }
}
