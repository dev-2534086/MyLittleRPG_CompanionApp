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

    }

    public void SetEmail(string email)
    {
        PlayerEmail = email;
        logoutSent = false;
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
    }

    private void OnDisable()
    {
        Logout();
    }

    private void OnDestroy()
    {
        Logout();
    }

    private void OnApplicationQuit()
    {
        Logout();
    }

    [System.Serializable]
    private class LogoutRequestBody
    {
        public string Email;
    }
}
