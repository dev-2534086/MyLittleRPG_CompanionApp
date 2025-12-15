using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    public string PlayerEmail { get; private set; }

    private const string apiBaseUrl = "https://localhost:7029/api";
    private bool logoutSent = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("SessionManager initialized");
    }

    public void SetEmail(string email)
    {
        PlayerEmail = email;
        logoutSent = false;
        Debug.Log("Session email set: " + PlayerEmail);
    }

    public bool IsLoggedIn()
    {
        return !string.IsNullOrEmpty(PlayerEmail);
    }

    public void Logout()
    {
        if (!IsLoggedIn())
            return;

        if (logoutSent)
            return;

        logoutSent = true;

        Debug.Log("LOGOUT TRIGGERED FOR: " + PlayerEmail);
        StartCoroutine(LogoutRequest(PlayerEmail));

        PlayerEmail = null;
    }

    private IEnumerator LogoutRequest(string email)
    {
        string url = apiBaseUrl + "/Users/logout";

        var body = new LogoutRequestBody { Email = email };
        string json = JsonUtility.ToJson(body);

        UnityWebRequest req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
            Debug.LogWarning("Logout API failed: " + req.error);
        else
            Debug.Log("Logout API SUCCESS");
    }

    // 🔥 APPELÉ QUAND TU STOP LE PLAY MODE
    private void OnDisable()
    {
        Debug.Log("SessionManager OnDisable → logout");
        Logout();
    }

    // 🔥 APPELÉ QUAND L'OBJET EST DÉTRUIT
    private void OnDestroy()
    {
        Debug.Log("SessionManager OnDestroy → logout");
        Logout();
    }

    // 🔥 EN BUILD (EXE / MOBILE)
    private void OnApplicationQuit()
    {
        Debug.Log("Application quitting → logout");
        Logout();
    }

    [System.Serializable]
    private class LogoutRequestBody
    {
        public string Email;
    }
}
