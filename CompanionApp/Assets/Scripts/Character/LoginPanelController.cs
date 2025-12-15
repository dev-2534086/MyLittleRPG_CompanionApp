using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class LoginPanelController : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public TMP_Text errorText;

    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject characterPanel;
    public GameObject bottomNavigationPanel;

    private string apiBaseUrl = "https://localhost:7029/api";

    private void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
        errorText.gameObject.SetActive(false);
    }

    private void OnLoginClicked()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        errorText.gameObject.SetActive(false);

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowError("Email et mot de passe requis");
            return;
        }

        Login(email, password);
    }

    private async void Login(string email, string password)
    {
        string url = apiBaseUrl + "/Users/login";
        var requestData = new LoginRequest { Email = email, Password = password };
        string json = JsonUtility.ToJson(requestData);

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            var op = req.SendWebRequest();
            while (!op.isDone) await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
            {
                ShowError("Erreur serveur");
                Debug.LogError(req.error);
                return;
            }

            Debug.Log("Login response: " + req.downloadHandler.text);

            SessionManager.Instance.SetEmail(email);

            // Switch panels
            loginPanel.SetActive(false);
            characterPanel.SetActive(true);
            bottomNavigationPanel.SetActive(true);
        }
    }

    private void ShowError(string message)
    {
        errorText.text = message;
        errorText.gameObject.SetActive(true);
    }

    [System.Serializable]
    private class LoginRequest
    {
        public string Email;
        public string Password;
    }
}
