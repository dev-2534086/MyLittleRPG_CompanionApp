using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class MonsterService : MonoBehaviour
{
    public string apiUrl = "https://localhost:7029/api/Monstres/Pokedex";
    public string email = "test@test.test";
    public async Task<MonsterPageResponse> GetPokemons(int offset, int limit, string type = null)
    {
        string url = $"{apiUrl}?offset={offset}&limit={limit}&email={email}";

        if (!string.IsNullOrEmpty(type)) url += $"&types={type}";

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.downloadHandler = new DownloadHandlerBuffer();
            var op = req.SendWebRequest();

            while (!op.isDone)
                await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("API ERROR: " + req.error);
                return null;
            }
            
            return JsonUtility.FromJson<MonsterPageResponse>(req.downloadHandler.text);
        }
    }
}
